using Avalonia.Styling;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Solitaire.Models;

namespace Solitaire.Controls;

public partial class PlayingCard : ContentControl
{
    protected override Type StyleKeyOverride => typeof(ContentControl);
    [GeneratedDirectProperty]
    public partial BoxShadows ContactShadow { get; private set; }

    [GeneratedDirectProperty]
    public partial double ContactShadowOpacity { get; private set; } = 1;

    [GeneratedDirectProperty]
    public partial bool DisplayFaceUp { get; private set; }
    private readonly MatrixTransform _poseTransform = new();
    internal CardPose Pose { get; private set; }

    public PlayingCard()
    {
        RenderTransformOrigin = RelativePoint.TopLeft;
        RenderTransform = _poseTransform;
    }

    internal void SetPose(CardPose pose)
    {
        Pose = pose;
        _poseTransform.Matrix = pose.Artwork(Bounds.Size);
        DisplayFaceUp = pose.Turn >= .5;
    }

    internal void ResetMotion()
    {
        Classes.Remove("dragging");
        Classes.Remove("lastCard");
        _stackOrder = 0;
        _contactExposed = true;
        IsFloating = false;
        ZIndex = 0;
        SetContactFactor(1);
        SetPose(new CardPose(default, 0, 0));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
            SetPose(Pose);
    }
    private bool _contactExposed = true;
    private double _contactFactor = 1;
    private int _stackOrder;
    private bool _lightingAttached;
    internal int StackOrder => _stackOrder;
    internal bool IsFloating { get; private set; }

    internal void SetContactExposed(bool exposed)
    {
        _contactExposed = exposed;
        UpdateContactOpacity();
    }

    internal void SetContactFactor(double factor)
    {
        _contactFactor = factor;
        UpdateContactOpacity();
    }

    private void UpdateContactOpacity() => ContactShadowOpacity = _contactExposed ? _contactFactor : 0;
    private void UpdateLighting() => ContactShadow = TableLighting.ContactShadow();

    internal void SetStackOrder(int order)
    {
        _stackOrder = order;
        if (!IsFloating)
            ZIndex = order;
    }

    internal void FloatAt(int order)
    {
        IsFloating = true;
        ZIndex = order;
    }

    internal void Land()
    {
        IsFloating = false;
        ZIndex = _stackOrder;
        SetContactFactor(1);
    }



    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_lightingAttached)
            return;
        _lightingAttached = true;
        TableLighting.Changed += UpdateLighting;
        UpdateLighting();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _lightingAttached = false;
        TableLighting.Changed -= UpdateLighting;
        base.OnDetachedFromVisualTree(e);
    }

}
