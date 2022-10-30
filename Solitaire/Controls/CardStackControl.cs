using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Styling;
using Avalonia.VisualTree;
using ReactiveUI;
using Solitaire.Models;
using Solitaire.ViewModels;
using Vector = Avalonia.Vector;

namespace Solitaire.Controls;

public class CardStackControl : Border
{
    public static readonly StyledProperty<List<PlayingCardViewModel>> SourceItemsProperty = AvaloniaProperty.Register<CardStackControl, List<PlayingCardViewModel>>("SourceItems");
    public static readonly StyledProperty<Canvas> TargetCanvasProperty = AvaloniaProperty.Register<CardStackControl, Canvas>("TargetCanvas");
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<CardStackControl, Orientation>("Orientation");
    public static readonly StyledProperty<double> FaceDownOffsetProperty = AvaloniaProperty.Register<CardStackControl, double>("FaceDownOffset");
    public static readonly StyledProperty<double> FaceUpOffsetProperty = AvaloniaProperty.Register<CardStackControl, double>("FaceUpOffset");
    public static readonly StyledProperty<OffsetMode> OffsetModeProperty = AvaloniaProperty.Register<CardStackControl, OffsetMode>("OffsetMode");
    public static readonly StyledProperty<ICommand> CommandOnCardClickProperty = AvaloniaProperty.Register<CardStackControl, ICommand>("CommandOnCardClick");
    public static readonly StyledProperty<DrawMode> NValueProperty = AvaloniaProperty.Register<CardStackControl, DrawMode>("NValue");

    public List<PlayingCardViewModel> SourceItems
    {       
        get => GetValue(SourceItemsProperty);
        set => SetValue(SourceItemsProperty, value);
    }

    public Canvas TargetCanvas
    {
        get => GetValue(TargetCanvasProperty);
        set => SetValue(TargetCanvasProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double     FaceDownOffset
    {
        get => GetValue(FaceDownOffsetProperty);
        set => SetValue(FaceDownOffsetProperty, value);
    }

    public double FaceUpOffset
    {
        get => GetValue(FaceUpOffsetProperty);
        set => SetValue(FaceUpOffsetProperty, value);
    }

    public OffsetMode OffsetMode
    {
        get => GetValue(OffsetModeProperty);
        set => SetValue(OffsetModeProperty, value);
    }

    public ICommand CommandOnCardClick
    {
        get => GetValue(CommandOnCardClickProperty);
        set => SetValue(CommandOnCardClickProperty, value);
    }

    public DrawMode NValue
    {
        get => GetValue(NValueProperty);
        set => SetValue(NValueProperty, value);
    }
}