using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Solitaire.Controls;

public sealed class CasinoButton : Button
{
    protected override Type StyleKeyOverride => typeof(Button);
    internal bool IsActivating { get; private set; }

    protected override void OnClick()
    {
        IsActivating = true;
        try
        {
            var chrome = this.GetVisualDescendants().OfType<GoldButtonChrome>().FirstOrDefault();
            if (chrome?.PressSweep >= 1)
                chrome.Flash();
            base.OnClick();
        }
        finally
        {
            IsActivating = false;
        }
    }
}
