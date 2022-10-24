using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solitaire.ViewModels;

namespace Solitaire.Browser;

public class BrowserSettingsStore<T> : IRuntimeStorageProvider<T>
{
    public BrowserSettingsStore()
    {
    }

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

        Console.WriteLine(serializedObjJson);
        //await _js.InvokeVoidAsync("localStorage.setItem", _ident, serializedObjJson);
    }

    /// <inheritdoc />
    public async Task<T?> LoadObject()
    {
        try
        {
            var _ident = typeof(T).FullName?.ToLowerInvariant().Replace(".", string.Empty) ?? "default";
            Console.WriteLine(_ident);

            /*var t = await _js.InvokeAsync<string>("localStorage.getItem", _ident);
            if (string.IsNullOrEmpty(t)) return default;

            var x = JsonConvert.DeserializeObject<T>(t);

            Console.WriteLine(t);
            return x ?? default;*/

            return default;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return default;
    }
}