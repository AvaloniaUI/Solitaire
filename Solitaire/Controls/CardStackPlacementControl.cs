using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Solitaire.Behaviors;
using Solitaire.Models;
using Solitaire.ViewModels;
using Solitaire.ViewModels.Pages;

namespace Solitaire.Controls;

public class CardStackPlacementControl : Border
{
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _targetStacksMetaData = CardFieldBehavior.GetCardStacks(TargetCanvas);
        OnPropertyChanged(null);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs? change)
    {
        if (_targetStacksMetaData is null) return;

        var prevStacksMeta = _targetStacksMetaData.FirstOrDefault(x => x.Name == Name);
        if (prevStacksMeta is not null) _targetStacksMetaData.Remove(prevStacksMeta);

        var u = GetStacksMetadata;

        if (u is null) return;
        _targetStacksMetaData.Add(u);


        base.OnPropertyChanged(change);
    }

    public StacksMetadata? GetStacksMetadata
    {
        get
        {
            if (Name is null || Bounds.Position == new Point() || SourceItems is null)
            {
                return null;
            }

            return
                new StacksMetadata(Name, Bounds.Position, SourceItems, FaceDownOffset ?? default,
                    FaceUpOffset ?? default, OffsetMode ?? default, CommandOnCardClick ?? default, NValue ?? default,
                    IsHomeStack);
        }
    }

    /// <inheritdoc />
    protected override void OnUnloaded()
    {
        if (_targetStacksMetaData is null) return;
        _targetStacksMetaData = null;
        base.OnUnloaded();
    }

    public static readonly StyledProperty<ObservableCollection<PlayingCardViewModel>?> SourceItemsProperty =
        AvaloniaProperty
            .Register<CardStackPlacementControl, ObservableCollection<PlayingCardViewModel>?>("SourceItems");

    public static readonly StyledProperty<Canvas> TargetCanvasProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, Canvas>("TargetCanvas");

    public static readonly StyledProperty<Orientation?> OrientationProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, Orientation?>("Orientation");

    public static readonly StyledProperty<double?> FaceDownOffsetProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, double?>("FaceDownOffset");

    public static readonly StyledProperty<double?> FaceUpOffsetProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, double?>("FaceUpOffset");

    public static readonly StyledProperty<OffsetMode?> OffsetModeProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, OffsetMode?>("OffsetMode");

    public static readonly StyledProperty<ICommand?> CommandOnCardClickProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, ICommand?>("CommandOnCardClick");

    public static readonly StyledProperty<DrawMode?> NValueProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, DrawMode?>("NValue");

    private ObservableCollection<StacksMetadata>? _targetStacksMetaData;

    public static readonly StyledProperty<bool> IsHomeStackProperty =
        AvaloniaProperty.Register<CardStackPlacementControl, bool>("IsHomeStack");

    private StacksMetadata? _getStacksMetadata;

    public ObservableCollection<PlayingCardViewModel>? SourceItems
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

    public DrawMode? NValue
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