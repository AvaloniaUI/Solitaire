using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Solitaire.Behaviors;
using Solitaire.Utils;

namespace Solitaire.Controls;

// Keep one frame across page changes. Each page controls its content layout.
public sealed partial class ConnectedNavigationHost : Panel, IDisposable
{
    [GeneratedStyledProperty]
    public partial object? Content { get; set; }
    private readonly Canvas _frameLayer = new() { ClipToBounds = true, IsHitTestVisible = false };
    private readonly AnimatedTrayFrame _frame = new()
    {
        Name = "PART_ConnectedFrame",
        IsHitTestVisible = false,
        IsVisible = false,
        UseLayoutRounding = false,
        RenderTransformOrigin = RelativePoint.TopLeft
    };
    private CancellationTokenSource? _animationCancellation;
    private MenuChoreography? _currentMotion;
    private MenuChoreography? _previousMotion;
    private readonly ViewLocator _locator = new();
    private Control? _current;
    private Control? _previous;
    private ConnectedTray? _currentTray;
    private ConnectedTray? _previousTray;
    private ConnectedFrameState _start;
    private ConnectedFrameState _target;
    private Size _viewport;
    private double _outgoingOpacity;
    private bool _pending;
    private bool _animating;
    private bool _sourceConnected;
    private bool _attached;
    internal Task TransitionTask { get; private set; } = Task.CompletedTask;
    internal TimeSpan? TransitionDuration { get; init; }

    public ConnectedNavigationHost()
    {
        _frameLayer.Children.Add(_frame);
        Children.Add(_frameLayer);
        LayoutUpdated += UpdateDestination;
        AddHandler(KeyDownEvent, BlockTransitionKeys, RoutingStrategies.Tunnel);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
            Navigate(change.NewValue);
    }

    private void Navigate(object? content)
    {
        ResetCardViews(_current);
        StopAnimation();
        RemovePrevious();
        if (!_frame.IsVisible && TryGetDestination(out var source))
        {
            FrameTransitions.Place(_frame, source);
            _frame.IsVisible = true;
            _currentTray!.FrameVisible = false;
        }
        _sourceConnected = _frame.IsVisible;
        _start = _frame.State;
        _previous = _current;
        _previousMotion = _previous is null ? null : MenuChoreography.Prepare(_previous, false);
        _currentMotion?.Dispose();
        _currentMotion = null;
        _previousTray = _currentTray;
        _outgoingOpacity = _previous?.Opacity ?? 0;
        _current = content as Control ?? _locator.Build(content) as Control;
        _currentTray = null;
        if (_current is null)
        {
            _pending = false;
            Finish();
            return;
        }
        if (content is not Control)
            _current.DataContext = content;
        _current.Opacity = _sourceConnected ? 0 : 1;
        if (_sourceConnected)
            _currentMotion = MenuChoreography.Prepare(_current, true);
        Children.Add(_current);
        _pending = true;
        IsHitTestVisible = false;
    }

    private void UpdateDestination(object? sender, EventArgs e)
    {
        if (!_attached || _current is null)
            return;
        if (!_pending && !_animating && _currentTray is { UseLocalFrameWhenIdle: true })
            return;
        if (_pending)
        {
            _currentTray = _current.GetVisualDescendants().OfType<ConnectedTray>().FirstOrDefault();
            if (_currentTray is null)
            {
                _frame.IsVisible = false;
                _pending = false;
                Finish();
                return;
            }
        }
        if (!TryGetDestination(out var bounds))
            return;
        _target = bounds;
        if (_pending)
            BeginTransition();
        else if (_animating && _viewport != Bounds.Size)
            Finish();
        else if (!_animating)
            FrameTransitions.Place(_frame, _target);
    }

    private bool TryGetDestination(out ConnectedFrameState state)
    {
        state = default;
        if (_currentTray is null || _currentTray.Bounds.Width <= 0 || _currentTray.Bounds.Height <= 0 ||
            _currentTray.TransformToVisual(_frameLayer) is not { } transform)
            return false;
        var scale = Math.Abs(transform.M11);
        if (scale <= 0)
            return false;
        state = new ConnectedFrameState(new Rect(_currentTray.Bounds.Size).TransformToAABB(transform),
            scale, _currentTray.FrameShadow);
        return true;
    }

    private async void BeginTransition()
    {
        _pending = false;
        _currentTray!.FrameVisible = false;
        _frame.IsVisible = true;
        if (!_sourceConnected || _previous is null)
        {
            Finish();
            return;
        }
        _viewport = Bounds.Size;
        _animating = true;
        FrameTransitions.Place(_frame, _start);
        _animationCancellation = new CancellationTokenSource();
        var token = _animationCancellation.Token;
        TransitionTask = Task.WhenAll(
            CasinoAnimations.Connect(_start, _target, _frame, token, TransitionDuration),
            CasinoAnimations.Run(CasinoAnimations.FadeOut(_outgoingOpacity, TransitionDuration), _previous, token),
            CasinoAnimations.Run(CasinoAnimations.Create("PageEnterAnimation", TransitionDuration), _current!, token),
            _previousMotion?.Run(token, TransitionDuration) ?? Task.CompletedTask,
            _currentMotion?.Run(token, TransitionDuration) ?? Task.CompletedTask);
        await TransitionTask;
        if (!token.IsCancellationRequested)
            Finish();
    }

    private void StopAnimation()
    {
        var displayed = _frame.State;
        var currentOpacity = _current?.Opacity;
        _animating = false;
        _currentMotion?.Freeze();
        _previousMotion?.Freeze();
        _animationCancellation?.Cancel();
        _animationCancellation?.Dispose();
        _animationCancellation = null;
        FrameTransitions.Place(_frame, displayed);
        if (_current is { } current && currentOpacity is { } opacity)
            current.Opacity = opacity;
    }

    private void Finish()
    {
        StopAnimation();
        RemovePrevious();
        _currentMotion?.Dispose();
        _currentMotion = null;
        if (_current is { } current)
            current.Opacity = 1;
        if (_currentTray is { } tray)
        {
            tray.Clip = null;
            FrameTransitions.Place(_frame, _target);
            tray.FrameVisible = tray.UseLocalFrameWhenIdle;
            _frame.IsVisible = !tray.UseLocalFrameWhenIdle;
        }
        else
            _frame.IsVisible = false;
        IsHitTestVisible = true;
    }

    private static void ResetCardViews(Control? view)
    {
        if (view is null)
            return;
        foreach (var canvas in view.GetVisualDescendants().OfType<Canvas>())
            foreach (var field in Interaction.GetBehaviors(canvas).OfType<CardFieldBehavior>())
                field.ResetView();
    }

    private void RemovePrevious()
    {
        if (_previousTray is { } tray)
        {
            tray.Clip = null;
            tray.FrameVisible = true;
        }
        if (_previous is { } previous)
            Children.Remove(previous);
        _previousMotion?.Dispose();
        _previousMotion = null;
        _previous = null;
        _previousTray = null;
    }

    private void BlockTransitionKeys(object? sender, KeyEventArgs e)
    {
        if (_animating || _pending)
            e.Handled = true;
    }

    public void Dispose() => Finish();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        if (_current is null && Content is not null)
            Navigate(Content);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        _pending = false;
        ResetCardViews(_current);
        Finish();
        if (_currentTray is { } tray)
            tray.FrameVisible = true;
        if (_current is { } current)
            Children.Remove(current);
        _current = null;
        _currentTray = null;
        _frame.IsVisible = false;
        base.OnDetachedFromVisualTree(e);
    }
}
