using System.Reactive.Disposables;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace Solitaire.Behaviors;

public class CompositionOpacityBehavior : CompositionBehaviors<Control>
{ 
    public float InitialOpacity { get; set; } = 0f;
    public float StartOpacity { get; set; } = 0f;
    public float FinalOpacity { get; set; } = 1f;

    protected override void OnAttached(CompositeDisposable disposables)
    {
        if (AssociatedObject != null)
            AssociatedObject.Loaded += delegate 
            {
                if (AssociatedObject == null) return;
                var compositionVisual = ElementComposition.GetElementVisual(AssociatedObject);
                if (compositionVisual is null)
                {
                    return;
                }

                compositionVisual.Opacity = InitialOpacity;
                var compositor = compositionVisual.Compositor;
                var animation = compositor.CreateScalarKeyFrameAnimation();
                animation.InsertKeyFrame(1f, FinalOpacity, Easing ?? new LinearEasing());
                animation.Duration = Duration;

                DispatcherTimer.RunOnce(
                    () => { compositionVisual.StartAnimation(nameof(compositionVisual.Opacity), animation); },
                    Delay);
            };
        
    }
}