using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
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
    
    private class DefaultSettingsStore<T> : IRuntimeStorageProvider<T>
    {
        private static string Identifier { get; } = typeof(T).FullName?.Replace(".", string.Empty) ?? "default";

        
        /// <inheritdoc />
        public async Task SaveObject(T obj, string key)
        {
            try
            {
                // Get a new isolated store for this user, domain, and assembly.
                using var isoStore = IsolatedStorageFile.GetUserStoreForApplication();

                //  Create data stream.
                await using var isoStream = isoStore.OpenFile(Identifier + key, FileMode.CreateNew, FileAccess.Write);
                await JsonSerializer.SerializeAsync(isoStream, obj, typeof(T), JsonContext.Default);
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
                using var isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                await using var isoStream = isoStore.OpenFile(Identifier + key, FileMode.Open, FileAccess.Read);
                var storedObj = (T?)await JsonSerializer.DeserializeAsync(isoStream, typeof(T), JsonContext.Default);
                return storedObj ?? default;
            }
            catch (Exception e) when (e.InnerException is FileNotFoundException)
            {
                // Ignore
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return default;
        }

    }
    
    public static IRuntimeStorageProvider<PersistentState> CasinoStorage { get; set; }
        = new DefaultSettingsStore<PersistentState>();
}