using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Solitaire.Controls;
using Solitaire.ViewModels;

#pragma warning disable CS0618 // Type or member is obsolete

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            topLevel.PlatformSettings?.HotkeyConfiguration.Back.Add(new KeyGesture(Key.Escape));

            topLevel.BackRequested += TopLevelOnBackRequested;
            
            if (topLevel is { InsetsManager: { } insetsManager })
            {
                insetsManager.SafeAreaChanged += InsetsManagerOnSafeAreaChanged;
                InsetsManagerOnSafeAreaChanged(insetsManager, new SafeAreaChangedArgs(insetsManager.SafeAreaPadding));
            }
            else
            {
                InsetsManagerOnSafeAreaChanged(this, new SafeAreaChangedArgs(default));
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            topLevel.BackRequested -= TopLevelOnBackRequested;

            if (topLevel is { InsetsManager: { } insetsManager })
            {
                insetsManager.SafeAreaChanged -= InsetsManagerOnSafeAreaChanged;
            }
        }
    }

    private void InsetsManagerOnSafeAreaChanged(object? sender, SafeAreaChangedArgs e)
    {
        // Apply "10,10,10,0" as a minimum padding.
        RootContentControl.Padding = new Thickness(
            Math.Max(10, e.SafeAreaPadding.Left),
            Math.Max(10, e.SafeAreaPadding.Top),
            Math.Max(10, e.SafeAreaPadding.Right),
            e.SafeAreaPadding.Bottom);
    }

    private void TopLevelOnBackRequested(object? sender, RoutedEventArgs e)
    {
        (DataContext as CasinoViewModel)?.NavigateToTitleCommand.Execute(null);
    }
}