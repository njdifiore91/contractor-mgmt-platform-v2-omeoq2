// Microsoft.AspNetCore.Hosting v6.0.0
using Microsoft.AspNetCore.Hosting;
// Microsoft.Extensions.Hosting v6.0.0
using Microsoft.Extensions.Hosting;
// Microsoft.Extensions.Configuration v6.0.0
using Microsoft.Extensions.Configuration;
// Microsoft.Extensions.Logging v6.0.0
using Microsoft.Extensions.Logging;
// Microsoft.ApplicationInsights.AspNetCore v2.21.0
using Microsoft.ApplicationInsights.AspNetCore;
// Microsoft.AspNetCore.HttpsPolicy v6.0.0
using Microsoft.AspNetCore.HttpsPolicy;
using Backend.API;
using Backend.API.Middleware;
using System;
using System.Threading.Tasks;

namespace Backend.API
{
    /// <summary>
    /// Enterprise-grade entry point for the ASP.NET Core web application with comprehensive
    /// security, monitoring, and scalability features.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Enhanced application entry point with comprehensive error handling and startup logging
        /// </summary>
        public static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();

                // Log application startup
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Starting application. Environment: {Environment}",
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                // Ensure critical startup errors are logged
                Console.Error.WriteLine($"Critical startup error: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Creates and configures an enterprise-grade web host builder with advanced security,
        /// monitoring, and performance features
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables()
                          .AddCommandLine(args);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                    logging.AddApplicationInsights();

                    if (!hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddEventLog();
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel(options =>
                        {
                            // Configure Kestrel with security and performance settings
                            options.AddServerHeader = false;
                            options.Limits.MaxConcurrentConnections = 100;
                            options.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB
                            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
                        })
                        .ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.Configure(options =>
                            {
                                options.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                                options.ConfigureHttpsDefaults(httpsOptions =>
                                {
                                    httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                                                              System.Security.Authentication.SslProtocols.Tls13;
                                });
                            });
                        })
                        .UseIIS()
                        .UseIISIntegration()
                        .ConfigureServices((context, services) =>
                        {
                            // Configure Application Insights
                            services.AddApplicationInsightsTelemetry(options =>
                            {
                                options.EnableAdaptiveSampling = true;
                                options.EnableQuickPulseMetricStream = true;
                                options.EnableHeartbeat = true;
                            });
                        })
                        .UseDefaultServiceProvider((context, options) =>
                        {
                            options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                            options.ValidateOnBuild = true;
                        });
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    // Enable dependency validation in development
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                    options.ValidateOnBuild = true;
                });
    }
}