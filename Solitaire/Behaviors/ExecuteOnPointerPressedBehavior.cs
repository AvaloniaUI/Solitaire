using System.Windows.Input;
using Avalonia;
using Avalonia.Animation.Animators;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Solitaire.Behaviors;

public class ExecuteOnPointerPressedBehavior : Behavior<Control>
{
    public enum Buttons
    {
        LeftButton,
        RightButton
    }

    public static readonly StyledProperty<Buttons> ActivateOnProperty =
        AvaloniaProperty.Register<ExecuteOnPointerPressedBehavior, Buttons>(
            "ActivateOn");

    public Buttons ActivateOn
    {
        get => GetValue(ActivateOnProperty);
        set => SetValue(ActivateOnProperty, value);
    }

    public static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<ExecuteOnPointerPressedBehavior, ICommand>(
            "Command");

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<ExecuteOnPointerPressedBehavior, object?>(
            "CommandParameter");

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is { })
        {
            AssociatedObject.PointerPressed += PointerPressed;
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is { })
        {
            AssociatedObject.PointerPressed -= PointerPressed;
        }
    }

    private void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
         
        
        if (!Command.CanExecute(CommandParameter)) return;

        var properties = e.GetCurrentPoint(AssociatedObject).Properties;

        if ((ActivateOn == Buttons.LeftButton && properties.IsLeftButtonPressed) ||
            (ActivateOn == Buttons.RightButton && properties.IsRightButtonPressed))
        {
            Command.Execute(CommandParameter);
        }
    }
}