using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Check out appsettings.Development.json for credentials/passwords settings.


var sqlite = builder.AddSqlite("sqlite", databaseFileName: "Bit.TemplatePlaygroundDb.db")
    .WithSqliteWeb(config => config.WithVolume("/var/lib/sqliteweb/Bit.TemplatePlayground/data"));

// https://aspire.dev/integrations/security/keycloak/
var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume()
    .WithRealmImport("./Realms");

var serverWebProject = builder.AddProject("serverweb", "../Bit.TemplatePlayground.Server.Web/Bit.TemplatePlayground.Server.Web.csproj")
    .WithExternalHttpEndpoints();

// Adding health checks endpoints to applications in non-development environments has security implications.
// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
if (builder.Environment.IsDevelopment())
{
    serverWebProject.WithHttpHealthCheck("/alive");
}


serverWebProject.WithReference(sqlite).WaitFor(sqlite);
serverWebProject.WithReference(keycloak);

if (builder.ExecutionContext.IsRunMode) // The following project is only added for testing purposes.
{
    // Blazor WebAssembly Standalone project.
    builder.AddProject("clientwebwasm", "../../Client/Bit.TemplatePlayground.Client.Web/Bit.TemplatePlayground.Client.Web.csproj")
        .WithExplicitStart();

    var mailpit = builder.AddMailPit("smtp") // For testing purposes only, in production, you would use a real SMTP server.
        .WithDataVolume("mailpit");

    serverWebProject.WithReference(mailpit);


    var tunnel = builder.AddDevTunnel("web-dev-tunnel")
        .WithAnonymousAccess()
        .WithReference(serverWebProject.WithHttpEndpoint(name: "devTunnel", port: 5000).GetEndpoint("devTunnel"));

    if (OperatingSystem.IsWindows())
    {
        // Blazor Hybrid Windows project.
        builder.AddProject("clientwindows", "../../Client/Bit.TemplatePlayground.Client.Windows/Bit.TemplatePlayground.Client.Windows.csproj")
            .WithExplicitStart();
    }

    // Blazor Hybrid MAUI project.
    var mauiapp = builder.AddMauiProject("mauiapp", @"../../Client/Bit.TemplatePlayground.Client.Maui/Bit.TemplatePlayground.Client.Maui.csproj");

    if (OperatingSystem.IsWindows())
    {
        mauiapp.AddWindowsDevice()
            .WithExplicitStart()
            .WithReference(serverWebProject);
    }

    if (OperatingSystem.IsMacOS())
    {
        mauiapp.AddMacCatalystDevice()
            .WithExplicitStart()
            .WithReference(serverWebProject);
    }

    if (OperatingSystem.IsMacOS())
    {
        // Windows supports iOS Simulator and Physical devices if there's a mac connected to network, but the following runners only work on macOS for now.

        mauiapp.AddiOSDevice()
            .WithExplicitStart()
            .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
            .WithReference(serverWebProject, tunnel);

        mauiapp.AddiOSSimulator()
            .WithExplicitStart()
            .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
            .WithReference(serverWebProject, tunnel);
    }

    mauiapp.AddAndroidDevice()
        .WithExplicitStart()
        .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
        .WithReference(serverWebProject, tunnel);

    mauiapp.AddAndroidEmulator()
        .WithExplicitStart()
        .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
        .WithReference(serverWebProject, tunnel);
}

await builder
    .Build()
    .RunAsync();
