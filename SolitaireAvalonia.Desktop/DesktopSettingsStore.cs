using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SolitaireAvalonia.ViewModels;

namespace SolitaireAvalonia.Desktop;

internal class DesktopSettingsStore<T> : IRuntimeStorageProvider<T>
{
    /// <inheritdoc />
    public async Task SaveObject(T obj)
    {
        var _ident = typeof(T).FullName?.ToLowerInvariant().Replace(".", string.Empty) ?? "default";

        Console.WriteLine(_ident);

        var serializedObjJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Get a new isolated store for this user, domain, and assembly.
        var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                    IsolatedStorageScope.Domain |
                                                    IsolatedStorageScope.Assembly, null, null);

        //  Create data stream.
        await using var isoStream = new IsolatedStorageFileStream($"{_ident}.xml", FileMode.Create, isoStore);
        await using var writer = new StreamWriter(isoStream);
        await writer.WriteAsync(serializedObjJson);
    }

    /// <inheritdoc />
    public async Task<T?> LoadObject()
    {
        try
        {
            var _ident = typeof(T).FullName?.ToLowerInvariant().Replace(".", string.Empty) ?? "default";
            Console.WriteLine(_ident);


            // Get a new isolated store for this user, domain, and assembly.
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                        IsolatedStorageScope.Domain |
                                                        IsolatedStorageScope.Assembly, null, null);

            //  Save the casino.
            await using var isoStream =
                new IsolatedStorageFileStream($"{_ident}.xml", FileMode.Open, isoStore);


            using var reader = new StreamReader(isoStream);
            var t = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(t)) return default;

            var x = JsonConvert.DeserializeObject<T>(t);

            Console.WriteLine(t);

            return x ?? default;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return default;
    }
}