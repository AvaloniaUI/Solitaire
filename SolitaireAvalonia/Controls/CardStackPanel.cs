using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using SolitaireAvalonia.ViewModels;

namespace SolitaireAvalonia.Controls;

/// <summary>
/// A panel for laying out cards.
/// </summary>
public class CardStackPanel : StackPanel
{
    /// <summary>
    /// Infinite size, useful later.
    /// </summary>
    private readonly Size infiniteSpace = new(Double.MaxValue, Double.MaxValue);

    /// <summary>
    /// Measures the child elements of a <see cref="T:Avalonia.Controls.StackPanel"/> in anticipation of arranging them during the <see cref="M:Avalonia.Controls.StackPanel.ArrangeOverride(Avalonia.Size)"/> pass.
    /// </summary>
    /// <param name="constraint">An upper limit <see cref="T:Avalonia.Size"/> that should not be exceeded.</param>
    /// <returns>
    /// The <see cref="T:Avalonia.Size"/> that represents the desired size of the element.
    /// </returns>
    protected override Size MeasureOverride(Size constraint)
    {
        //  Keep track of the overall size required.
        var resultSize = new Size(0, 0);

        //  Get the offsets that each element will need.
        var offsets = CalculateOffsets();

        //  Calculate the total.
        var totalX = (from o in offsets select o.Width).Sum();
        var totalY = (from o in offsets select o.Height).Sum();

        //  Measure each child (always needed, even if we don't use
        //  the measurement!)
        foreach (var child in Children.Cast<Control?>())
        {
            //  Measure the child against infinite space.
            child?.Measure(infiniteSpace);
        }

        //  Add the size of the last element.
        if (LastChild == null) return new Size(totalX, totalY);
            
        //  Add the size.
        totalX += LastChild.DesiredSize.Width;
        totalY += LastChild.DesiredSize.Height;

        return new Size(totalX, totalY);
    }

    /// <summary>
    /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:Avalonia.FrameworkElement"/> derived class.
    /// </summary>
    /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
    /// <returns>The actual size used.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0, y = 0;
        var n = 0;
        var total = Children.Count;

        //  Get the offsets that each element will need.
        var offsets = CalculateOffsets();
            
        //  If we're going to pass the bounds, deal with it.
        if ((Bounds.Width > 0 && finalSize.Width > Bounds.Width) || 
            (Bounds.Height > 0 && finalSize.Height > Bounds.Height))
        {
            //  Work out the amount we have to remove from the offsets.
            var overrunX = finalSize.Width - Bounds.Width;
            var overrunY = finalSize.Height - Bounds.Height;

            //  Now as a per-offset.
            var dx = overrunX / offsets.Count;
            var dy = overrunY / offsets.Count;

            //  Now nudge each offset.
            for (var i = 0; i < offsets.Count; i++)
            {
                offsets[i] = new Size(Math.Max(0, offsets[i].Width - dx), Math.Max(0, offsets[i].Height - dy));
            }
            //  Make sure the final size isn't increased past what we can handle.

            finalSize -= new Size(overrunX, overrunY);
        }

        //  Arrange each child.
        foreach (Control child in Children)
        {
            //  Get the card. If we don't have one, skip.
            if (child.DataContext is not PlayingCardViewModel card)
                continue;

            //  Arrange the child at x,y (the first will be at 0,0)
            child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));

            //  Update the offset.
            x += offsets[n].Width;
            y += offsets[n].Height;

            //  Increment.
            n++;
        }

        return finalSize;
    }

    /// <summary>
    /// Calculates the offsets.
    /// </summary>
    /// <returns></returns>
    private List<Size> CalculateOffsets()
    {
        //  Calculate the offsets on a card by card basis.
        var offsets = new List<Size>();

        var n = 0;
        var total = Children.Count;

        //  Go through each card.
        foreach (var child in Children.Cast<Control>())
        {
            //  Get the card. If we don't have one, skip.
            if (child.DataContext is not PlayingCardViewModel card)
                continue;

            //  The amount we'll offset by.
            double faceDownOffset = 0;
            double faceUpOffset = 0;

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
                default:
                    break;
            }

            n++;

            //  Create the offset as a size.
            var offset = new Size(0, 0);

            var offsetsX = card.IsFaceDown ? faceDownOffset : faceUpOffset;
                
            //  Offset.
            switch (Orientation)
            { 
                case Orientation.Horizontal:
                    offset = new Size(offsetsX, offset.Height);
                    break;
                case Orientation.Vertical:
                    offset = new Size(offset.Width, offsetsX);
                    break;
                default:
                    break;
            }

            //  Add to the list.
            offsets.Add(offset);
        }

        return offsets;
    }

    /// <summary>
    /// Gets the last child.
    /// </summary>
    /// <value>The last child.</value>
    private IControl? LastChild => Children.Count > 0 ? Children[Children.Count - 1] : null;

    static CardStackPanel()
    {
        AffectsRender<CardStackPanel>(FaceUpOffsetProperty, FaceDownOffsetProperty);
    }

    public static readonly StyledProperty<double> FaceDownOffsetProperty = AvaloniaProperty.Register<CardStackPanel, double>(
        "FaceDownOffset", 5.0);

    public double FaceDownOffset
    {
        get => GetValue(FaceDownOffsetProperty);
        set => SetValue(FaceDownOffsetProperty, value);
    }

    public static readonly StyledProperty<double> FaceUpOffsetProperty = AvaloniaProperty.Register<CardStackPanel, double>(
        "FaceUpOffset", 5.0);

    public double FaceUpOffset
    {
        get => GetValue(FaceUpOffsetProperty);
        set => SetValue(FaceUpOffsetProperty, value);
    }
        
        
    public static readonly StyledProperty<OffsetMode> OffsetModeProperty = AvaloniaProperty.Register<CardStackPanel, OffsetMode>(
        "OffsetMode");

    public OffsetMode OffsetMode
    {
        get => GetValue(OffsetModeProperty);
        set => SetValue(OffsetModeProperty, value);
    }

    public static readonly StyledProperty<int> NValueProperty = AvaloniaProperty.Register<CardStackPanel, int>(
        "NValue", 1);

    public int NValue
    {
        get => GetValue(NValueProperty);
        set => SetValue(NValueProperty, value);
    }
}