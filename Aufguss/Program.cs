using System.Globalization;
using Aufguss.Services;
using Aufguss.Services.API;
using Aufguss.Services.Demo;
using Aufguss.Services.Interface;
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
        var culture = new CultureInfo("sv-SE");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddSweetAlert2();

        bool useDemo = builder.Configuration.GetValue<bool>("UseDemoData");

        if (useDemo)
        {
            builder.Services.AddScoped<IAuthService, DemoAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider, DemoAuthenticationStateProvider>();
            builder.Services.AddScoped<IEntryService, DemoEntryService>();
            builder.Services.AddScoped<IFaqService, DemoFaqService>();
            builder.Services.AddScoped<IEventService, DemoEventService>();
            builder.Services.AddScoped<IBookingService, DemoBookingService>();
            builder.Services.AddScoped<IUserService, DemoUserService>();
            builder.Services.AddScoped<IInviteService, DemoInviteService>();
            builder.Services.AddScoped<ISettingsService, DemoSettingsService>();
            builder.Services.AddScoped<ITemplateService, DemoTemplateService>();
            builder.Services.AddScoped<IImageService, DemoImageService>();
        }
        else
        {
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<IAuthService, CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<IEntryService, ApiEntryService>();
            builder.Services.AddScoped<IFaqService, ApiFaqService>();
            builder.Services.AddScoped<IEventService, ApiEventService>();
            builder.Services.AddScoped<IBookingService, ApiBookingService>();
            builder.Services.AddScoped<IUserService, ApiUserService>();
            builder.Services.AddScoped<IInviteService, ApiInviteService>();
            builder.Services.AddScoped<ISettingsService, ApiSettingsService>();
            builder.Services.AddScoped<ITemplateService, ApiTemplateService>();
            builder.Services.AddScoped<IImageService, ApiImageService>();
        }

        builder.Services.AddScoped<Services.AuthorizationMessageHandler>();

        builder.Services.AddHttpClient("AuthenticatedClient", client =>
        {
            client.BaseAddress = new Uri("https://aufgussapi-d6f4dae9g9h3a2h8.northeurope-01.azurewebsites.net");
        }).AddHttpMessageHandler<Services.AuthorizationMessageHandler>();

        
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://aufgussapi-d6f4dae9g9h3a2h8.northeurope-01.azurewebsites.net") });


        builder.Services.AddOidcAuthentication(options =>
        {
            // Configure your authentication provider options here.
            // For more information, see https://aka.ms/blazor-standalone-auth
            builder.Configuration.Bind("Local", options.ProviderOptions);
        });

        await builder.Build().RunAsync();
    }
}
