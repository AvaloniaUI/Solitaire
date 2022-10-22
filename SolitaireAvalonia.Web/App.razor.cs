using Avalonia.Web.Blazor;

namespace SolitaireAvalonia.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        WebAppBuilder.Configure<SolitaireAvalonia.App>()
            .SetupWithSingleViewLifetime();
    }
}