using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Solitaire.Models;

namespace Solitaire.Controls;

// A circular slider with the standard keyboard and accessibility support.
public sealed partial class LightDirectionDial : Slider
{
    [GeneratedStyledProperty(DefaultValue = TableLighting.DefaultElevation)]
    public partial double Elevation { get; set; }

    private IPointer? _dragPointer;

    static LightDirectionDial()
    {
        // Set the range before a two-way binding can clamp the saved direction.
        MaximumProperty.OverrideDefaultValue<LightDirectionDial>(360);
        ValueProperty.OverrideDefaultValue<LightDirectionDial>(TableLighting.DefaultDirection);
    }

    private void MoveLight(Point position)
    {
        var relative = position - new Point(Bounds.Width / 2, Bounds.Height / 2);
        // Near the center, small movements cause large angle changes.
        if (relative.X * relative.X + relative.Y * relative.Y < 144)
            return;
        var angle = Math.Atan2(relative.X, -relative.Y) * 180 / Math.PI;
        SetCurrentValue(ValueProperty, (angle + 360) % 360);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        Focus();
        _dragPointer = e.Pointer;
        e.Pointer.Capture(this);
        MoveLight(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragPointer != e.Pointer)
            return;
        MoveLight(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_dragPointer != e.Pointer)
            return;
        MoveLight(e.GetPosition(this));
        _dragPointer = null;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _dragPointer = null;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var step = e.Key switch
        {
            Key.Left or Key.Down => -SmallChange,
            Key.Right or Key.Up => SmallChange,
            Key.PageDown => -LargeChange,
            Key.PageUp => LargeChange,
            _ => 0
        };
        if (step == 0)
        { base.OnKeyDown(e); return; }
        SetCurrentValue(ValueProperty, (Value + step + 360) % 360);
        e.Handled = true;
    }
}
