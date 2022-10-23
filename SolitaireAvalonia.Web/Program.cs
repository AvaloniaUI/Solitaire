﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SolitaireAvalonia.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static WebAssemblyHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
            
        builder.RootComponents.Add<App>("#app");
        
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        return builder;
    }
}