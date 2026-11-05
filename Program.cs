using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AIAssistantSalesOperationsDashboard.Services;
using Syncfusion.Blazor;

namespace AIAssistantSalesOperationsDashboard;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder
            .RootComponents
            .Add<App>("#app")
            .Add<HeadOutlet>("head::after");

        // Add HttpClient
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        // Register Syncfusion Blazor service
        builder.Services.AddSyncfusionBlazor();

        // Register application services
        builder.Services.AddScoped<IOrderDataService, OrderDataService>();

        // Optional: Register Syncfusion license key (if using licensed version)
        // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY_HERE");

        await builder.Build().RunAsync();
    }
}
