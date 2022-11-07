using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solitaire.Models;

namespace Solitaire.Browser;

public partial class BrowserSettingsStore<T> : IRuntimeStorageProvider<T>
{
    [JSImport("globalThis.localStorage.setItem")]
    private static partial void SetItem(string key, string value);

    [JSImport("globalThis.localStorage.getItem")]
    private static partial string GetItem(string key);

    private static string Identifier { get; } = typeof(T).FullName?.Replace(".", string.Empty) ?? "default";

    /// <inheritdoc />
    public Task SaveObject(T obj, string key)
    {
        var serializedObjJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        SetItem(Identifier + key, serializedObjJson);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T?> LoadObject(string key)
    {
        try
        {
            var t = GetItem(Identifier + key);
            if (string.IsNullOrEmpty(t)) return default;
            var x = JsonConvert.DeserializeObject<T>(t);
            return x ?? default;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return default;
    }
}