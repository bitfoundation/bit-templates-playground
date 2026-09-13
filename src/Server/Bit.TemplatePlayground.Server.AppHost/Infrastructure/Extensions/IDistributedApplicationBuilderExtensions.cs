
namespace Aspire.Hosting;

public static class IDistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Adds a Keycloak identity server. In run mode the development realm of the <c>./Infrastructure/Realms</c>
        /// folder is imported into it; that realm seeds accounts with well-known passwords, so it must never reach a
        /// published application model.
        /// https://aspire.dev/integrations/security/keycloak/
        /// </summary>
        public IResourceBuilder<KeycloakResource> AddKeycloak()
        {
            // No explicit host port: every other container here lets Aspire allocate one, and a fixed port cannot be
            // held by two app hosts at once - which `aspire run` plus `dotnet test` on one machine already is.
            var keycloak = builder.AddKeycloak("keycloak")
                .WithDataVolume();

            if (builder.ExecutionContext.IsRunMode)
            {
                keycloak.WithRealmImport("./Infrastructure/Realms");
            }

            return keycloak;
        }





        /// <summary>
        /// Adds a SQLite database instance with a web-based management UI.
        /// </summary>
        public IResourceBuilder<SqliteResource> AddSqlite()
        {
            return builder.AddSqlite("sqlite", databaseFileName: "Bit.TemplatePlaygroundDb.db")
                .WithSqliteWeb();
        }


        /// <summary>
        /// Exposes the server projects through a Cloudflare Tunnel (cloudflared dials out, so the origin needs no
        /// public ip). Opt-in: does nothing unless the <c>cloudflare-tunnel-*</c> parameters are set (see appsettings.Development.json).
        /// </summary>
        public void AddCloudflareTunnels(
            IResourceBuilder<ProjectResource> serverWebProject
            )
        {
            var serverWebDomain = builder.Configuration["Parameters:cloudflare-tunnel-web-domain"];
            if (string.IsNullOrWhiteSpace(serverWebDomain) is false)
            {
                var tunnel = builder.AddCloudflareTunnel("cloudflare-tunnel-web");
                serverWebProject.WithCloudflareTunnel(tunnel, hostname: serverWebDomain);
            }
            else
            {
                builder.AddCloudflareQuickTunnel("cloudflare-tunnel-web")
                    .WithReference(serverWebProject);
            }

        }

        /// <summary>
        /// The <c>*</c> of a <c>http://*:5000</c> in launchSettings does not work with Aspire's proxy. This takes those
        /// endpoints out of the proxy and lets Kestrel bind them itself, so devices on the same local network can reach
        /// it. Run mode only.
        /// </summary>
        public IDistributedApplicationBuilder ExposeWildcardEndpointsToLan()
        {
            if (builder.ExecutionContext.IsRunMode is false)
                return builder;
            foreach (var project in builder.Resources.OfType<ProjectResource>().ToArray())
            {
                var endpoints = project.Annotations.OfType<EndpointAnnotation>().ToArray();
                foreach (var wildcard in endpoints.Where(endpoint => endpoint.TargetHost is "*"))
                {
                    if (wildcard.Port is not int port)
                    {
                        project.Annotations.Remove(wildcard); // No fixed port to expose.
                        continue;
                    }
                    var lanName = endpoints.Count(endpoint => endpoint.TargetHost is "*") is 1 ? "lan" : $"{wildcard.UriScheme}-lan";
                    wildcard.Name = lanName;
                    wildcard.TargetHost = "0.0.0.0";
                    wildcard.TargetPort = port; // Proxy-less endpoints must have Port == TargetPort.
                    wildcard.IsProxied = false;
                    wildcard.ExcludeReferenceEndpoint = true; // Keep 0.0.0.0 out of service discovery / tunnel references.

                    // Same port, same process: the localhost sibling(s) must be served by Kestrel too.
                    foreach (var sibling in endpoints.Where(endpoint => endpoint != wildcard && endpoint.UriScheme == wildcard.UriScheme && endpoint.Port == port))
                    {
                        sibling.TargetPort = port;
                        sibling.IsProxied = false;
                    }
                }
            }
            return builder;
        }

        /// <summary>
        /// Gives every container of the application model a persistent lifetime, so that they are created once and are
        /// then reused by every subsequent run, instead of being re-created and booted up from scratch each and every time.
        /// </summary>
        /// <remarks>
        /// Call it right before <see cref="IDistributedApplicationBuilder.Build"/>, so all the resources are already added
        /// while the application model is still mutable.
        /// </remarks>
        public IDistributedApplicationBuilder UsePersistentContainers()
        {
            foreach (var container in builder.Resources.OfType<ContainerResource>().ToArray())
            {
                builder.CreateResourceBuilder(container)
                    .WithEnvironment(context =>
                    {
                        // Aspire injects its own OTLP endpoint (https://aspire.dev.internal:<port>) into the containers,
                        // and that port is allocated again on every run. Since the environment variables are part of the
                        // container's lifecycle key, leaving them in place makes Aspire re-create every container on each
                        // run ("Found existing Container, but calculated lifecycle key doesn't match"), which defeats the
                        // whole purpose. Dropping them costs us the containers' telemetry in the Aspire dashboard only.
                        foreach (var otelVariable in context.EnvironmentVariables.Keys.Where(key => key.StartsWith("OTEL_")).ToArray())
                        {
                            context.EnvironmentVariables.Remove(otelVariable);
                        }
                    })
                    .WithLifetime(ContainerLifetime.Persistent);
            }

            return builder;
        }
    }
}
