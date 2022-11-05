using System;
using System.Numerics;
using System.Reactive.Disposables;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;

namespace Solitaire.Behaviors;

public class CompositionScaleBehavior : CompositionBehaviors<Control>
{
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

                compositionVisual.AnchorPoint = new Vector2(0.5f, 0.5f);
                var compositor = compositionVisual.Compositor;
                var animation = compositor.CreateVector3KeyFrameAnimation();

                animation.InsertKeyFrame(1f, new Vector3(3f, 3f, 0), new SplineEasing(0.1, 0.9, 0.2, 1d));
                animation.Duration = TimeSpan.FromSeconds(0.9);
                animation.DelayTime = TimeSpan.FromSeconds(1);
                compositionVisual.StartAnimation("Scale", animation);
                return;

            };
    }
}