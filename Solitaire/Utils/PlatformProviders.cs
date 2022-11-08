using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.VisualTree;
using Newtonsoft.Json;
using Solitaire.Models;
using Solitaire.ViewModels;

namespace Solitaire.Utils;

public static class VisualExtensions
{
    
    public static Rect TransformToVisualRect(this IVisual visual, IVisual relativeTo)
    {
        var sourceBounds = visual.Bounds;
        if (visual.TransformToVisual(relativeTo) is { } sourceTransform)
        {
            return sourceBounds.TransformToAABB(sourceTransform);
        }
        return default;
    }

}

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
            await using var isoStream = new IsolatedStorageFileStream(Identifier + key, FileMode.Create, isoStore);
            await using var writer = new StreamWriter(isoStream);
            await writer.WriteAsync(serializedObjJson);
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
                using var reader = new StreamReader(isoStream);
                var savedString = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(savedString)) return default;
                var storedObj = JsonConvert.DeserializeObject<T>(savedString);
                return storedObj ?? default;
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