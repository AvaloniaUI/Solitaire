using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.Controls;

// One progress value controls card movement, turnover, lift, and shadows.
internal sealed partial class CardMotionFlight : Animatable, IDisposable
{
    [GeneratedStyledProperty]
    public partial double Progress { get; set; }
    private readonly CardPose[] _starts;
    private readonly Action<CardMotionFlight> _finished;
    private readonly Action? _posesChanged;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly CardMotionCurve[] _curves;
    private readonly double _duration;
    private readonly List<CardShadowShape> _shapes = new();
    private readonly HashSet<CardShadowShape> _uniqueShapes = new();
    private readonly bool _packet;
    private bool _disposed;
    public CardMotionTarget[] Targets { get; }
    public CardCastShadow Shadow { get; }
    public bool Held { get; }
    internal IReadOnlyList<CardMotionCurve> Curves => _curves;

    public CardMotionFlight(CardMotionTarget[] targets, CardPoseVelocity?[] velocities, CardCastShadow shadow, bool held, bool packet,
        Action<CardMotionFlight> finished, Action? posesChanged = null)
    {
        Targets = targets;
        Shadow = shadow;
        Held = held;
        _packet = packet;
        _starts = targets.Select(t => t.Card.Pose).ToArray();
        var deal = targets.Any(t => t.Deal);
        var distance = targets.Select((t, i) => (t.Position - _starts[i].Position).Length).Max();
        var turning = targets.Where((t, i) => t.Turn != _starts[i].Turn).Any();
        _duration = (held ? 130 : Math.Clamp(150 + distance * .18, turning ? 280 : 180, deal ? 380 : 300)) / 1000;
        _curves = targets.Select((target, i) => new CardMotionCurve(_starts[i], target, velocities[i], _duration, held, deal || held)).ToArray();
        _finished = finished;
        _posesChanged = posesChanged;
        foreach (var target in Targets)
            target.Card.PropertyChanged += CardChanged;
        TableLighting.Changed += RefreshShadow;
        ApplyPose(0);
    }

    public async Task Animate(int delay, TimeSpan? duration = null)
    {
        if (_disposed)
            return;
        try
        {
            // FillMode.Both keeps the final pose while the player holds the cards.
            await CasinoAnimations.Run(CasinoAnimations.Create("CardFlightAnimation",
                duration ?? TimeSpan.FromSeconds(_duration), delay), this, _cancellation.Token);
        }
        catch (OperationCanceledException) when (_disposed) { }
        if (!_disposed && !Held)
            _finished(this);
    }

    public void CaptureVelocities(IDictionary<PlayingCard, CardPoseVelocity> velocities)
    {
        for (var i = 0; i < Targets.Length; i++)
            velocities[Targets[i].Card] = _curves[i].Velocity(Math.Clamp(Progress, 0, 1));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ProgressProperty && !_disposed)
            ApplyPose(Math.Clamp(Progress, 0, 1));
    }

    private void ApplyPose(double progress)
    {
        for (var i = 0; i < Targets.Length; i++)
        {
            var target = Targets[i];
            var pose = _curves[i].Sample(progress);
            target.Card.SetPose(Held ? pose with { Position = target.Card.Pose.Position } : pose);
        }
        RefreshShadow();
        _posesChanged?.Invoke();
    }

    private void CardChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == PlayingCard.BoundsProperty)
            RefreshShadow();
    }

    public void RefreshShadow()
    {
        if (_disposed)
            return;
        _shapes.Clear();
        _uniqueShapes.Clear();
        var bounds = default(Rect);
        var height = 0d;
        var origin = Targets[0].Card.Pose.Position;
        for (var i = 0; i < Targets.Length; i++)
        {
            var card = Targets[i].Card;
            var cardHeight = card.Pose.CenterHeight(card.Bounds.Width);
            card.SetContactFactor(_packet && i > 0 ? 1 : Math.Clamp(1 - cardHeight / 3, 0, 1));
            if (card.Bounds.Width <= 0 || card.Bounds.Height <= 0)
                continue;
            // Held cards keep their spacing. Local coordinates avoid rounding errors
            // that rebuild the same shadow after a pointer move.
            var position = Held ? _starts[i].Position - _starts[0].Position : card.Pose.Position - origin;
            var shape = (card.Pose with { Position = position }).Project(card.Bounds.Size);
            height = Math.Max(height, cardHeight);
            // Cards at the same position need only one shadow outline.
            if (!_uniqueShapes.Add(shape))
                continue;
            bounds = _shapes.Count == 0 ? shape.Bounds : bounds.Union(shape.Bounds);
            _shapes.Add(shape);
        }
        if (_shapes.Count > 0)
            Shadow.UpdateShape(_shapes, bounds, height, origin);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _cancellation.Cancel();
        _cancellation.Dispose();
        foreach (var target in Targets)
            target.Card.PropertyChanged -= CardChanged;
        TableLighting.Changed -= RefreshShadow;
    }
}
