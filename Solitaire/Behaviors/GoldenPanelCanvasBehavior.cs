using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Solitaire.Controls;

namespace Solitaire.Behaviors;

public class GoldenPanelCanvasBehavior : GoldenPanelBaseBehavior
{
    private Rect? _lastCallRect;
    private CompositionCustomVisual? _customVisual;
    private bool _isCanvasFirstTimeLayout = true;
    private ImplicitAnimationCollection? _implicitAnimations;

    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is Canvas)
        {
            AssociatedObject.Loaded += CanvasOnLoaded;
            AssociatedObject.Unloaded -= CanvasOnUnloaded;
        }

        base.OnAttachedToVisualTree();
    }

    private void CanvasOnUnloaded(object? sender, RoutedEventArgs e)
    {
        _customVisual?.SendHandlerMessage(new CustomVisualHandler.MessageStruct(CustomVisualHandler.CustomVisualMessage.Stop));
    }

    private void CanvasOnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateTrayBoundsAction = UpdateTrayBounds;
        MasterCanvas = sender as Canvas ?? throw new InvalidOperationException();
        var compositor = ElementComposition.GetElementVisual(MasterCanvas)?.Compositor;
        if (compositor == null) return;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);

        var sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
        sizeAnimation.Target = "Size";
        sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        sizeAnimation.Duration = TimeSpan.FromMilliseconds(400);

        _implicitAnimations = compositor.CreateImplicitAnimationCollection();
        _implicitAnimations["Offset"] = offsetAnimation;
        _implicitAnimations["Size"] = sizeAnimation;
    }

    private void UpdateTrayBounds(Rect? rect, bool animationsEnabled)
    {
        if (rect is not { } rectV) return;

        var targetRect = rectV.Inflate(CanvasPadding);

        if (_isCanvasFirstTimeLayout && MasterCanvas is not null)
        {
            var compositor = ElementComposition.GetElementVisual(MasterCanvas)?.Compositor;
            if (compositor == null || _customVisual?.Compositor == compositor)
                return;
            _customVisual = compositor.CreateCustomVisual(new CustomVisualHandler());
            ElementComposition.SetElementChildVisual(MasterCanvas, _customVisual);
            _customVisual.Size = new Vector2((float)targetRect.Width, (float)targetRect.Size.Height);
            _customVisual.Offset = new Vector3((float)targetRect.Position.X, (float)targetRect.Position.Y, 0);
            _customVisual.SendHandlerMessage(new CustomVisualHandler.MessageStruct(CustomVisualHandler.CustomVisualMessage.Start));
            _isCanvasFirstTimeLayout = false;
        }

        if (AssociatedObject is not Canvas || _lastCallRect.Equals(targetRect)) return;
        _lastCallRect = targetRect;
        
        if (animationsEnabled) 
            EnableAnimations();
        else 
            DisableAnimations();
        
        Update();
    }

    private const int CanvasPadding = 50;

    private void Update()
    {
        if (_customVisual is null || MasterCanvas is null || _lastCallRect is not { } targetRect)
            return;

        _customVisual.Size = new Vector2((float)targetRect.Width, (float)targetRect.Size.Height);
        _customVisual.Offset = new Vector3((float)targetRect.Position.X, (float)targetRect.Position.Y, 0);

        _customVisual.SendHandlerMessage(new CustomVisualHandler.MessageStruct(CustomVisualHandler.CustomVisualMessage.EndValueBounds)
        {
            PayloadRect = targetRect
        });
    }

    private void EnableAnimations()
    {
        if (_customVisual is not null && _implicitAnimations is not null)
            _customVisual.ImplicitAnimations = _implicitAnimations;
    }

    private void DisableAnimations()
    {
        if (_customVisual is not null)
            _customVisual.ImplicitAnimations = null;
    }

    private class CustomVisualHandler : CompositionCustomVisualHandler
    {
        private readonly IImmutableBrush _brush0 = Brushes.Transparent.ToImmutable();
        private readonly IImmutableBrush _brush1 = Brushes.Gold.ToImmutable();
        private readonly BoxShadows _defaultBoxShadow = BoxShadows.Parse("0 0 50 0 Black");

        private TimeSpan _animationElapsed;
        private TimeSpan? _lastServerTime;
        private bool _running;
        private BorderRenderHelper? _borderRenderHelper;
        private Rect? _finalLayout;


        public enum CustomVisualMessage
        {
            Start,
            Stop,
            EndValueBounds
        }

        public record struct MessageStruct(CustomVisualMessage Message, Rect? PayloadRect = null);

        public override void OnMessage(object message)
        {
            if (message is not MessageStruct msg) return;

            switch (msg)
            {
                case { Message: CustomVisualMessage.Start }:
                    _running = true;
                    _lastServerTime = null;
                    _borderRenderHelper = new BorderRenderHelper();
                    RegisterForNextAnimationFrameUpdate();
                    break;
                case { Message: CustomVisualMessage.Stop }:
                    _running = false;
                    break;
                case { Message: CustomVisualMessage.EndValueBounds, PayloadRect: { } payloadRect }:
                    _finalLayout = payloadRect;
                    break;
            }
        }

        public override void OnAnimationFrameUpdate()
        {
            if (!_running)
                return;

            // This throttles down the animations so that it wont waste render thread time.
            if (_finalLayout is { } finalLayout && GetRenderBounds() is var currentBounds)
            {
                if (Math.Abs(finalLayout.Width - currentBounds.Width) < 0.5d &&
                    Math.Abs(finalLayout.Height - currentBounds.Height) < 0.5d)
                {
                    return;
                }
            }

            Invalidate();
            RegisterForNextAnimationFrameUpdate();
        }

        private void RenderTiledNoise(ImmediateDrawingContext drawingContext, Rect rect)
        {
            using (drawingContext.PushClip(new RoundedRect(rect, 8)))
            {
                drawingContext.FillRectangle(Brushes.Black, rect);
                using (drawingContext.PushOpacity(0.08, rect))
                {
                    if (RandomNoiseTextureControl.NoiseTexture == null) return;
                    var bitmapSize = RandomNoiseTextureControl.NoiseTexture.Size;
                    var tileCount = rect.Size / bitmapSize;
                    for (var x = 0; x < Math.Ceiling(tileCount.X); x++)
                    for (var y = 0; y < Math.Ceiling(tileCount.Y); y++)
                        drawingContext.DrawBitmap(RandomNoiseTextureControl.NoiseTexture, new Rect(new Point(
                            rect.Position.X + (bitmapSize.Width * x),
                            rect.Position.Y + (bitmapSize.Width * y)), bitmapSize));
                }
            }
        }

        public override void OnRender(ImmediateDrawingContext drawingContext)
        {
            if (_running)
            {
                if (_lastServerTime.HasValue) _animationElapsed += (CompositionNow - _lastServerTime.Value);
                _lastServerTime = CompositionNow;
            }

            if (_borderRenderHelper is null) return;

            var rb0 = GetRenderBounds().Deflate(CanvasPadding);
            var rb1 = rb0.Deflate(5);

            BorderRenderHelper.RenderImmediate(drawingContext, rb0, new Thickness(6),
                8, _brush0,
                _brush1, _defaultBoxShadow, borderLineCap: PenLineCap.Round);

            RenderTiledNoise(drawingContext, rb1);
        }
    }
}