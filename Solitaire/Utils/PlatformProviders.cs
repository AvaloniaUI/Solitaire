using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Solitaire.Models;
using Solitaire.ViewModels;

namespace Solitaire.Utils;
public static class PlatformProviders
{
    private static Random _random = new Random();
    public static double NextRandomDouble() => _random.NextDouble();

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
    
    public class StaticFileStorage<T> : IRuntimeStorageProvider<T>
    {
        private readonly string _path;

        public StaticFileStorage(string path)
        {
            _path = path;
        }
        
        public async Task SaveObject(T obj, string key)
        {
            await using var f = File.Create(_path);
            await JsonSerializer.SerializeAsync(f, obj, typeof(T), JsonContext.Default);
        }

        public async Task<T?> LoadObject(string key)
        {
            try
            {
                if (!File.Exists(_path))
                    return default;
                await using var f = File.Open(_path, FileMode.Open);
                return (T?)await JsonSerializer.DeserializeAsync(f, typeof(T), JsonContext.Default);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return default;
            }
        }
    }
    
    public static IRuntimeStorageProvider<PersistentState> CasinoStorage { get; set; }
        = new DefaultSettingsStore<PersistentState>();
}