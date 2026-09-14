using System;
using System.Diagnostics;
using Solitaire.Platform.Browser;
using System.Threading.Tasks;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.Browser;

internal sealed class BrowserSettingsStore<T> : IRuntimeStorageProvider<T>
{
    private static string Identifier { get; } = typeof(T).FullName?.Replace(".", string.Empty) ?? "default";

    /// <inheritdoc />
    public Task SaveObject(T obj, string key)
    {
        var serializedObjJson = StorageJson.Serialize(obj);

        LocalStorageInterop.SetItem(Identifier + key, serializedObjJson);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T?> LoadObject(string key)
    {
        try
        {
            var t = LocalStorageInterop.GetItem(Identifier + key);
            if (string.IsNullOrEmpty(t))
                return Task.FromResult<T?>(default);
            var x = StorageJson.Deserialize<T>(t);
            return Task.FromResult(x ?? default);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return Task.FromResult<T?>(default);
    }
}
