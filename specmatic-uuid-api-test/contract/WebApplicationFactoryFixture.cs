using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace specmatic_uuid_api_test.contract;

public class WebApplicationFactoryFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private string? HostUrl { get; } = GetHostUrlFromLaunchSettings(); 

    private static string? GetHostUrlFromLaunchSettings()
    {
        var launchSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "launchSettings.json");
        if (!File.Exists(launchSettingsPath)) return null;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(launchSettingsPath, optional: false)
            .Build();

        return configuration["profiles:Http:applicationUrl"];
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(HostUrl); 
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel()).Build().Start();
        return host;
    }
}