using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Utilities;
using Avalonia.Xaml.Interactivity;
using Microsoft.VisualBasic;
using Solitaire.Controls;

namespace Solitaire.Behaviors;

public class ConnectedTrayBehavior : Behavior<Control>
{
    private static Action<Rect?>? UpdateTrayBoundsAction { get; set; }
    private static Canvas? MasterCanvas { get; set; }

    protected override void OnAttachedToVisualTree()
    {
        switch (AssociatedObject)
        {
            case Canvas:
                AssociatedObject.Loaded += CanvasOnLoaded;
                AssociatedObject.Unloaded -= CanvasOnUnloaded;
                break;
            case Border:
                AssociatedObject.Loaded += BorderOnLoaded;
                AssociatedObject.Unloaded += BorderOnUnload;
                break;
        }

        base.OnAttachedToVisualTree();
    }

    private void CanvasOnUnloaded(object? sender, RoutedEventArgs e)
    {
        _customVisual?.SendHandlerMessage(new CustomVisualHandler.MessageStruct("Stop"));
    }

    private void BorderOnUnload(object? sender, RoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.LayoutUpdated -= BorderOnLayoutUpdated;
            DisableAnimations();
        }
    }

    private void BorderOnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border border) return;
        border.LayoutUpdated += BorderOnLayoutUpdated;
        if (UpdateTrayBoundsAction is null || MasterCanvas is null) return;
        var t = border.TransformToVisual(MasterCanvas);
        if (t is not { } m) return;
        var z = m.Transform(border.Bounds.TopLeft);
        var w = m.Transform(border.Bounds.BottomRight);
        EnableAnimations();
        UpdateTrayBoundsAction(new Rect(z, w));
    }

    private void BorderOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is not Border border || UpdateTrayBoundsAction is null || MasterCanvas is null) return;
        var t = border.TransformToVisual(MasterCanvas);
        if (t is not { } m) return;
        var z = m.Transform(border.Bounds.TopLeft);
        var w = m.Transform(border.Bounds.BottomRight);
        UpdateTrayBoundsAction(new Rect(z, w));
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
        
        MasterCanvas.LayoutUpdated += MasterCanvasOnLayoutUpdated;
    }

    private void MasterCanvasOnLayoutUpdated(object? sender, EventArgs e)
    {
        _customVisual?.SendHandlerMessage(new CustomVisualHandler.MessageStruct("MasterCanvasLayout")
        {
            PayloadRect = MasterCanvas?.Bounds
        });
    }

    private static Rect? _lastCallRect;
    private static CompositionCustomVisual? _customVisual;
    private static bool _isCanvasFirstTimeLayout = true;
    private static ImplicitAnimationCollection? _implicitAnimations;
    private bool _isBorderLayoutFirstTime = true;

    private void UpdateTrayBounds(Rect? obj)
    {
        if (_isCanvasFirstTimeLayout && MasterCanvas is not null && obj is { } targetRect)
        {
            var compositor = ElementComposition.GetElementVisual(MasterCanvas)?.Compositor;
            if (compositor == null || _customVisual?.Compositor == compositor)
                return;
            _customVisual = compositor.CreateCustomVisual(new CustomVisualHandler());
            ElementComposition.SetElementChildVisual(MasterCanvas, _customVisual);
            _customVisual.Size = new Vector2((float)targetRect.Width, (float)targetRect.Size.Height);
            _customVisual.Offset = new Vector3((float)targetRect.Position.X, (float)targetRect.Position.Y, 0);
            EnableAnimations();
            _customVisual.SendHandlerMessage(new CustomVisualHandler.MessageStruct("Start"));
            _isCanvasFirstTimeLayout = false;
        }

        if (AssociatedObject is not Canvas || _lastCallRect.Equals(obj)) return;
        _lastCallRect = obj;
        Update();
    }

    private void Update()
    {
        if (_customVisual is null || MasterCanvas is null || _lastCallRect is not { } targetRect)
            return;

        _customVisual.Size = new Vector2((float)targetRect.Width, (float)targetRect.Size.Height);
        _customVisual.Offset = new Vector3((float)targetRect.Position.X, (float)targetRect.Position.Y, 0);

        if (_isBorderLayoutFirstTime)
        {
            _isBorderLayoutFirstTime = false;
        }

        _customVisual.SendHandlerMessage(new CustomVisualHandler.MessageStruct("FinalLayout")
        {
            PayloadRect = targetRect
        });
        DisableAnimations();
    }

    private static void EnableAnimations()
    {
        if (_customVisual != null && _implicitAnimations != null)
            _customVisual.ImplicitAnimations = _implicitAnimations;
    }

    private static void DisableAnimations()
    {
        if (_customVisual != null)
            _customVisual.ImplicitAnimations = null;
    }

    private class CustomVisualHandler : CompositionCustomVisualHandler
    {
        private TimeSpan _animationElapsed;
        private TimeSpan? _lastServerTime;
        private bool _running;
        private BorderRenderHelper? _borderRenderHelper;
        private Rect? _finalLayout;
        private Rect? _canvasLayout;

        public record struct MessageStruct(string Message, Rect? PayloadRect = null);

        public override void OnMessage(object message)
        {
            if (message is not MessageStruct msg) return;

            switch (msg)
            {
                case { Message: "Start" }:
                    _running = true;
                    _lastServerTime = null;
                    _borderRenderHelper = new BorderRenderHelper();
                    RegisterForNextAnimationFrameUpdate();
                    break;
                case { Message: "Stop" }:
                    _running = false;
                    break;
                case { Message: "FinalLayout", PayloadRect: { } payloadRect}:
                    _finalLayout = payloadRect;
                    break;
                case { Message: "MasterCanvasLayout", PayloadRect: { } payloadRect}:
                    _canvasLayout = payloadRect;
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
            drawingContext.FillRectangle(Brushes.Black, rect);
            using (drawingContext.PushOpacity(0.08, rect))
            using (drawingContext.PushClip(rect))
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

        public override void OnRender(ImmediateDrawingContext drawingContext)
        {
            var clearRenderBounds = GetRenderBounds().Inflate(50);
            
            clearRenderBounds = new Rect(Math.Round(clearRenderBounds.X), Math.Round(clearRenderBounds.Y),
                Math.Round(clearRenderBounds.Width), Math.Round(clearRenderBounds.Height));
            drawingContext.FillRectangle(Brushes.Transparent, clearRenderBounds);

            if (_running)
            {
                if (_lastServerTime.HasValue) _animationElapsed += (CompositionNow - _lastServerTime.Value);
                _lastServerTime = CompositionNow;
            }

            if (_borderRenderHelper is null) return;

            try
            {
                var rb1 = GetRenderBounds().Deflate(3);

                RenderTiledNoise(drawingContext, rb1);
                //
                // _borderRenderHelper.Render(drawingContext, GetRenderBounds().Size, new Thickness(6),
                //     8, Brushes.Transparent,
                //     Brushes.Gold, BoxShadows.Parse("0 0 50 0 Black"), borderLineCap: PenLineCap.Round);
                
                _borderRenderHelper.Render(drawingContext, GetRenderBounds().Size, new Thickness(6),
                    8, Brushes.Transparent,
                    Brushes.Gold, default, borderLineCap: PenLineCap.Round);
            }
            catch (Exception)
            {
            }
        }
    }
}

internal class BorderRenderHelper
{
    public void Render(ImmediateDrawingContext context,
        Size finalSize, Thickness borderThickness, int cornerRadius,
        IBrush? background, IBrush? borderBrush, BoxShadows boxShadows, double borderDashOffset = 0,
        PenLineCap borderLineCap = PenLineCap.Flat, PenLineJoin borderLineJoin = PenLineJoin.Miter,
        IReadOnlyCollection<double>? borderDashArray = null)
    {
        var bordThick = borderThickness.Top;
        IPen? pen = null;

        ImmutableDashStyle? dashStyle = null;

        if (borderDashArray is { Count: > 0 })
        {
            dashStyle = new ImmutableDashStyle(borderDashArray, borderDashOffset);
        }

        if (borderBrush != null && bordThick > 0)
        {
            pen = new ImmutablePen(
                borderBrush.ToImmutable(),
                bordThick,
                dashStyle,
                borderLineCap,
                borderLineJoin);
        }

        var rect = new Rect(finalSize);
        if (!MathUtilities.IsZero(bordThick))
            rect = rect.Deflate(bordThick * 0.5);
        if (background == null || pen == null) return;
        context.DrawRectangle(background.ToImmutable(), pen.ToImmutable(), rect, cornerRadius, cornerRadius,
            boxShadows);
    }
}