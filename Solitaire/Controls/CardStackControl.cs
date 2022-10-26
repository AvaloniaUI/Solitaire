using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using ReactiveUI;
using Solitaire.Utils;
using Solitaire.ViewModels;

namespace Solitaire.Controls;

public class CardStackControl : TemplatedControl, IStyleable
{
    public CardStackControl()
    {
        if (Application.Current != null &&
            Application.Current.Styles.TryGetResource("PlayingCardDataTemplate", out var obj) &&
            obj is DataTemplate dataTemplate)
        {
            _dataTemplate = dataTemplate;
        }

        this.WhenAnyValue(x => x.SourceItems)
            .Do(x =>
            {
                if (x is { })
                    x.CollectionChanged += XOnCollectionChanged;
            })
            .Subscribe();
    }

    protected override void OnLoaded()
    {
        foreach (var keyValues in _containers)
        {
            SetContainerLayout(keyValues.Key, keyValues.Value);
        }

        base.OnLoaded();
    }

    private readonly Dictionary<PlayingCardViewModel, ContentControl> _containers = new();

    private void XOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (TargetCanvas is null || _dataTemplate is null) return;
        
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

                if (e.NewItems is { })
                    foreach (var newItem in e.NewItems.Cast<PlayingCardViewModel>())
                    {
                        var cachedContainer = new ContentControl()
                        {
                            DataTemplates = { _dataTemplate },
                            Content = newItem
                        };

                        newItem.PropertyChanged += NewItemOnPropertyChanged;

                        SetContainerLayout(newItem, cachedContainer);

                        _containers.Add(newItem, cachedContainer);

                        RegisterEvents(cachedContainer);

                        TargetCanvas.Children.Add(cachedContainer);
                    }

                break;
            case NotifyCollectionChangedAction.Remove:

