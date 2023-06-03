using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Solitaire.Models;

namespace Solitaire.Views;

/// <summary>
/// Interaction logic for CasinoView.xaml
/// </summary>
public partial class CasinoView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CasinoView"/> class.
    /// </summary>
    public CasinoView()
    {
        InitializeComponent();
    }

    public static RenderTargetBitmap? CardsTextureAtlas;
    public static Dictionary<string, Rect>? CardsAtlasDictionary;

    protected override void OnLoaded()
    {
        if ((Application.Current?.TryGetResource("CardWidth", out var cw) ?? false) &&
            cw is double cardWidth &&
            (Application.Current?.TryGetResource("CardHeight", out var ch) ?? false) &&
            ch is double cardHeight &&
            this.GetVisualRoot()?.RenderScaling is { } scaling)
        {
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

                var pos = new Point(cardWidth * x, cardHeight * y);
                CardsAtlasDictionary.Add(cardTypes[cardCount], new Rect(pos, new Size(cardWidth, cardHeight)));
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
        base.OnLoaded();
    }
}