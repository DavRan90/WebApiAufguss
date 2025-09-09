using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Aufguss;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7162") });
        builder.Services.AddSweetAlert2();

        builder.Services.AddOidcAuthentication(options =>
        {
            // Configure your authentication provider options here.
            // For more information, see https://aka.ms/blazor-standalone-auth
            builder.Configuration.Bind("Local", options.ProviderOptions);
        });

        await builder.Build().RunAsync();
    }
}
