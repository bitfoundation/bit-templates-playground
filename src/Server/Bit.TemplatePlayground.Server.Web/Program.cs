using Bit.TemplatePlayground.Server.Api.Data;
using Bit.TemplatePlayground.Server.Web.Services;
using Bit.TemplatePlayground.Client.Core.Services.Contracts;

namespace Bit.TemplatePlayground.Server.Web;

public static partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(options: new()
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        AppEnvironment.Set(builder.Environment.EnvironmentName);

        builder.Configuration.AddClientConfigurations(clientEntryAssemblyName: "Bit.TemplatePlayground.Client.Web");


        builder.AddServerWebProjectServices();

        var app = builder.Build();

        AppDomain.CurrentDomain.UnhandledException += (_, e) => LogException(e.ExceptionObject, reportedBy: nameof(AppDomain.UnhandledException), app);
        TaskScheduler.UnobservedTaskException += (_, e) => { LogException(e.Exception, reportedBy: nameof(TaskScheduler.UnobservedTaskException), app); e.SetObserved(); };

        if (builder.Environment.IsDevelopment())
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync(); // It's recommended to start using ef-core migrations.
        }

        app.ConfigureMiddlewares();

        #if Development
        _ = ScssCompilerService.WatchScssFiles(app);
#endif
        
        await app.RunAsync();
    }

    private static void LogException(object? error, string reportedBy, WebApplication app)
    {
        if (error is Exception exp)
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IExceptionHandler>().Handle(exp, parameters: new()
            {
                { nameof(reportedBy), reportedBy }
            }, displayKind: AppEnvironment.IsDev() ? ExceptionDisplayKind.NonInterrupting : ExceptionDisplayKind.None);
        }
        else
        {
            _ = Console.Error.WriteLineAsync(error?.ToString() ?? "Unknown error");
        }
    }
}
