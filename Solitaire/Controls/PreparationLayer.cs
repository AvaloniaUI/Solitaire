using Avalonia;
using Avalonia.Controls;

namespace Solitaire.Controls;

// Preparation must not change the window's desired size.
internal sealed class PreparationLayer : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        return default;
    }
}
