using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Solitaire.Models;
using Solitaire.ViewModels;
using Solitaire.Views;

namespace Solitaire.Controls;

public class CardDisplayControl : Control
{
    public static RenderTargetBitmap? CardsTextureAtlas;
    public static Dictionary<string, Rect>? CardsAtlasDictionary;


    static CardDisplayControl()
    {
    }

    private CardType _cardType;

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
            this.GetVisualRoot()?.RenderScaling is not { } scaling)return;
        
        Width = cardWidth;
        Height = cardHeight;

            
            if(CardsTextureAtlas is not null ||
            CardsAtlasDictionary is not null) return;


        var cardTypes = Enum.GetNames<CardType>().ToList();
        cardTypes.Add("CardBack");

        var side = Math.Ceiling(Math.Sqrt(cardTypes.Count));
        CardsAtlasDictionary = new Dictionary<string, Rect>();

        var cardCount = 0;
        for (var y = 0; y < side; y++)
        for (var x = 0; x < side; x++)
        {
            if (cardCount >= cardTypes.Count)
                break;

            CardsAtlasDictionary.Add(cardTypes[cardCount], new Rect(
                new Point(cardWidth * x, cardHeight * y),
                new Size(cardWidth, cardHeight)));

            cardCount++;
        }

        var maxPointX = CardsAtlasDictionary.Select(x => x.Value.BottomRight.X).Max();
        var maxPointY = CardsAtlasDictionary.Select(x => x.Value.BottomRight.Y).Max();

        CardsTextureAtlas = new RenderTargetBitmap(
            new PixelSize((int)Math.Ceiling(maxPointX), (int)Math.Ceiling(maxPointY)),
            new Vector(96d, 96d) * scaling);

        using var drawingContext = CardsTextureAtlas.CreateDrawingContext();

        foreach (var card in CardsAtlasDictionary)
        {
            if (!(Application.Current?.TryGetResource(card.Key, out var val) ?? false)) continue;
            if (val is not (DrawingImage and IImage img)) continue;

            using (drawingContext.PushTransform(new TranslateTransform(card.Value.X, card.Value.Y).Value))
            {
                img.Draw(drawingContext, new Rect(img.Size),
                    new Rect(new Size(cardWidth, cardHeight)));
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.White, new Rect(Bounds.Size));

        if ((DataContext is PlayingCardViewModel vm) && CardsTextureAtlas != null && (CardsAtlasDictionary != null))
            context.DrawImage(CardsTextureAtlas,
                CardsAtlasDictionary[vm.IsFaceDown ? "CardBack" : _cardType.ToString()],
                new Rect(Bounds.Size).Deflate(6));

        base.Render(context);
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PlayingCardViewModel.CardType):

                break;
        }
    }
}