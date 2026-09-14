using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Solitaire.Utils;

namespace Solitaire.Controls;

// Animate the menu content separately from the page frame.
internal sealed partial class MenuChoreography : Animatable, IDisposable
{
    [GeneratedStyledProperty]
    public partial double Progress { get; set; }
    private static readonly CasinoSCurve Ease = new();
    private readonly MenuMotionPart[] _parts;
    private readonly MenuMotionPart[] _decoration;
    private readonly bool _entering;
    private readonly MenuMotionPart? _selected;
    private bool _frozen;

    private MenuChoreography(Control page, Control[] controls, Panel owner, bool entering, Control? selected)
    {
        _entering = entering;
        _parts = controls.Select(control => new MenuMotionPart(control, owner, entering)).ToArray();
        _decoration = page.GetLogicalDescendants().OfType<Viewbox>()
            .Select(control => new MenuMotionPart(control, owner, entering)).ToArray();
        _selected = _parts.FirstOrDefault(part => ReferenceEquals(part.Control, selected));
    }

    public static MenuChoreography? Prepare(Control page, bool entering)
    {
        var tray = page.GetLogicalDescendants().OfType<ConnectedTray>().FirstOrDefault();
        if (tray is null || tray.UseLocalFrameWhenIdle || tray.Content is not Panel panel)
            return null;
        var controls = panel.Children.SelectMany(child => child is WrapPanel wrap ? wrap.Children.ToArray() : new[] { child })
            .Where(child => child.IsVisible).ToArray();
        var selected = controls.FirstOrDefault(control => control is CasinoButton { IsActivating: true } ||
            control.GetLogicalDescendants().OfType<CasinoButton>().Any(button => button.IsActivating));
        return new MenuChoreography(page, controls, panel, entering, selected);
    }

    public Task Run(CancellationToken token, TimeSpan? duration = null) => CasinoAnimations.Run(
        CasinoAnimations.Create("MenuMotionAnimation", duration), this, token);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != ProgressProperty || _frozen)
            return;
        // The title artwork clears the moving frame before it crosses that area.
        foreach (var decoration in _decoration)
        {
            if (_entering)
                decoration.Arrive(Ease.Ease(Math.Clamp((Progress - .7) / .3, 0, 1)));
            else
                decoration.Depart(Math.Clamp(Progress / .2, 0, 1), false, default);
        }
        for (var index = 0; index < _parts.Length; index++)
        {
            var part = _parts[index];
            if (_entering)
            {
                var delay = .1 + Math.Min(index * .055, .34);
                part.Arrive(Ease.Ease(Math.Clamp((Progress - delay) / .55, 0, 1)));
            }
            else
            {
                var selected = ReferenceEquals(part, _selected);
                var delta = _selected is null ? new Vector(0, -12) :
                    (_selected.Center - part.Center) * .16;
                var length = delta.Length;
                if (length > 30)
                    delta *= 30 / length;
                part.Depart(Ease.Ease(Math.Clamp(Progress / (selected ? .65 : .8), 0, 1)), selected, selected ? default : delta);
            }
        }
    }

    public void Freeze() => _frozen = true;

    public void Dispose()
    {
        _frozen = true;
        foreach (var part in _parts)
            part.Dispose();
        foreach (var decoration in _decoration)
            decoration.Dispose();
    }
}
