using System;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Threading;
using Avalonia.Xaml.Interactions.Custom;

namespace Solitaire.Behaviors;

public abstract class CompositionBehaviors<T> : DisposingBehavior<T> where T : class, IAvaloniaObject
{
    public TimeSpan Duration { get; set; }
    public TimeSpan Delay { get; set; }
    public IEasing? Easing { get; set; }
}

public class CompositionOpacityBehavior : CompositionBehaviors<Control>
{ 
    public float InitialOpacity { get; set; } = 0f;
    public float StartOpacity { get; set; } = 0f;
    public float FinalOpacity { get; set; } = 1f;

    protected override void OnAttached(CompositeDisposable disposables)
    {
        if (AssociatedObject != null)
            Observable.FromEventPattern(AssociatedObject, nameof(AssociatedObject.Loaded))
                .Do(x =>
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
                    return;
                })
                .Subscribe()
                .DisposeWith(disposables);
    }
}

public class CompositionScaleBehavior : CompositionBehaviors<Control>
{
    protected override void OnAttached(CompositeDisposable disposables)
    {
        if (AssociatedObject != null)
            Observable.FromEventPattern(AssociatedObject, nameof(AssociatedObject.Loaded))
                .Do(x =>
                {
                    if (AssociatedObject == null) return;
                    var compositionVisual = ElementComposition.GetElementVisual(AssociatedObject);
                    if (compositionVisual is null)
                    {
                        return;
                    }

                    compositionVisual.AnchorPoint = new Vector2(0.5f, 0.5f);
                    var compositor = compositionVisual.Compositor;
                    var animation = compositor.CreateVector3KeyFrameAnimation();

                    animation.InsertKeyFrame(1f, new Vector3(3f, 3f, 0), new SplineEasing(0.1, 0.9, 0.2, 1d));
                    animation.Duration = TimeSpan.FromSeconds(0.9);
                    animation.DelayTime = TimeSpan.FromSeconds(1);
                    compositionVisual.StartAnimation("Scale", animation);
                    return;
                })
                .Subscribe()
                .DisposeWith(disposables);
    }
}