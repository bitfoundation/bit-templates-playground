using Projects;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Check out appsettings.json for credential settings.


var serverWebProject = builder.AddProject<Bit.TemplatePlayground_Server_Web>("serverweb") // Replace . with _ if needed to ensure the project builds successfully.
    .WithExternalHttpEndpoints();

// Adding health checks endpoints to applications in non-development environments has security implications.
// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
if (builder.Environment.IsDevelopment())
{
    serverWebProject.WithHttpHealthCheck("/alive");
}




// Blazor WebAssembly Standalone project.
builder.AddProject<Bit.TemplatePlayground_Client_Web>("clientwebwasm"); // Replace . with _ if needed to ensure the project builds successfully.

if (builder.ExecutionContext.IsRunMode) // The following project is only added for testing purposes.
{
    // Blazor Hybrid Windows project.
    builder.AddProject<Bit.TemplatePlayground_Client_Windows>("clientwindows") // Replace . with _ if needed to ensure the project builds successfully.
        .WithExplicitStart();
}

builder.AddAspireDashboard();

await builder
    .Build()
    .RunAsync();
