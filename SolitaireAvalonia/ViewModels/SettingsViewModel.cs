using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace SolitaireAvalonia.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly CasinoViewModel _casinoViewModel;
    
    public ICommand NavigateToTitleCommand { get; }

    public SettingsViewModel(CasinoViewModel casinoViewModel)
    {
        _casinoViewModel = casinoViewModel;

        NavigateToTitleCommand = new RelayCommand(() =>
        {
            _casinoViewModel.CurrentView = _casinoViewModel.TitleInstance;
        });
    }
}