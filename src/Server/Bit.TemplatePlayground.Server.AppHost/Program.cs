using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Check out appsettings.Development.json for credentials/passwords settings.

var sqlite = builder.AddSqlite("sqlite", databaseFileName: "Bit.TemplatePlaygroundDb.db")
    .WithSqliteWeb(config => config.WithVolume("/var/lib/sqliteweb/Bit.TemplatePlayground/data"));

var serverWebProject = builder.AddProject("serverweb", "../Bit.TemplatePlayground.Server.Web/Bit.TemplatePlayground.Server.Web.csproj")
    .WithExternalHttpEndpoints();

// Adding health checks endpoints to applications in non-development environments has security implications.
// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
if (builder.Environment.IsDevelopment())
{
    serverWebProject.WithHttpHealthCheck("/alive");
}


serverWebProject.WithReference(sqlite).WaitFor(sqlite);

if (builder.ExecutionContext.IsRunMode) // The following project is only added for testing purposes.
{
    // Blazor WebAssembly Standalone project.
    builder.AddProject("clientwebwasm", "../../Client/Bit.TemplatePlayground.Client.Web/Bit.TemplatePlayground.Client.Web.csproj")
        .WithExplicitStart();

    var mailpit = builder.AddMailPit("smtp") // For testing purposes only, in production, you would use a real SMTP server.
        .WithDataVolume("mailpit");

    serverWebProject.WithReference(mailpit);

    // Blazor Hybrid Windows project.
    builder.AddProject("clientwindows", "../../Client/Bit.TemplatePlayground.Client.Windows/Bit.TemplatePlayground.Client.Windows.csproj")
        .WithExplicitStart();


    var tunnel = builder.AddDevTunnel("web-dev-tunnel")
        .WithAnonymousAccess()
        .WithReference(serverWebProject.WithHttpEndpoint(name: "devTunnel").GetEndpoint("devTunnel"));
}

await builder
    .Build()
    .RunAsync();
