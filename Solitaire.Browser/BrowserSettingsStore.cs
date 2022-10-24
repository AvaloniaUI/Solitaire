using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solitaire.ViewModels;

namespace Solitaire.Browser;

public partial class BrowserSettingsStore<T> : IRuntimeStorageProvider<T>
{
    public BrowserSettingsStore()
    {
    }
    
    [JSImport("localStorage.setItem")]
    private static partial void SetItem(string key, string value);
    
    [JSImport("localStorage.getItem")]
    private static partial string GetItem(string key);

    /// <inheritdoc />
    public Task SaveObject(T obj)
    {
        var _ident = typeof(T).FullName?.ToLowerInvariant().Replace(".", string.Empty) ?? "default";

        Console.WriteLine(_ident);

        var serializedObjJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        Console.WriteLine(serializedObjJson);
        
        SetItem(_ident, serializedObjJson);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T?> LoadObject()
    {
        try
        {
            var _ident = typeof(T).FullName?.ToLowerInvariant().Replace(".", string.Empty) ?? "default";
            Console.WriteLine(_ident);

            var t = GetItem(_ident);
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