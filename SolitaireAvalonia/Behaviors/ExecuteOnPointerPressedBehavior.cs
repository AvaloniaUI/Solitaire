using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace SolitaireAvalonia.Behaviors;

public class ExecuteOnPointerPressedBehavior : Behavior<Control>
{
    public static readonly StyledProperty<bool> ActivateOnLeftMouseProperty =
        AvaloniaProperty.Register<ExecuteOnPointerPressedBehavior, bool>(
            "ActivateOnLeftMouse", true);

    public bool ActivateOnLeftMouse
    {
        get => GetValue(ActivateOnLeftMouseProperty);
        set => SetValue(ActivateOnLeftMouseProperty, value);
    }

    public static readonly StyledProperty<bool> ActivateOnRightMouseProperty =
        AvaloniaProperty.Register<ExecuteOnPointerPressedBehavior, bool>(
            "ActivateOnRightMouse");

    public bool ActivateOnRightMouse
    {
        get => GetValue(ActivateOnRightMouseProperty);
        set => SetValue(ActivateOnRightMouseProperty, value);
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

        if (ActivateOnLeftMouse && properties.IsLeftButtonPressed)
        {
            Command?.Execute(CommandParameter);
        }
        else if (ActivateOnRightMouse && properties.IsRightButtonPressed)
        {
            Command?.Execute(CommandParameter);
        }
    }
}