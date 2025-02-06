using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using specmatic_uuid_api.Data;
using System.Linq;

namespace specmatic_uuid_api_test.contract
{
    public class ServiceFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _connectionString;

        public ServiceFactory(PostgresTestContainer postgresContainer)
        {
            _connectionString = postgresContainer.ConnectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8080, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                });
            });

            builder.UseKestrel();

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Register DbContext with PostgreSQL Test Container
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(_connectionString));

                // Apply migrations automatically
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            });

            builder.UseKestrel(options =>
                {
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
                }
            );
        }
    }
}