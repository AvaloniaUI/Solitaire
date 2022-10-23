 
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;

namespace SolitaireAvalonia.Controls;

public class CardStackControl : ItemsControl, IStyleable
{
    Type IStyleable.StyleKey => typeof(CardStackControl);

    public static readonly StyledProperty<double> FaceDownOffsetProperty = AvaloniaProperty.Register<CardStackControl, double>(
        "FaceDownOffset");

    public double FaceDownOffset
    {
        get => GetValue(FaceDownOffsetProperty);
        set => SetValue(FaceDownOffsetProperty, value);
    }

    public static readonly StyledProperty<double> FaceUpOffsetProperty = AvaloniaProperty.Register<CardStackControl, double>(
        "FaceUpOffset");

    public double FaceUpOffset
    {
        get => GetValue(FaceUpOffsetProperty);
        set => SetValue(FaceUpOffsetProperty, value);
    }

    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<CardStackControl, Orientation>(
        "Orientation");

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }


    public static readonly StyledProperty<OffsetMode> OffsetModeProperty = AvaloniaProperty.Register<CardStackControl, OffsetMode>(
        "OffsetMode");

    public OffsetMode OffsetMode
    {
        get => GetValue(OffsetModeProperty);
        set => SetValue(OffsetModeProperty, value);
    }

    public static readonly StyledProperty<int> NValueProperty = AvaloniaProperty.Register<CardStackControl, int>(
        "NValue", 1);

    public int NValue
    {
        get => GetValue(NValueProperty);
        set => SetValue(NValueProperty, value);
    }
}