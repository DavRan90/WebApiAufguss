using System.Globalization;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Aufguss;

public class Program
{
    public static async Task Main(string[] args)
    {
        var culture = new CultureInfo("sv-SE"); // Swedish, Monday is first day
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        
       builder.Services.AddSweetAlert2();


        // 1. Default HttpClient (unauthenticated)
        //builder.Services.AddHttpClient("UnauthenticatedClient", client =>
        //{
        //    client.BaseAddress = new Uri("https://localhost:7162");
        //});

        // 2. Authenticated HttpClient (adds JWT)
        builder.Services.AddScoped<AuthorizationMessageHandler>();

        builder.Services.AddHttpClient("AuthenticatedClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7162");
        }).AddHttpMessageHandler<AuthorizationMessageHandler>();


        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7162") });


        //builder.Services.AddScoped<AuthorizationMessageHandler>();

        //builder.Services.AddScoped(sp =>
        //{
        //    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
        //    return new HttpClient(handler)
        //    {
        //        BaseAddress = new Uri("https://localhost:7162")
        //    };
        //});

        

        builder.Services.AddAuthorizationCore();  // Required!
        builder.Services.AddScoped<AuthenticationStateProvider, Services.CustomAuthStateProvider>();


        builder.Services.AddOidcAuthentication(options =>
        {
            // Configure your authentication provider options here.
            // For more information, see https://aka.ms/blazor-standalone-auth
            builder.Configuration.Bind("Local", options.ProviderOptions);
        });

        await builder.Build().RunAsync();
    }
}
