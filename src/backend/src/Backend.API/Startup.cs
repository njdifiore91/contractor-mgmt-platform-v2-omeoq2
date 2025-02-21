using Backend.API.Filters;
using Backend.API.Middleware;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend.API
{
    /// <summary>
    /// Configures the ASP.NET Core application services and middleware pipeline
    /// with enterprise-grade features and security optimizations.
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Configures application services with comprehensive security and performance features
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure SQL Server with connection pooling and retry policies
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        sqlOptions.MinBatchSize(10);
                        sqlOptions.MaxBatchSize(100);
                    });
            });

            // Configure API behavior and validation
            services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = false;
            });

            // Configure CORS with secure defaults
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", builder =>
                {
                    builder.WithOrigins(Configuration.GetSection("AllowedOrigins").Get<string[]>())
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .WithExposedHeaders("X-Pagination", "X-Total-Count")
                           .SetIsOriginAllowedToAllowWildcardSubdomains()
                           .AllowCredentials();
                });
            });

            // Configure response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = new[] { "application/json", "text/plain" };
            });

            // Configure response caching
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 64 * 1024; // 64KB
                options.UseCaseSensitivePaths = false;
            });

            // Configure memory cache
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024; // 1GB
                options.CompactionPercentage = 0.2;
            });

            // Configure rate limiting
            services.AddRateLimiting(Configuration);

            // Configure health checks
            services.AddHealthChecks()
                   .AddSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                   .AddCheck<StartupHealthCheck>("Startup");

            // Configure API versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            // Configure authorization options
            services.Configure<AuthorizationOptions>(options =>
            {
                options.CaseSensitiveRoleMatching = false;
                options.CaseSensitivePermissionMatching = false;
                options.EnableCaching = true;
                options.CacheDurationMinutes = 10;
            });

            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddEventSourceLogger();
                
                if (!Environment.IsDevelopment())
                {
                    builder.AddApplicationInsights(Configuration["ApplicationInsights:InstrumentationKey"]);
                }
            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline with security and performance middleware
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure exception handling based on environment
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Add custom exception handling middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Enable response compression
            app.UseResponseCompression();

            // Enable response caching
            app.UseResponseCaching();

            // Configure security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                await next();
            });

            // Enable HTTPS redirection
            app.UseHttpsRedirection();

            // Configure static files with caching
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append(
                        "Cache-Control", $"public, max-age=31536000");
                }
            });

            // Enable routing
            app.UseRouting();

            // Configure CORS
            app.UseCors("DefaultPolicy");

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure rate limiting
            app.UseRateLimiting();

            // Configure endpoints
            app.UseEndpoints(endpoints =>
            {
                // API endpoints
                endpoints.MapControllers();

                // Health check endpoint
                endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResponseWriter = WriteHealthCheckResponse
                });

                // Fallback for SPA
                endpoints.MapFallbackToFile("index.html");
            });
        }

        /// <summary>
        /// Writes a detailed health check response
        /// </summary>
        private static Task WriteHealthCheckResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries,
                duration = report.TotalDuration
            };

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    /// <summary>
    /// Health check that verifies the application has completed startup
    /// </summary>
    public class StartupHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
    {
        private volatile bool _isReady = false;

        public bool StartupCompleted
        {
            get => _isReady;
            set => _isReady = value;
        }

        public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
            System.Threading.CancellationToken cancellationToken = default)
        {
            if (_isReady)
            {
                return Task.FromResult(
                    Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("The startup task has completed."));
            }

            return Task.FromResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("The startup task is still running."));
        }
    }
}