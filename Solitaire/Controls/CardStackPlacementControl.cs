using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Solitaire.Behaviors;
using Solitaire.Models;
using Solitaire.Utils;
using Solitaire.ViewModels;

namespace Solitaire.Controls;

public partial class CardStackPlacementControl : Border
{
    [GeneratedStyledProperty]
    public partial BatchObservableCollection<PlayingCardViewModel>? SourceItems { get; set; }

    [GeneratedStyledProperty]
    public partial Canvas TargetCanvas { get; set; }

    [GeneratedStyledProperty(DefaultValue = Avalonia.Layout.Orientation.Vertical)]
    public partial Orientation? Orientation { get; set; }

    [GeneratedStyledProperty]
    public partial double? FaceDownOffset { get; set; }

    [GeneratedStyledProperty]
    public partial double? FaceUpOffset { get; set; }

    [GeneratedStyledProperty(DefaultValue = Controls.OffsetMode.EveryCard)]
    public partial OffsetMode? OffsetMode { get; set; }

    [GeneratedStyledProperty]
    public partial ICommand? CommandOnCardClick { get; set; }

    [GeneratedStyledProperty]
    public partial DrawMode NValue { get; set; }

    [GeneratedStyledProperty]
    public partial bool IsHomeStack { get; set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var stacks = CardFieldBehavior.GetCardStacks(this);
        if (!stacks.Contains(this))
            stacks.Add(this);
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var stacks = CardFieldBehavior.GetCardStacks(this);
        stacks.Remove(this);
        base.OnDetachedFromVisualTree(e);
    }
}
