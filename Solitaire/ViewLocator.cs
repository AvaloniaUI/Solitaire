using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Solitaire.ViewModels;

namespace Solitaire;

public class ViewLocator : IDataTemplate
{
    public IControl? Build(object? data)
    {
        if (data is null)
            return null;

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        IControl? returnVal = null;
        Exception? ex = null;

        try
        {
            var type = Type.GetType(name);

            if (type != null)
            {
                returnVal = (Control) Activator.CreateInstance(type)!;
            }
        }
        catch (Exception _)
        {
            ex = _;
        }
        finally
        {
            if (ex is { })
            {
                returnVal = new TextBlock
                {
                    Text = $"An exception occurred while trying to instantiate {name}: {ex}"
                };
            }
        }

        return returnVal;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}