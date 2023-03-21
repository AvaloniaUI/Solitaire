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
            // Get a new isolated store for this user, domain, and assembly.
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                        IsolatedStorageScope.Domain |
                                                        IsolatedStorageScope.Assembly, null, null);

            //  Create data stream.
            await using var isoStream = new IsolatedStorageFileStream(Identifier + key, FileMode.Create, isoStore);
            await JsonSerializer.SerializeAsync(isoStream, obj, typeof(T), JsonContext.Default);
        }

        /// <inheritdoc />
        public async Task<T?> LoadObject(string key)
        {
            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                            IsolatedStorageScope.Domain |
                                                            IsolatedStorageScope.Assembly, null, null);
                await using var isoStream = new IsolatedStorageFileStream(Identifier + key, FileMode.Open, isoStore);
                var storedObj = (T?)await JsonSerializer.DeserializeAsync(isoStream, typeof(T), JsonContext.Default);
                return storedObj ?? default;
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