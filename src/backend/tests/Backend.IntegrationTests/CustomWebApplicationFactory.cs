using Backend.API;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.IntegrationTests
{
    /// <summary>
    /// Custom WebApplicationFactory that provides isolated test environments with in-memory database,
    /// mock authentication, and test-specific service configurations.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        /// <summary>
        /// Unique identifier for the test database instance
        /// </summary>
        public string TestDatabaseId { get; private set; }

        /// <summary>
        /// Service scope for managing test database lifetime
        /// </summary>
        private IServiceScope TestScope { get; set; }

        /// <summary>
        /// Initializes a new instance of CustomWebApplicationFactory with unique test database identifier
        /// </summary>
        public CustomWebApplicationFactory()
        {
            TestDatabaseId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Configures the web host with test-specific services and database context
        /// </summary>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing db context registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{TestDatabaseId}");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Configure test authentication
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(
                        "Test", options => { });

                // Configure test logging
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });

                // Create a new service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create scope to obtain reference to db context
                TestScope = serviceProvider.CreateScope();
                var scopedServices = TestScope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created and seeded
                db.Database.EnsureCreated();
                CreateTestDatabase(db);
            });
        }

        /// <summary>
        /// Creates and seeds the test database with comprehensive test data
        /// </summary>
        protected virtual void CreateTestDatabase(ApplicationDbContext context)
        {
            // Add test customers
            context.Customers.AddRange(new[]
            {
                new Core.Entities.Customer("TestUser")
                {
                    Name = "Test Customer 1",
                    Code = "TC1",
                    IsActive = true
                },
                new Core.Entities.Customer("TestUser")
                {
                    Name = "Test Customer 2",
                    Code = "TC2",
                    IsActive = true
                }
            });

            // Add test inspectors
            context.Inspectors.AddRange(new[]
            {
                new Core.Entities.Inspector
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@test.com",
                    InspectorId = "INS001",
                    Status = "Active",
                    State = "TX"
                },
                new Core.Entities.Inspector
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@test.com",
                    InspectorId = "INS002",
                    Status = "Active",
                    State = "CA"
                }
            });

            // Add test equipment
            context.Equipment.AddRange(new[]
            {
                new Core.Entities.Equipment
                {
                    Model = "Test Model 1",
                    SerialNumber = "SN001",
                    Condition = "New",
                    CreatedBy = "TestUser",
                    UpdatedBy = "TestUser"
                },
                new Core.Entities.Equipment
                {
                    Model = "Test Model 2",
                    SerialNumber = "SN002",
                    Condition = "Used",
                    CreatedBy = "TestUser",
                    UpdatedBy = "TestUser"
                }
            });

            // Add test users with different permission sets
            context.Users.AddRange(new[]
            {
                new Core.Entities.User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    EmailConfirmed = true,
                    Roles = new[] { "Admin" },
                    Permissions = new[] { "EditUsers", "EditCodes", "EditLinks" }
                },
                new Core.Entities.User
                {
                    FirstName = "Regular",
                    LastName = "User",
                    Email = "user@test.com",
                    EmailConfirmed = true,
                    Roles = new[] { "User" },
                    Permissions = new[] { "ViewUsers", "ViewEquipment" }
                }
            });

            context.SaveChanges();
        }

        /// <summary>
        /// Cleans up test database and resources after test completion
        /// </summary>
        public async Task CleanupTestDatabase()
        {
            if (TestScope != null)
            {
                var context = TestScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureDeletedAsync();
                await TestScope.DisposeAsync();
                TestScope = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TestScope?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    // Test Authentication Infrastructure
    public class TestAuthenticationOptions : Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions { }

    public class TestAuthenticationHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<TestAuthenticationOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<TestAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("permission", "EditUsers")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}