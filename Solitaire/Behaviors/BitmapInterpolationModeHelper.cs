using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Reactive;

namespace Solitaire.Behaviors;

public class BitmapInterpolationModeHelper
{
    public static readonly AttachedProperty<BitmapInterpolationMode> BitmapInterpolationModeProperty =
        AvaloniaProperty.RegisterAttached<BitmapInterpolationModeHelper, Visual, BitmapInterpolationMode>("BitmapInterpolationMode");

    public static void SetBitmapInterpolationMode(Visual obj, BitmapInterpolationMode value) => obj.SetValue(BitmapInterpolationModeProperty, value);
    public static BitmapInterpolationMode GetBitmapInterpolationMode(Visual obj) => obj.GetValue(BitmapInterpolationModeProperty);

    static BitmapInterpolationModeHelper()
    {
        BitmapInterpolationModeProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<BitmapInterpolationMode>>(
                e =>
                {
                    RenderOptions.SetBitmapInterpolationMode((Visual)e.Sender,
                        e.GetNewValue<BitmapInterpolationMode>());
                }));
    }
}