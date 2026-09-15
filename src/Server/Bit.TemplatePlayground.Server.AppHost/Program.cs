using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Check out appsettings.Development.json for credentials/passwords settings.


var sqlite = builder.AddSqlite();


var keycloak = builder.AddKeycloak();

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

// cloudflared connects straight to the projects (no reverse proxy) - possible now that RemoveWildcardEndpoints drops http2.
builder.AddCloudflareTunnels(serverWebProject
    );

if (builder.ExecutionContext.IsRunMode) // The following project is only added for testing purposes.
{
    // Blazor WebAssembly Standalone project.
    builder.AddProject("clientwebwasm", "../../Client/Bit.TemplatePlayground.Client.Web/Bit.TemplatePlayground.Client.Web.csproj")
        .WithExplicitStart();

    var mailpit = builder.AddMailPit("smtp") // For testing purposes only, in production, you would use a real SMTP server.
        .WithOtlpExporter()
        .WithDataVolume("mailpit");

    serverWebProject.WithReference(mailpit);

    if (OperatingSystem.IsWindows())
    {
        // Blazor Hybrid Windows project.
        builder.AddProject("clientwindows", "../../Client/Bit.TemplatePlayground.Client.Windows/Bit.TemplatePlayground.Client.Windows.csproj")
            .WithExplicitStart();
    }

    // By default every container is created from scratch on each run and is destroyed as soon as the app host stops.
    // UsePersistentContainers keeps them alive and reuses them instead, which makes starting the project
    // (F5 / `aspire start`) and running the automated tests considerably faster, at the cost of the memory they keep
    // consuming while you're not debugging (stop them from Docker Desktop whenever you need it back).
    // Inside a Dev Container / GitHub Codespaces it is always on: the containers run in its docker-in-docker, so they
    // never outlive the dev container itself. To have it on your own machine as well, remove the `if` below and keep the `builder.UsePersistentContainers();`.
    // Check out the `.docs/20- .NET Aspire.md` file for more details.

    var inDevContainer = Environment.GetEnvironmentVariable("REMOTE_CONTAINERS") is "true" || Environment.GetEnvironmentVariable("CODESPACES") is "true";
    if (inDevContainer)
    {
        builder.UsePersistentContainers();
    }


    builder.RemoveWildcardEndpoints();
}

await builder
    .Build()
    .RunAsync();
