using System.Net;
using System.Net.Mail;
using System.ClientModel.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.OpenApi;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.OData;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Twilio;
using Ganss.Xss;
using Fido2NetLib;
using PhoneNumbers;
using FluentStorage;
using FluentEmail.Core;
using FluentStorage.Blobs;
using Hangfire.EntityFrameworkCore;
using AdsPush;
using AdsPush.Abstraction;
using Bit.TemplatePlayground.Server.Api.Features.Identity.Models;
using Bit.TemplatePlayground.Server.Api.Features.Identity.Services;
using Medallion.Threading;
using Bit.TemplatePlayground.Shared.Features.Identity;
using Bit.TemplatePlayground.Server.Api.Features.Statistics;
using Bit.TemplatePlayground.Shared.Infrastructure.Resources;
using Bit.TemplatePlayground.Server.Api.Infrastructure.RequestPipeline;
using Bit.TemplatePlayground.Server.Api.Features.PushNotification;
using Bit.TemplatePlayground.Server.Api.Infrastructure.Services;
using Bit.TemplatePlayground.Server.Api.Features.Products;

namespace Bit.TemplatePlayground.Server.Api;

public static partial class Program
{
    public static void AddServerApiProjectServices(this WebApplicationBuilder builder)
    {
        // Services being registered here can get injected in server project only.
        var env = builder.Environment;
        var services = builder.Services;
        var configuration = builder.Configuration;

        builder.AddServerSharedServices();

        builder.AddServerApiHealthChecks();

        ServerApiSettings appSettings = new();
        configuration.Bind(appSettings);

        services.AddScoped<IdentityEmailService>();
        services.AddScoped<EmailServiceJobsRunner>();
        services.AddScoped<PhoneService>();
        services.AddScoped<PhoneServiceJobsRunner>();
        // Add MCP server with chatbot tools
        services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();
        services.AddScoped<Infrastructure.SignalR.AppChatbot>();
        if (appSettings.Sms?.Configured is true)
        {
            TwilioClient.Init(appSettings.Sms.TwilioAccountSid, appSettings.Sms.TwilioAutoToken);
        }

        services.AddSingleton(_ => PhoneNumberUtil.GetInstance());
        services.AddSingleton<IBlobStorage>(sp =>
        {
            var isRunningInsideDocker = Directory.Exists("/container_volume"); // It's supposed to be a mounted volume named /container_volume
            var appDataDirPath = Path.Combine(isRunningInsideDocker ? "/container_volume" : Directory.GetCurrentDirectory(), "App_Data");
            Directory.CreateDirectory(appDataDirPath);
            return StorageFactory.Blobs.DirectoryFiles(appDataDirPath);
        });


        services.AddHttpClient("APNS"); // Apple Push Notification Service
        services.AddHttpClient("Vapid"); // Web Push
        services.AddSingleton(sp =>
        {
            var adsPushSenderBuilder = new AdsPushSenderBuilder();

            if (string.IsNullOrEmpty(appSettings.AdsPushAPNS?.P8PrivateKey) is false)
            {
                adsPushSenderBuilder = adsPushSenderBuilder.ConfigureApns(appSettings.AdsPushAPNS, sp.GetRequiredService<IHttpClientFactory>().CreateClient("APNS"));
            }

            if (string.IsNullOrEmpty(appSettings.AdsPushFirebase?.PrivateKey) is false)
            {
                appSettings.AdsPushFirebase.PrivateKey = appSettings.AdsPushFirebase.PrivateKey.Replace(@"\n", string.Empty);

                adsPushSenderBuilder = adsPushSenderBuilder.ConfigureFirebase(appSettings.AdsPushFirebase, AdsPushTarget.Android);
            }

            if (string.IsNullOrEmpty(appSettings.AdsPushVapid?.PrivateKey) is false)
            {
                if (string.IsNullOrEmpty(appSettings.AdsPushVapid.PublicKey))
                    throw new InvalidOperationException("VAPID public key is required");
                if (string.IsNullOrEmpty(appSettings.AdsPushVapid.Subject))
                    throw new InvalidOperationException("VAPID subject is required"); // While it would work on Android, Windows, Linux, Apple requires subject, so we enforce it for all platforms to avoid confusion and potential issues.

                adsPushSenderBuilder = adsPushSenderBuilder.ConfigureVapid(appSettings.AdsPushVapid, sp.GetRequiredService<IHttpClientFactory>().CreateClient("Vapid"));
            }

            return adsPushSenderBuilder
                .BuildSender();
        });
        services.AddScoped<PushNotificationService>();
        services.AddScoped<PushNotificationJobRunner>();

        // Register distributed lock factory
        services.AddTransient(sp => new DistributedLockFactory((string lockKey) =>
        {
            return new Medallion.Threading.FileSystem.FileDistributedLock(new(Path.Combine(Path.GetTempPath(), $"Bit.TemplatePlayground-{lockKey}.lock")));
        }));

        services.AddSingleton<ServerExceptionHandler>();
        services.AddSingleton(sp => (IProblemDetailsWriter)sp.GetRequiredService<ServerExceptionHandler>());
        services.AddProblemDetails();

        services.AddCors(builder =>
        {
            CorsPolicyBuilder ApplyPolicyDefaults(CorsPolicyBuilder policy)
            {
                if (env.IsDevelopment() is false)
                {
                    policy.SetPreflightMaxAge(TimeSpan.FromDays(1)); // https://stackoverflow.com/a/74184331
                }

                ServerApiSettings settings = new();
                configuration.Bind(settings);

                policy.SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) && settings.IsTrustedOrigin(uri))
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .WithExposedHeaders(HeaderNames.RequestId,
                            HeaderNames.Age, "App-Cache-Response", "X-App-Platform", "X-App-Version", "X-Origin");

                return policy;
            }

            builder.AddDefaultPolicy(policy =>
            {
                ApplyPolicyDefaults(policy);
            });

            // Required for Cookies.Delete & Cookies.Append to work.
            builder.AddPolicy("CorsWithCredentials", policy =>
            {
                ApplyPolicyDefaults(policy)
                    .AllowCredentials();
            });
        });
        services.AddRateLimiter();

        services.AddSingleton(sp =>
        {
            JsonSerializerOptions options = new JsonSerializerOptions(AppJsonContext.Default.Options);

            options.TypeInfoResolverChain.Add(IdentityJsonContext.Default);
            options.TypeInfoResolverChain.Add(ServerJsonContext.Default);

            return options;
        });

        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.ApplyDefaultOptions());

        services.AddSingleton<HtmlSanitizer>();

        services
            .AddControllers(options => options.Filters.Add<AutoCsrfProtectionFilter>())
            .AddJsonOptions(options => options.JsonSerializerOptions.ApplyDefaultOptions())
            .AddApplicationPart(typeof(AppControllerBase).Assembly)
            .AddOData(options => options.EnableQueryFeatures())
            .AddDataAnnotationsLocalization(options => options.DataAnnotationLocalizerProvider = StringLocalizerProvider.ProvideLocalizer)
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    throw new ResourceValidationException(context.ModelState.Select(ms => (ms.Key, ms.Value!.Errors.Select(e => new LocalizedString(e.ErrorMessage, e.ErrorMessage)).ToArray())).ToArray());
                };
            });

        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc() // For API Controllers
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        var signalRBuilder = services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = env.IsDevelopment();
        }).AddJsonProtocol(options => options.PayloadSerializerOptions.ApplyDefaultOptions());

        if (string.IsNullOrEmpty(configuration["Azure:SignalR:ConnectionString"]) is false)
        {
            signalRBuilder.AddAzureSignalR(options =>
            {
                configuration.GetRequiredSection("Azure:SignalR").Bind(options);
            });
        }

        services.AddPooledDbContextFactory<AppDbContext>(AddDbContext);
        services.AddDbContextPool<AppDbContext>(AddDbContext);

        void AddDbContext(DbContextOptionsBuilder options)
        {
            options.EnableSensitiveDataLogging(env.IsDevelopment())
                .EnableDetailedErrors(env.IsDevelopment());

            var connectionStringBuilder = new SqliteConnectionStringBuilder(configuration.GetRequiredConnectionString("sqlite"));
            connectionStringBuilder.DataSource = Environment.ExpandEnvironmentVariables(connectionStringBuilder.DataSource);
            if (connectionStringBuilder.Mode is not SqliteOpenMode.Memory)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(connectionStringBuilder.DataSource)!);
            }
            options.UseSqlite(connectionStringBuilder.ConnectionString, dbOptions =>
            {
                // dbOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        }


        services.AddOptions<IdentityOptions>()
            .Bind(configuration.GetRequiredSection(nameof(ServerApiSettings.Identity)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ServerApiSettings>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            ServerApiSettings settings = new();
            configuration.Bind(settings);
            return settings;
        });

        services.AddEndpointsApiExplorer();

        services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;

            options.AddOperationTransformer(async (operation, context, cancellationToken) =>
            {
                var isAuthorizedAction = context.Description.ActionDescriptor.EndpointMetadata.Any(em => em is AuthorizeAttribute);
                var isODataEnabledAction = context.Description.ActionDescriptor.FilterDescriptors.Any(f => f.Filter is EnableQueryAttribute);

                operation.Parameters = [new OpenApiParameter()
                {
                    In = ParameterLocation.Header,
                    Name = HeaderNames.Authorization,
                    Example = "Bearer XXX.YYY...",
                    Description = "Get your JWT token by signin-in through Identity/SignIn endpoint",
                    Required = isAuthorizedAction
                }];

                if (isODataEnabledAction)
                {
                    operation.Parameters.AddRange([

                        new OpenApiParameter() { In = ParameterLocation.Query, Name = "$filter", Description = "Filters the results, based on a Boolean condition. (ex. Age gt 25)" },
                        new OpenApiParameter() { In = ParameterLocation.Query, Name = "$select", Description = "Returns only the selected properties. (ex. FirstName, LastName)" },
                        new OpenApiParameter() { In = ParameterLocation.Query, Name = "$expand", Description = "Include only the selected objects. (ex. Orders, Locations)" },
                        new OpenApiParameter() { In = ParameterLocation.Query, Name = "$search", Description = "Finds resources that match a search criteria. (ex. \"search term\")" },
                        new OpenApiParameter() { In = ParameterLocation.Query, Name = "$top", Description = "Returns only the first n items from a collection. (ex. 10)" },
                        new OpenApiParameter() { In = ParameterLocation.Query, Name = "$skip", Description = "Skips the first n items from a collection. (ex. 10)" },
                        new OpenApiParameter() { In = ParameterLocation.Query, Name = "$orderby", Description = "Orders the results of a query by one or more properties. (ex. Name desc)" }
                    ]);
                }
            });
        });

        services.AddDataProtection()
            .PersistKeysToDbContext<AppDbContext>()
            .ProtectKeysWithCertificate(AppCertificateService.GetAppCertificate(configuration));

        AddIdentity(builder);

        var emailSettings = appSettings.Email ?? throw new InvalidOperationException("Email settings are required.");
        var fluentEmailServiceBuilder = services.AddFluentEmail(emailSettings.DefaultFromEmail);
        fluentEmailServiceBuilder.AddSmtpSender(() =>
        {
            var smtpConnectionString = configuration.GetRequiredConnectionString("smtp")!;
            var endpoint = new Uri(GetConnectionStringValue(smtpConnectionString, "Endpoint", "localhost"));
            var host = endpoint.Host;
            var port = endpoint.Port is -1 ? 25 : endpoint.Port;
            var userName = GetConnectionStringValue(smtpConnectionString, "UserName", string.Empty);
            var password = GetConnectionStringValue(smtpConnectionString, "Password", string.Empty);
            var enableSsl = GetConnectionStringValue(smtpConnectionString, "EnableSsl", port == 465 || port == 587 ? "true" : "false") is not "false";

            SmtpClient smtpClient = new(host, port)
            {
                EnableSsl = enableSsl
            };

            if (string.IsNullOrEmpty(userName) is false
                && string.IsNullOrEmpty(password) is false)
            {
                smtpClient.Credentials = new NetworkCredential(userName.ToString(), password.ToString());
            }

            return smtpClient;
        });

        services.AddHttpClient<GoogleRecaptchaService>(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(10);
            c.BaseAddress = new Uri("https://www.google.com/recaptcha/");
        });

        services.AddHttpClient<NugetStatisticsService>(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(20);
            c.BaseAddress = new Uri("https://azuresearch-usnc.nuget.org");
            c.DefaultRequestVersion = HttpVersion.Version11;
        });

        services.AddHttpClient<ResponseCacheService>(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(10);
            c.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/zones/");
        });

        services.AddHttpClient("Keycloak", c =>
        {
            c.BaseAddress = new Uri(configuration["KEYCLOAK_HTTP"]
                ?? configuration["Authentication:Keycloak:KeycloakUrl"]
                ?? throw new InvalidOperationException("KEYCLOAK_HTTP configuration is required"));
            c.DefaultRequestVersion = HttpVersion.Version11;
        });

        services.AddFido2(options =>
        {

        });

        services.AddScoped(sp =>
        {
            var webAppUrl = sp.GetRequiredService<IHttpContextAccessor>()
                .HttpContext!.Request.GetWebAppUrl();

            var options = new Fido2Configuration
            {
                ServerDomain = webAppUrl.Host,
                TimestampDriftTolerance = 1000,
                ServerName = "Bit.TemplatePlayground WebAuthn",
                Origins = new HashSet<string>([webAppUrl.AbsoluteUri]),
                ServerIcon = new Uri(webAppUrl, "images/icons/bit-logo.png").ToString()
            };

            return options;
        });

        services.AddHttpClient("AI");

        if (string.IsNullOrEmpty(appSettings.AI?.OpenAI?.ChatApiKey) is false)
        {
            // https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.AI.OpenAI#microsoftextensionsaiopenai
            services.AddChatClient(sp => new OpenAI.Chat.ChatClient(model: appSettings.AI.OpenAI.ChatModel, credential: new(appSettings.AI.OpenAI.ChatApiKey), options: new()
            {
                Endpoint = appSettings.AI.OpenAI.ChatEndpoint,
                Transport = new HttpClientPipelineTransport(sp.GetRequiredService<IHttpClientFactory>().CreateClient("AI"))
            }).AsIChatClient())
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = env.IsDevelopment());
            // .UseDistributedCache()
        }
        else if (string.IsNullOrEmpty(appSettings.AI?.AzureOpenAI?.ChatApiKey) is false)
        {
            // https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.AI.AzureAIInference#microsoftextensionsaiazureaiinference
            services.AddChatClient(sp => new Azure.AI.Inference.ChatCompletionsClient(endpoint: appSettings.AI.AzureOpenAI.ChatEndpoint,
                credential: new Azure.AzureKeyCredential(appSettings.AI.AzureOpenAI.ChatApiKey),
                options: new()
                {
                    Transport = new Azure.Core.Pipeline.HttpClientTransport(sp.GetRequiredService<IHttpClientFactory>().CreateClient("AI"))
                }).AsIChatClient(appSettings.AI.AzureOpenAI.ChatModel))
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = env.IsDevelopment());
            // .UseDistributedCache()
        }

        if (string.IsNullOrEmpty(appSettings.AI?.OpenAI?.EmbeddingApiKey) is false)
        {
            services.AddEmbeddingGenerator(sp => new OpenAI.Embeddings.EmbeddingClient(model: appSettings.AI.OpenAI.EmbeddingModel, credential: new(appSettings.AI.OpenAI.EmbeddingApiKey), options: new()
            {
                Endpoint = appSettings.AI.OpenAI.EmbeddingEndpoint,
                Transport = new HttpClientPipelineTransport(sp.GetRequiredService<IHttpClientFactory>().CreateClient("AI"))
            }).AsIEmbeddingGenerator())
            .UseLogging()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = env.IsDevelopment());
            // .UseDistributedCache()
        }
        else if (string.IsNullOrEmpty(appSettings.AI?.AzureOpenAI?.EmbeddingApiKey) is false)
        {
            services.AddEmbeddingGenerator(sp => new Azure.AI.Inference.EmbeddingsClient(endpoint: appSettings.AI.AzureOpenAI.EmbeddingEndpoint,
                credential: new Azure.AzureKeyCredential(appSettings.AI.AzureOpenAI.EmbeddingApiKey),
                options: new()
                {
                    Transport = new Azure.Core.Pipeline.HttpClientTransport(sp.GetRequiredService<IHttpClientFactory>().CreateClient("AI"))
                }).AsIEmbeddingGenerator(appSettings.AI.AzureOpenAI.EmbeddingModel))
            .UseLogging()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = env.IsDevelopment());
            // .UseDistributedCache()
        }
        else if (string.IsNullOrEmpty(appSettings.AI?.HuggingFace?.EmbeddingEndpoint) is false)
        {
            services.AddEmbeddingGenerator(sp => new Microsoft.SemanticKernel.Connectors.HuggingFace.HuggingFaceEmbeddingGenerator(
                  new Uri(appSettings.AI.HuggingFace.EmbeddingEndpoint),
                  apiKey: appSettings.AI.HuggingFace.EmbeddingApiKey,
                  httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient("AI"), loggerFactory: sp.GetRequiredService<ILoggerFactory>()))
            .UseLogging()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = env.IsDevelopment());
            // .UseDistributedCache()
        }

        // Configure Hangfire to use Redis for persistent background job storage
        services.AddHangfire((sp, hangfireConfiguration) =>
        {
            if (appSettings.Hangfire?.UseIsolatedStorage is not true)
            {
                hangfireConfiguration.UseEFCoreStorage(AddDbContext, new()
                {
                    Schema = "jobs",
                    QueuePollInterval = new TimeSpan(0, 0, 1)
                });
            }
            else
            {
                hangfireConfiguration.UseEFCoreStorage(optionsBuilder =>
                {
                    var connectionString = "Data Source=Bit.TemplatePlaygroundJobs.db;Mode=Memory;Cache=Shared;";
                    var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
                    connection.Open();
                    AppContext.SetData("ReferenceTheKeepTheInMemorySQLiteDatabaseAlive", connection);
                    optionsBuilder.UseSqlite(connectionString);
                }, new()
                {
                    Schema = "jobs",
                    QueuePollInterval = new TimeSpan(0, 0, 1)
                })
                .UseDatabaseCreator();
            }

            hangfireConfiguration.UseRecommendedSerializerSettings();
            hangfireConfiguration.UseSimpleAssemblyNameTypeSerializer();
            hangfireConfiguration.UseIgnoredAssemblyVersionTypeResolver();
            hangfireConfiguration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
        });

        services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromSeconds(5);
            configuration.Bind("Hangfire", options);
        });
    }

    private static void AddIdentity(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        var env = builder.Environment;
        ServerApiSettings appSettings = new();
        configuration.Bind(appSettings);
        var identityOptions = appSettings.Identity;

        services.AddIdentity<User, Features.Identity.Models.Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<AppIdentityErrorDescriber>()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
            .AddApiEndpoints();

        services.AddScoped<UserClaimsService>();
        services.AddScoped<IUserConfirmation<User>, AppUserConfirmation>();
        services.AddScoped(sp => (IUserEmailStore<User>)sp.GetRequiredService<IUserStore<User>>());
        services.AddScoped(sp => (IUserPhoneNumberStore<User>)sp.GetRequiredService<IUserStore<User>>());
        services.AddScoped(sp => (AppUserClaimsPrincipalFactory)sp.GetRequiredService<IUserClaimsPrincipalFactory<User>>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<Microsoft.AspNetCore.Authentication.BearerToken.BearerTokenOptions>, AppBearerTokenOptionsConfigurator>());
        var authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.BearerScheme;
            options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
            options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
        })
        .AddBearerToken(IdentityConstants.BearerScheme /*Checkout AppBearerTokenOptionsConfigurator*/ );

        services.AddAuthorization();

        if (string.IsNullOrEmpty(configuration["Authentication:Google:ClientId"]) is false)
        {
            authenticationBuilder.AddGoogle(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.AdditionalAuthorizationParameters["prompt"] = "select_account";
                configuration.GetRequiredSection("Authentication:Google").Bind(options);
            });
        }

        if (string.IsNullOrEmpty(configuration["Authentication:GitHub:ClientId"]) is false)
        {
            authenticationBuilder.AddGitHub(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                configuration.GetRequiredSection("Authentication:GitHub").Bind(options);
            });
        }

        if (string.IsNullOrEmpty(configuration["Authentication:Twitter:ConsumerKey"]) is false)
        {
            authenticationBuilder.AddTwitter(options =>
            {
                options.RetrieveUserDetails = true;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                configuration.GetRequiredSection("Authentication:Twitter").Bind(options);
            });
        }

        if (string.IsNullOrEmpty(configuration["Authentication:Apple:ClientId"]) is false)
        {
            authenticationBuilder.AddApple(options =>
            {
                options.UsePrivateKey(keyId =>
                {
                    return env.ContentRootFileProvider.GetFileInfo("AppleAuthKey.p8");
                });
                configuration.GetRequiredSection("Authentication:Apple").Bind(options);
            });
        }

        if (string.IsNullOrEmpty(configuration["Authentication:AzureAD:ClientId"]) is false)
        {
            authenticationBuilder.AddMicrosoftIdentityWebApp(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Events = new()
                {
                    OnTokenValidated = async context =>
                    {
                        var props = new AuthenticationProperties();
                        props.Items["LoginProvider"] = "AzureAD";
                        await context.HttpContext.SignInAsync(IdentityConstants.ExternalScheme, context.Principal!, props);
                    }
                };
                configuration.GetRequiredSection("Authentication:AzureAD").Bind(options);
            }, openIdConnectScheme: "AzureAD");
        }

        if (string.IsNullOrEmpty(configuration["Authentication:Facebook:AppId"]) is false)
        {
            authenticationBuilder.AddFacebook(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                configuration.GetRequiredSection("Authentication:Facebook").Bind(options);
            });
        }

        var keycloakBaseUrl = configuration["KEYCLOAK_HTTP"]
            ?? configuration["Authentication:Keycloak:KeycloakUrl"];

        if (string.IsNullOrEmpty(keycloakBaseUrl) is false)
        {
            // In order to have better understanding of Keycloak integration, checkout .docs/07- ASP.NET Core Identity - Authentication & Authorization.md
            authenticationBuilder.AddOpenIdConnect("Keycloak", options =>
            {
                configuration.GetRequiredSection("Authentication:Keycloak").Bind(options);

                var realm = configuration["Authentication:Keycloak:Realm"] ?? throw new InvalidOperationException("Authentication:Keycloak:Realm configuration is required");

                options.Authority = $"{keycloakBaseUrl.TrimEnd('/')}/realms/{realm}";

                options.ResponseType = "code";
                options.ResponseMode = "query";

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("offline_access"); // To get refresh tokens

                options.MapInboundClaims = true;
                options.SaveTokens = true;

                options.Prompt = "login"; // Force login every time

                if (env.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });
        }

        services.ConfigureHttpClientFactoryForExternalIdentityProviders();
    }

    private static string GetConnectionStringValue(string connectionString, string key, string? defaultValue = null)
    {
        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            if (part.StartsWith($"{key}="))
                return part[$"{key}=".Length..];
        }
        return defaultValue ?? throw new ArgumentException($"Invalid connection string: '{key}' not found.");
    }
    
    private static WebApplicationBuilder AddServerApiHealthChecks(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        ServerApiSettings appSettings = new();
        configuration.Bind(appSettings);

        var healthChecksBuilder = builder.AddDefaultHealthChecks()
            .AddDbContextCheck<AppDbContext>(tags: ["live"])
            .AddHangfire(setup => setup.MinimumAvailableServers = 1, tags: ["live"])
            .AddCheck<UserProfileImagesStorageHealthCheck>("userProfileImages", tags: ["live"])
            .AddCheck<TwilioHealthCheck>("sms", tags: ["live"]);

        // Cloudflare Cache Purge API
        if (appSettings.Cloudflare?.Configured is true)
        {
            var cloudflareApiToken = appSettings.Cloudflare.ApiToken;
            healthChecksBuilder.AddUrlGroup(
                new Uri($"https://api.cloudflare.com/client/v4/zones/{appSettings.Cloudflare.ZoneId}"),
                name: "cloudflare",
                tags: ["ready"],
                configureClient: (_, client) =>
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {cloudflareApiToken}");
                });
        }

        var keycloakBaseUrl = configuration["KEYCLOAK_HTTP"] ?? configuration["Authentication:Keycloak:KeycloakUrl"];
        if (string.IsNullOrEmpty(keycloakBaseUrl) is false)
        {
            var realm = configuration["Authentication:Keycloak:Realm"] ?? "dev";
            healthChecksBuilder.AddUrlGroup(
                new Uri($"{keycloakBaseUrl.TrimEnd('/')}/realms/{realm}/.well-known/openid-configuration"),
                name: "keycloakIdentity",
                tags: ["ready"],
                configureClient: (_, client) => client.Timeout = TimeSpan.FromSeconds(10));
        }

        return builder;
    }
}
