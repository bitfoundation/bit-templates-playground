using Microsoft.EntityFrameworkCore;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Aspire.Hosting.DevTunnels;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Bit.TemplatePlayground.Tests.Features.Identity;
using Bit.TemplatePlayground.Server.Api.Infrastructure.Data;

namespace Bit.TemplatePlayground.Tests.Infrastructure;

[TestClass]
public partial class TestsAssemblyInitializer
{
    private static DistributedApplication? aspireApp;

    [AssemblyInitialize]
    public static async Task Initialize(TestContext testContext)
    {
        await RunAspireHost(testContext);
        await using var testServer = new AppTestServer();

        await testServer.Build().Start(testContext.CancellationToken);

        await InitializeDatabase(testServer);
    }

    /// <summary>
    /// Aspire.Hosting.Testing executes the complete application, including dependencies like databases, 
    /// closely mimicking a production environment. However, it has a limitation: backend services cannot 
    /// be overridden in tests if needed, unlike <see cref="AppTestServer"/> used in <see cref="UITests"/> 
    /// and <see cref="IntegrationTests"/>. The code below runs the Aspire app without the server web 
    /// project, retrieves necessary connection strings (e.g., database connection string), and passes 
    /// them to <see cref="AppTestServer"/>, so you can override services in the server project.
    /// </summary>
    private static async Task RunAspireHost(TestContext testContext)
    {
        var aspireBuilder = await DistributedApplicationTestingBuilder
            .CreateAsync<Program>(testContext.CancellationToken);

        foreach (var res in aspireBuilder.Resources.OfType<ProjectResource>().ToList())
            aspireBuilder.Resources.Remove(res);
        foreach (var res in aspireBuilder.Resources.OfType<DevTunnelResource>().ToList()) // remove unnecessary resources.
            aspireBuilder.Resources.Remove(res);

        aspireApp = await aspireBuilder.BuildAsync(testContext.CancellationToken);

        await aspireApp.StartAsync(testContext.CancellationToken);

        foreach (var connectionString in aspireBuilder.Resources.OfType<IResourceWithConnectionString>())
        {
            Environment.SetEnvironmentVariable($"ConnectionStrings__{connectionString.Name}", await aspireApp.GetConnectionStringAsync(connectionString.Name, testContext.CancellationToken));
            await aspireApp.ResourceNotifications.WaitForResourceAsync(connectionString.Name, [.. KnownResourceStates.TerminalStates, KnownResourceStates.Running], testContext.CancellationToken);
        }
    }

    //SQLite database in in-memory mode only lives as long as at least one connection to it is open
    //This connection is required to keep the database alive during the test run.
    private static SqliteConnection connection = null!;
    private static async Task InitializeDatabase(AppTestServer testServer)
    {
        if (testServer.WebApp.Environment.IsDevelopment())
        {
            await using var scope = testServer.WebApp.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                connection = new SqliteConnection(dbContext.Database.GetConnectionString());
                await connection.OpenAsync();
            await dbContext.Database.EnsureCreatedAsync(); // It's recommended to start using ef-core migrations.
        }
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        if (aspireApp is not null)
        {
            await aspireApp.StopAsync();
            await aspireApp.DisposeAsync();
        }
    }
}
