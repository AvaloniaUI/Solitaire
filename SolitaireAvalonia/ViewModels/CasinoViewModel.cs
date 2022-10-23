using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SolitaireAvalonia.Utils;

namespace SolitaireAvalonia.ViewModels;

/// <summary>
/// The casino view model.
/// </summary>
public partial class CasinoViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase? _currentView;

    /// <summary>
    /// Initializes a new instance of the <see cref="CasinoViewModel"/> class.
    /// </summary>
    public CasinoViewModel()
    {
        KlondikeInstance = new KlondikeSolitaireViewModel(this);
        SpiderInstance = new SpiderSolitaireViewModel(this);
        SettingsInstance = new SettingsViewModel(this);
        TitleInstance = new TitleViewModel(this);
        CurrentView = TitleInstance;
    }

    public TitleViewModel TitleInstance { get; }
    public SettingsViewModel SettingsInstance { get; }
    public SpiderSolitaireViewModel SpiderInstance { get; }
    public KlondikeSolitaireViewModel KlondikeInstance { get; }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    public async void Save()
    {
        await PlatformProviders.CasinoStorage.SaveObject(this);
        return;


        // Get a new isolated store for this user, domain, and assembly.
        var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                    IsolatedStorageScope.Domain |
                                                    IsolatedStorageScope.Assembly, null, null);

        //  Create data stream.
        using var isoStream = new IsolatedStorageFileStream("Casino4.xml", FileMode.Create, isoStore);
        using var writer = new StreamWriter(isoStream);
        writer.Write(JsonConvert.SerializeObject(this, new JsonSerializerSettings()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
    }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    /// <returns></returns>
    public static async Task<CasinoViewModel> Load()
    {
        // // Get a new isolated store for this user, domain, and assembly.
        // var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
        //                                             IsolatedStorageScope.Domain |
        //                                             IsolatedStorageScope.Assembly, null, null);
        //
        // //  Create data stream.
        // try
        // {
        //     //  Save the casino.
        //     using var isoStream =
        //         new IsolatedStorageFileStream("Casino4.xml", FileMode.Open, isoStore);
        // }
        // catch
        // {
        // }
        // finally
        // {
        // }


        var ret = await PlatformProviders.CasinoStorage.LoadObject();
        if (ret is CasinoViewModel loadedVM)
        {
            Console.WriteLine($"Test: {loadedVM}");
            return loadedVM;
        }

        Console.WriteLine($"Testsdaasd");

        return new CasinoViewModel();
    }
}

public interface IRuntimeStorageProvider<T>
{
    Task SaveObject(T obj);
    Task<T?> LoadObject();
}