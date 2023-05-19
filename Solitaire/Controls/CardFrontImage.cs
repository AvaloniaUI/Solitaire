using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Solitaire.Converters;
using Solitaire.Models;
using Solitaire.ViewModels;

namespace Solitaire.Controls;

public class CardFrontImage : Border
{
    private PlayingCardViewModel? _model;
    private Image _image = new Image();
    private readonly DrawingImage _back;

    public CardFrontImage()
    {
        Application.Current!.Styles.TryGetResource("CardBack", null, out var res);
        _back = (DrawingImage)res!;
        _image.Source = _back;
        Update();
        Child = _image;
    }

    void Update()
    {
        if (_model == null)
            _image.Source = _back;
        else
        {
            if (_model.IsFaceDown)
                _image.Source = _back;
            else
                _image.Source = PlayingCardToBrushConverter.Convert(_model?.CardType ?? CardType.SA);
        }
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
        Update();
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