                if (e.OldItems is { })
                    foreach (var oldItem in e.OldItems.Cast<PlayingCardViewModel>())
                    {
                        var container = _containers[oldItem];
                        oldItem.PropertyChanged -= NewItemOnPropertyChanged;
                        TargetCanvas.Children.Remove(container);
                        _containers.Remove(oldItem);
                        UnregisterEvents(container);
                    }

                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:
                break;
        }
    }

    private void RegisterEvents(ContentControl container)
    {
        container.PointerPressed += ContainerOnPointerPressed;
    }

    private void UnregisterEvents(ContentControl container)
    {
        container.PointerPressed -= ContainerOnPointerPressed;
    }

    private void ContainerOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (CommandOnCardClick is { } && CommandOnCardClick.CanExecute(null))
        {
            CommandOnCardClick.Execute(null);
            return;
        }
        
        
        
    }

    private void NewItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayingCardViewModel.IsFaceDown) && sender is PlayingCardViewModel card)
        {
            SetContainerLayout(card, _containers[card]);
        }
    }

    private void SetContainerLayout(PlayingCardViewModel card, ContentControl cachedContainer)
    {
        if (SourceItems is null) return;

        var cardIndex = SourceItems.IndexOf(card);
        var totalItems = SourceItems.Count;
        GetOffsets(card, cardIndex, totalItems, out var faceDownOffset, out var faceUpOffset);

        var selectedOffset = card.IsFaceDown ? faceDownOffset : faceUpOffset;

        double totalOffset;

        if (cardIndex > 0)
        {
            totalOffset = 1;
        }
        else
        {
            totalOffset = selectedOffset;
        }
        
        if (Orientation == Orientation.Horizontal)
        {
            Canvas.SetLeft(cachedContainer, Bounds.Left + totalOffset);
            Canvas.SetTop(cachedContainer, Bounds.Top);
        }
        else
        {
            Canvas.SetLeft(cachedContainer, Bounds.Left);
            Canvas.SetTop(cachedContainer, Bounds.Top + totalOffset);
        }
    }


    private void GetOffsets(PlayingCardViewModel card, int n, int total, out double faceDownOffset,
        out double faceUpOffset)

    {
        faceDownOffset = 0;
        faceUpOffset = 0;

        //  We are now going to offset only if the offset mode is appropriate.
        switch (OffsetMode)
        {
            case OffsetMode.EveryCard:
                //  Offset every card.
                faceDownOffset = FaceDownOffset;
                faceUpOffset = FaceUpOffset;
                break;
            case OffsetMode.EveryNthCard:
                //  Offset only if n Mod N is zero.
                if ((n + 1) % NValue == 0)
                {
                    faceDownOffset = FaceDownOffset;
                    faceUpOffset = FaceUpOffset;
                }

                break;
            case OffsetMode.TopNCards:
                //  Offset only if (Total - N) <= n < Total
                if (total - NValue <= n && n < total)
                {
                    faceDownOffset = FaceDownOffset;
                    faceUpOffset = FaceUpOffset;
                }

                break;
            case OffsetMode.BottomNCards:
                //  Offset only if 0 < n < N
                if (n < NValue)
                {
                    faceDownOffset = FaceDownOffset;
                    faceUpOffset = FaceUpOffset;
                }

                break;
            case OffsetMode.UseCardValues:
                //  Offset each time by the amount specified in the card object.
                faceDownOffset = card.FaceDownOffset;
                faceUpOffset = card.FaceUpOffset;
                break;
        }
    }


    public static readonly StyledProperty<Canvas?> TargetCanvasProperty =
        AvaloniaProperty.Register<CardStackControl, Canvas?>(
            "TargetCanvas");

    public Canvas? TargetCanvas
    {
        get => GetValue(TargetCanvasProperty);
        set => SetValue(TargetCanvasProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<PlayingCardViewModel>?> SourceItemsProperty =
        AvaloniaProperty.Register<CardStackControl, ObservableCollection<PlayingCardViewModel>?>(
            "SourceItems");

    public ObservableCollection<PlayingCardViewModel>? SourceItems
    {
        get => GetValue(SourceItemsProperty);
        set => SetValue(SourceItemsProperty, value);
    }

    Type IStyleable.StyleKey => typeof(CardStackControl);

    public static readonly StyledProperty<double> FaceDownOffsetProperty =
        AvaloniaProperty.Register<CardStackControl, double>(
            "FaceDownOffset");

    public double FaceDownOffset
    {
        get => GetValue(FaceDownOffsetProperty);
        set => SetValue(FaceDownOffsetProperty, value);
    }

    public static readonly StyledProperty<double> FaceUpOffsetProperty =
        AvaloniaProperty.Register<CardStackControl, double>(
            "FaceUpOffset");

    public double FaceUpOffset
    {
        get => GetValue(FaceUpOffsetProperty);
        set => SetValue(FaceUpOffsetProperty, value);
    }

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CardStackControl, Orientation>(
            "Orientation");

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }


    public static readonly StyledProperty<OffsetMode> OffsetModeProperty =
        AvaloniaProperty.Register<CardStackControl, OffsetMode>(
            "OffsetMode");

    public OffsetMode OffsetMode
    {
        get => GetValue(OffsetModeProperty);
        set => SetValue(OffsetModeProperty, value);
    }

    public static readonly StyledProperty<int> NValueProperty = AvaloniaProperty.Register<CardStackControl, int>(
        "NValue", 1);

    private readonly DataTemplate? _dataTemplate;

    public static readonly StyledProperty<ICommand?> CommandOnCardClickProperty =
        AvaloniaProperty.Register<CardStackControl, ICommand?>("CommandOnCardClick");

    public int NValue
    {
        get => GetValue(NValueProperty);
        set => SetValue(NValueProperty, value);
    }

    public ICommand? CommandOnCardClick
    {
        get => (ICommand?)GetValue(CommandOnCardClickProperty);
        set => SetValue(CommandOnCardClickProperty, value);
    }
}