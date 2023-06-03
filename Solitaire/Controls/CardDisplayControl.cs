using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Solitaire.Behaviors;
using Solitaire.Models;
using Solitaire.ViewModels;
using Solitaire.Views;

namespace Solitaire.Controls;

public class CardDisplayControl : Control
{
    private static Dictionary<string, RenderTargetBitmap>? _cardsAtlasDictionary;
    private CardType _cardType;
    private static readonly BoxShadows DefaultBoxShadow = BoxShadows.Parse("0 5 40 -8 #88000000");

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is PlayingCardViewModel vm)
        {
            vm.PropertyChanged += VmOnPropertyChanged;
            _cardType = vm.CardType;
        }

        if ((!(Application.Current?.TryGetResource("CardWidth", out var cw) ?? false)) ||
            cw is not double cardWidth ||
            (!(Application.Current?.TryGetResource("CardHeight", out var ch) ?? false)) ||
            ch is not double cardHeight ||
            this.GetVisualRoot()?.RenderScaling is not { } scaling) return;

        Width = cardWidth;
        Height = cardHeight;


        if (_cardsAtlasDictionary is not null) return;
        var cardTypes = Enum.GetNames<CardType>().ToList();
        cardTypes.Add("CardBack");
        _cardsAtlasDictionary = new();

        foreach (var cardName in cardTypes)
        {
            _cardsAtlasDictionary.Add(cardName,
                new RenderTargetBitmap(new PixelSize((int)cardWidth, (int)cardHeight), new Vector(96d, 96d) * scaling));

            var targetBitmap = _cardsAtlasDictionary[cardName];
            using var drawingContext = targetBitmap.CreateDrawingContext();
            if (!(Application.Current?.TryGetResource(cardName, out var val) ?? false)) continue;
            if (val is not (DrawingImage and IImage img)) continue;
            img.Draw(drawingContext, new Rect(img.Size),
                new Rect(new Size(cardWidth, cardHeight)).CenterRect(new Rect(new Size(cardWidth, cardHeight)).Deflate(3)));
        }
    }

    public override void Render(DrawingContext context)
    {
        var renderBounds = new Rect(Bounds.Size);
        if (DataContext is not PlayingCardViewModel vm || _cardsAtlasDictionary is null) return;

        BorderRenderHelper.Render(context, renderBounds, new Thickness(1), 6, Brushes.White, Brushes.Gray,
            vm.IsFaceDown ? new BoxShadows() : DefaultBoxShadow);

        context.DrawImage(_cardsAtlasDictionary[vm.IsFaceDown ? "CardBack" : _cardType.ToString()],
            renderBounds.Deflate(6));

        base.Render(context);
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PlayingCardViewModel.CardType):

                break;
            case nameof(PlayingCardViewModel.IsFaceDown):
                InvalidateVisual();
                break;
        }
    }
}