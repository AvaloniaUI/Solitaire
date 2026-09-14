using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Solitaire.Controls;

internal sealed class CardMotionController : IDisposable
{
    private readonly Canvas _canvas;
    private readonly Action _changed;
    private readonly CardStackVisibility _visibility = new();
    private readonly List<CardMotionFlight> _flights = new();
    private readonly Dictionary<PlayingCard, CardMotionTarget> _targets = new();
    private readonly HashSet<PlayingCard> _pending = new();
    private readonly Dictionary<PlayingCard, CardPoseVelocity> _velocities = new();
    private bool _queued;
    private DispatcherOperation? _flushOperation;
    private bool _disposed;
    private int _sequence;

    public CardMotionController(Canvas canvas, Action changed)
    {
        _canvas = canvas;
        _changed = changed;
    }

    public void Place(PlayingCard card, Vector position, bool faceDown)
    {
        if (_disposed)
            return;
        _velocities.Remove(card);
        var turn = faceDown ? 0d : 1d;
        _targets[card] = new CardMotionTarget(card, position, turn, false);
        card.SetPose(new CardPose(position, turn, 0));
    }

    public void Move(PlayingCard card, Vector position, bool deal)
    {
        if (_disposed)
            return;
        var previous = _targets[card];
        if (previous.Position == position && !card.Classes.Contains("dragging"))
            return;
        _targets[card] = previous with { Position = position, Deal = deal };
        if (!card.Classes.Contains("dragging"))
            Queue(card);
    }

    public void Reveal(PlayingCard card, bool faceDown)
    {
        if (_disposed)
            return;
        var target = _targets[card];
        var turn = faceDown ? 0d : 1d;
        if (target.Turn == turn)
            return;
        _targets[card] = target with { Turn = turn };
        Queue(card);
    }

    private void Queue(PlayingCard card)
    {
        _pending.Add(card);
        if (_queued || _disposed)
            return;
        _queued = true;
        // Apply face and pile changes together before the next frame.
        _flushOperation = Dispatcher.UIThread.InvokeAsync(Flush, DispatcherPriority.Render);
    }

    internal IReadOnlyList<CardMotionFlight> Flights => _flights;
    internal void RefreshVisibility() => _visibility.Update(_targets.Keys);

    internal void Flush()
    {
        _flushOperation?.Abort();
        _flushOperation = null;
        _queued = false;
        if (_disposed || _pending.Count == 0)
            return;
        Interrupt(_pending.ToArray());
        var targets = _pending.Select(c => _targets[c]).ToArray();
        _pending.Clear();
        var dealIndex = 0;
        foreach (var group in targets.GroupBy(t => (t.Deal, Delta: t.Position - t.Card.Pose.Position)))
        {
            if (group.Key.Deal)
                foreach (var target in group)
                    Start(new[] { target }, false, false, dealIndex++ * 45);
            else
            {
                var moving = group.OrderBy(t => t.Card.StackOrder).ToArray();
                var packet = moving.Length > 1 && moving.Zip(moving.Skip(1)).All(pair =>
                    new Rect((Point)pair.First.Card.Pose.Position, pair.First.Card.Bounds.Size).Intersects(
                        new Rect((Point)pair.Second.Card.Pose.Position, pair.Second.Card.Bounds.Size)));
                Start(moving, false, packet, 0);
            }
        }
    }

    private void Interrupt(PlayingCard[] cards)
    {
        foreach (var flight in _flights.Where(f => f.Targets.Any(t => cards.Contains(t.Card))).ToArray())
        {
            foreach (var target in flight.Targets)
                _pending.Add(target.Card);
            flight.CaptureVelocities(_velocities);
            Remove(flight, false);
        }
    }

    public void Lift(IEnumerable<PlayingCard> cards)
    {
        if (_disposed)
            return;
        var packet = cards.ToArray();
        Interrupt(packet);
        foreach (var card in packet)
            _pending.Remove(card);
        if (packet.Length > 0)
            Start(packet.Select(c => _targets[c] with { Deal = false }).ToArray(), true, true, 0);
        if (_pending.Count > 0)
            Flush();
    }

    public void Drag(IReadOnlyList<PlayingCard> cards, IReadOnlyList<Vector> origins, Vector delta)
    {
        if (_disposed)
            return;
        for (var i = 0; i < cards.Count; i++)
            cards[i].SetPose(cards[i].Pose with { Position = origins[i] + delta });
        foreach (var flight in _flights)
            if (flight.Held)
                flight.RefreshShadow();
        RefreshVisibility();
    }

    public void Release()
    {
        if (_disposed)
            return;
        foreach (var card in _flights.Where(f => f.Held).SelectMany(f => f.Targets).Select(t => t.Card).ToArray())
            Queue(card);
    }

    private void Start(CardMotionTarget[] targets, bool held, bool packet, int delay)
    {
        var order = 1_000_000 + ++_sequence * 128;
        var shadow = new CardCastShadow { ZIndex = order };
        for (var i = 0; i < targets.Length; i++)
            targets[i].Card.FloatAt(order + 1 + i);
        _canvas.Children.Add(shadow);
        var velocities = new CardPoseVelocity?[targets.Length];
        for (var i = 0; i < targets.Length; i++)
            if (_velocities.Remove(targets[i].Card, out var velocity))
                velocities[i] = velocity;
        var flight = new CardMotionFlight(targets, velocities, shadow, held, packet, Finish, RefreshVisibility);
        _flights.Add(flight);
        _changed();
        _ = flight.Animate(delay);
    }

    private void Finish(CardMotionFlight flight)
    {
        foreach (var target in flight.Targets)
            if (!_pending.Contains(target.Card))
                _targets[target.Card] = target with { Deal = false };
        Remove(flight, true);
    }

    private void Remove(CardMotionFlight flight, bool land)
    {
        flight.Dispose();
        if (land)
            foreach (var target in flight.Targets)
                target.Card.Land();
        _canvas.Children.Remove(flight.Shadow);
        _flights.Remove(flight);
        RefreshVisibility();
        _changed();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _flushOperation?.Abort();
        _flushOperation = null;
        _queued = false;
        _pending.Clear();
        foreach (var flight in _flights.ToArray())
            Remove(flight, true);
        foreach (var card in _targets.Keys)
            card.ResetMotion();
        _targets.Clear();
        _velocities.Clear();
    }
}
