using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Solitaire.Behaviors;
using Solitaire.Models;
using Solitaire.Utils;
using Solitaire.ViewModels;

namespace Solitaire.Controls;

public class CardStackPlacementControl : Border
{
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var stacks = PlayFieldBehavior.GetCardStacks(this);
        stacks.Add(this);
        base.OnAttachedToVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var stacks = PlayFieldBehavior.GetCardStacks(this);
        stacks.Remove(this);
        base.OnDetachedFromVisualTree(e);
    }

    public static readonly StyledProperty<BatchObservableCollection<PlayingCardViewModel>?> SourceItemsProperty =
        AvaloniaProperty
            .Register<CardStackPlacementControl, BatchObservableCollection<PlayingCardViewModel>?>("SourceItems");

    public static readonly StyledProperty<Canvas> TargetCanvasProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, Canvas>("TargetCanvas");

    public static readonly StyledProperty<Orientation?> OrientationProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, Orientation?>("Orientation",
            Avalonia.Layout.Orientation.Vertical);

    public static readonly StyledProperty<double?> FaceDownOffsetProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, double?>("FaceDownOffset");

    public static readonly StyledProperty<double?> FaceUpOffsetProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, double?>("FaceUpOffset");

    public static readonly StyledProperty<OffsetMode?> OffsetModeProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, OffsetMode?>("OffsetMode", Controls.OffsetMode.EveryCard);

    public static readonly StyledProperty<ICommand?> CommandOnCardClickProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, ICommand?>("CommandOnCardClick");

    public static readonly StyledProperty<DrawMode> NValueProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, DrawMode>("NValue");

    public static readonly StyledProperty<bool> IsHomeStackProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, bool>("IsHomeStack");

    public BatchObservableCollection<PlayingCardViewModel>? SourceItems
    {
        get => GetValue(SourceItemsProperty);
        set => SetValue(SourceItemsProperty, value);
    }

    public Canvas TargetCanvas
    {
        get => GetValue(TargetCanvasProperty);
        set => SetValue(TargetCanvasProperty, value);
    }

    public Orientation? Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double? FaceDownOffset
    {
        get => GetValue(FaceDownOffsetProperty);
        set => SetValue(FaceDownOffsetProperty, value);
    }

    public double? FaceUpOffset
    {
        get => GetValue(FaceUpOffsetProperty);
        set => SetValue(FaceUpOffsetProperty, value);
    }

    public OffsetMode? OffsetMode
    {
        get => GetValue(OffsetModeProperty);
        set => SetValue(OffsetModeProperty, value);
    }

    public ICommand? CommandOnCardClick
    {
        get => GetValue(CommandOnCardClickProperty);
        set => SetValue(CommandOnCardClickProperty, value);
    }

    public DrawMode NValue
    {
        get => GetValue(NValueProperty);
        set => SetValue(NValueProperty, value);
    }

    public bool IsHomeStack
    {
        get => GetValue(IsHomeStackProperty);
        set => SetValue(IsHomeStackProperty, value);
    }
}