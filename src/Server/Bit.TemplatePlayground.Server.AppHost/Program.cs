using Projects;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

// Check out appsettings.json for credential settings.


var serverWebProject = builder.AddProject<Bit_TemplatePlayground_Server_Web>("serverweb") // Replace . with _ if needed to ensure the project builds successfully.
    .WithExternalHttpEndpoints();




// Blazor WebAssembly Standalone project.
builder.AddProject<Bit_TemplatePlayground_Client_Web>("clientwebwasm"); // Replace . with _ if needed to ensure the project builds successfully.

if (builder.ExecutionContext.IsRunMode) // The following project is only added for testing purposes.
{
    // Blazor Hybrid Windows project.
    builder.AddProject<Bit_TemplatePlayground_Client_Windows>("clientwindows") // Replace . with _ if needed to ensure the project builds successfully.
        .WithExplicitStart();
}

builder.AddAspireDashboard();

await builder
    .Build()
    .RunAsync();
