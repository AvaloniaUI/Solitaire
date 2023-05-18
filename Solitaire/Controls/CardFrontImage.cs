using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Solitaire.Converters;
using Solitaire.Models;
using Solitaire.ViewModels;

namespace Solitaire.Controls;

public class CardFrontImage : Image
{
    private PlayingCardViewModel? _model;

    public CardFrontImage()
    {
        UpdateCard();
    }

    void UpdateCard()
    {
        Source = PlayingCardToBrushConverter.Convert(CardType.SA);
    }


    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_model != null)
            _model.PropertyChanged += OnModelChanged;
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_model != null)
            _model.PropertyChanged -= OnModelChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        Source = PlayingCardToBrushConverter.Convert(_model?.CardType ?? CardType.SA);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_model != null)
            _model.PropertyChanged -= OnModelChanged;
        _model = DataContext as PlayingCardViewModel;
        if (_model != null)
            _model.PropertyChanged += OnModelChanged;
        OnModelChanged(null, null!);
        base.OnDataContextChanged(e);
    }
}