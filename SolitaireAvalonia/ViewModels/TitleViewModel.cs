namespace SolitaireAvalonia.ViewModels;

public class TitleViewModel : ViewModelBase
{
    private readonly CasinoViewModel _casinoViewModel;

    public TitleViewModel(CasinoViewModel casinoViewModel)
    {
        _casinoViewModel = casinoViewModel;
    }
}