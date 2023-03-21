using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;
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
        var serializedObjJson = JsonSerializer.Serialize(obj, typeof(T), JsonContext.Default);

        SetItem(Identifier + key, serializedObjJson);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T?> LoadObject(string key)
    {
        try
        {
            await Task.Delay(1);
            var t = GetItem(Identifier + key);
            if (string.IsNullOrEmpty(t)) return default;
            var x = (T?)JsonSerializer.Deserialize(t, typeof(T), JsonContext.Default);
            return x ?? default;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return default;
    }
}