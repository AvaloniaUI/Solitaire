using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace Solitaire.Controls;

// Animation-priority bindings preserve the control's own styles and local values.
internal sealed class MenuMotionPart : IDisposable
{
    private readonly CompositeDisposable _bindings = new();
    private readonly BehaviorSubject<double> _opacity;
    private readonly MatrixTransform _transform = new();
    private readonly Matrix _baseTransform;
    private readonly double _baseOpacity;
    private readonly Vector _initialOffset;
    private readonly double _initialAlpha;

    public MenuMotionPart(Control control, Panel owner, bool entering)
    {
        Control = control;
        Center = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), owner) ?? control.Bounds.Center;
        _baseTransform = control.RenderTransform?.Value ?? Matrix.Identity;
        _baseOpacity = control.Opacity;
        _opacity = new BehaviorSubject<double>(_baseOpacity);
        _bindings.Add(_opacity);
        _bindings.Add(control.Bind(Visual.OpacityProperty, _opacity, BindingPriority.Animation));
        _bindings.Add(control.SetValue(Visual.RenderTransformProperty, _transform, BindingPriority.Animation)!);
        _bindings.Add(control.SetValue(Visual.RenderTransformOriginProperty, RelativePoint.Center, BindingPriority.Animation)!);
        _initialOffset = entering ? new Vector(0, 18) : default;
        _initialAlpha = entering ? 0 : 1;
        Place(_initialOffset, 1, _initialAlpha);
    }

    public Control Control { get; }
    public Point Center { get; }
    public void Arrive(double progress) => Place(_initialOffset * (1 - progress),
        1, _initialAlpha + (1 - _initialAlpha) * progress);

    public void Depart(double progress, bool selected, Vector convergence) => Place(
        _initialOffset + convergence * progress,
        1 + (selected ? .1 * progress : 0), _initialAlpha * (1 - progress));

    private void Place(Vector offset, double scale, double alpha)
    {
        _transform.Matrix = _baseTransform * Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offset);
        _opacity.OnNext(_baseOpacity * alpha);
    }

    public void Dispose() => _bindings.Dispose();
}
