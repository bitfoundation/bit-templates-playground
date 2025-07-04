using Projects;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);


var serverWebProject = builder.AddProject<Bit_TemplatePlayground_Server_Web>("serverweb") // Replace . with _ if needed to ensure the project builds successfully.
    .WithExternalHttpEndpoints();




await builder
    .Build()
    .RunAsync();
