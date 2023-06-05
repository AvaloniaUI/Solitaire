using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;

namespace Solitaire.Behaviors;

public class StaggeredFadeInBehavior : Behavior<Panel>
{
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();

        if (AssociatedObject is null) return;

        foreach (var item in AssociatedObject.Children)
        {
            if (item is not { } innerItem) return;
            var cts = new CancellationTokenSource();

            innerItem.AttachedToVisualTree += delegate
            {
                var animations = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(250),
                    Delay =  TimeSpan.FromMilliseconds(75 * AssociatedObject.Children.IndexOf(innerItem)),
                    FillMode = FillMode.Both
                };

                animations.Children.Add(new KeyFrame()
                {
                    Cue = Cue.Parse("0%", null),
                    Setters = { new Setter { Property = Visual.OpacityProperty, Value = 0d } }
                });

                animations.Children.Add(new KeyFrame()
                {
                    Cue = Cue.Parse("100%", null),
                    Setters = { new Setter { Property = Visual.OpacityProperty, Value = 1d } }
                });
                
                
                animations.Children.Add(new KeyFrame()
                {
                    Cue = Cue.Parse("0%", null),
                    Setters =
                    {
                        new Setter { Property = ScaleTransform.ScaleXProperty, Value = 0.93d },
                        new Setter { Property = ScaleTransform.ScaleYProperty, Value = 0.93d }
                    }
                });

                animations.Children.Add(new KeyFrame()
                {
                    Cue = Cue.Parse("100%", null),
                    Setters = 
                    {
                        new Setter { Property = ScaleTransform.ScaleXProperty, Value = 1d },
                        new Setter { Property = ScaleTransform.ScaleYProperty, Value = 1d }
                    }
                });

                animations.RunAsync(innerItem, cts.Token);
            };

            innerItem.DetachedFromVisualTree += delegate { cts.Cancel(); };
        }
    }
}