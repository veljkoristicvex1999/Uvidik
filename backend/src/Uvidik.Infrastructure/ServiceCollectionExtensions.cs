using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uvidik.Infrastructure.Persistence;

namespace Uvidik.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Uvidik")
            ?? throw new InvalidOperationException("Connection string 'Uvidik' is not configured.");

        services.AddDbContext<UvidikDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
