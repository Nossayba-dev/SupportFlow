using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SupportFlow.Data;
using System.Linq;

namespace SupportFlowTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the old DbContextOptions registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<SupportFlowDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // NEW for EF Core 9: also remove IDbContextOptionsConfiguration<> registrations
                var optionsConfig = services.Where(r =>
                    r.ServiceType.IsGenericType &&
                    r.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>))
                    .ToArray();

                foreach (var option in optionsConfig)
                {
                    services.Remove(option);
                }

                services.AddDbContext<SupportFlowDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });
            });

        }
    }
}