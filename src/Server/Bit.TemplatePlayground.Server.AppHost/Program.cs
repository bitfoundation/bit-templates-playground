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

builder.ExposeWildcardEndpointsToLan();

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

    // Every container is created from scratch on each run and is destroyed as soon as the app host stops.
    // Uncommenting the following line keeps them alive and reuses them instead, which makes starting the project
    // (F5 / `aspire start`) and running the automated tests considerably faster.
    // The costs are that those containers keep consuming memory even while you're not debugging the project (you can
    // stop them from Docker Desktop whenever you need those resources back)
    // Check out the `.docs/20- .NET Aspire.md` file for more details.

    //builder.UsePersistentContainers();

}

await builder
    .Build()
    .RunAsync();
