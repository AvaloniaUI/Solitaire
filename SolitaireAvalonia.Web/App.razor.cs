using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Web.Blazor;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SolitaireAvalonia.Utils;
using SolitaireAvalonia.ViewModels;

namespace SolitaireAvalonia.Web;

public partial class App
{
    private class BlazorSettingsStore<T> : IRuntimeStorageProvider<T>
    {
        private readonly IJSRuntime _js;
 
        public BlazorSettingsStore(IJSRuntime js)
        {
            _js = js;
            
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
            await _js.InvokeVoidAsync("localStorage.setItem", _ident, serializedObjJson);
        }

        /// <inheritdoc />
        public async Task<T?> LoadObject()
        {
            try
            {
                
                var _ident = typeof(T).FullName?.ToLowerInvariant().Replace(".", string.Empty) ?? "default";
                Console.WriteLine(_ident);
                
                var t = await _js.InvokeAsync<string>("localStorage.getItem",  _ident);
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

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        WebAppBuilder.Configure<SolitaireAvalonia.App>()
            .SetupWithSingleViewLifetime();
    }
}