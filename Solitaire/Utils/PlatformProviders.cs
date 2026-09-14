using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.Json.Serialization.Metadata;
using Avalonia;
using Avalonia.VisualTree;
using Solitaire.Models;
using Solitaire.ViewModels;

namespace Solitaire.Utils;

public static class PlatformProviders
{
    public static double NextRandomDouble()
    {
        var nextULong = BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(sizeof(ulong)));

        return (nextULong >> 11) * (1.0 / (1ul << 53));
    }

    internal sealed class DefaultSettingsStore<T> : IRuntimeStorageProvider<T>
    {
        private static string Identifier { get; } = typeof(T).FullName?.Replace(".", string.Empty) ?? "default";
        private readonly JsonTypeInfo<T>? _metadata;

        public DefaultSettingsStore(JsonTypeInfo<T>? metadata = null) => _metadata = metadata;


        /// <inheritdoc />
        public async Task SaveObject(T obj, string key)
        {
            try
            {
                var serializedObjJson = StorageJson.Serialize(obj, _metadata);

                // Retain the original store identity and file names for existing saves.
                using var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                                 IsolatedStorageScope.Domain |
                                                                 IsolatedStorageScope.Assembly, null, null);

                await using var isoStream = new IsolatedStorageFileStream(Identifier + key, FileMode.Create, isoStore);
                await using var writer = new StreamWriter(isoStream);
                await writer.WriteAsync(serializedObjJson);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <inheritdoc />
        public async Task<T?> LoadObject(string key)
        {
            try
            {
                using var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                                 IsolatedStorageScope.Domain |
                                                                 IsolatedStorageScope.Assembly, null, null);
                await using var isoStream = new IsolatedStorageFileStream(Identifier + key, FileMode.Open, isoStore);
                using var reader = new StreamReader(isoStream);
                var savedString = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(savedString))
                    return default;
                var storedObj = StorageJson.Deserialize(savedString, _metadata);
                return storedObj ?? default;
            }
            catch (Exception e) when (e is FileNotFoundException || e.InnerException is FileNotFoundException)
            {
                // A first run has no settings file.
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return default;
        }

    }

    public static IRuntimeStorageProvider<CasinoViewModel> CasinoStorage { get; set; }
        = new DefaultSettingsStore<CasinoViewModel>();
}
