using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Data.Sqlite;

namespace Bit.TemplatePlayground.Tests.Infrastructure;

[TestClass]
public partial class TestsAssemblyInitializer
{
    private static DistributedApplication? aspireApp;

    /// <summary>
    /// The running Aspire host - with real backing containers such as Redis
    /// Started by <see cref="RunAspireHost"/> during assembly initialization.
    /// </summary>
    internal static DistributedApplication AspireApp => aspireApp ?? throw new InvalidOperationException();

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
        var aspireAppBuilder = await DistributedApplicationTestingBuilder
            .CreateAsync<Program>(testContext.CancellationToken);

        foreach (var res in aspireAppBuilder.Resources.Where(r => r is ProjectResource or IResourceWithParent<ProjectResource>).ToList())
            aspireAppBuilder.Resources.Remove(res);

        // The following resources are not that much useful in tests and just add to the startup time, so we remove them from the application.
        // Matched by name because some of them (OtlpLoopbackResource, CloudflareTunnelInstallerResource) are internal types.
        string[] typeNamesToBeRemoved = [
            "OtlpLoopbackResource",
            nameof(Aspire.Hosting.ApplicationModel.CloudflareTunnelResource),
            nameof(Aspire.Hosting.ApplicationModel.CloudflareQuickTunnelResource),
            "CloudflareTunnelInstallerResource",
            nameof(SqliteWebResource),
        ];

        foreach (var res in aspireAppBuilder.Resources.Where(r => typeNamesToBeRemoved.Contains(r.GetType().Name)).ToList())
        {
            aspireAppBuilder.Resources.Remove(res);
        }

        aspireApp = await aspireAppBuilder.BuildAsync(testContext.CancellationToken);

        await aspireApp.StartAsync(testContext.CancellationToken);

        foreach (var connectionString in aspireAppBuilder.Resources.OfType<IResourceWithConnectionString>())
        {
            Environment.SetEnvironmentVariable($"ConnectionStrings__{connectionString.Name}", await aspireApp.GetConnectionStringAsync(connectionString.Name, testContext.CancellationToken));
            await aspireApp.ResourceNotifications.WaitForResourceAsync(connectionString.Name, [.. KnownResourceStates.TerminalStates, KnownResourceStates.Running], testContext.CancellationToken);
        }
    }

    // The app's SQLite database is file-based in every shipped configuration, so this keep-alive connection only
    // matters when ConnectionStrings__sqlite is overridden to an in-memory database (Mode=Memory), which lives
    // only as long as at least one connection to it stays open.
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
