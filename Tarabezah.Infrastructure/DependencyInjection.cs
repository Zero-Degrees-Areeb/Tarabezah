using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tarabezah.Data.Context;
using Tarabezah.Data.Repositories;
using Tarabezah.Domain.Repositories;
using Tarabezah.Infrastructure.Services;

namespace Tarabezah.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<TarabezahDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Tarabezah.Data")));

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IFloorplanRepository, FloorplanRepository>();
        services.AddScoped<IFloorplanElementRepository, FloorplanElementRepository>();
        services.AddScoped<ICombinedTableRepository, CombinedTableRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        
        // Register services
        services.AddScoped<IFileUploadService, CloudixFileUploadService>();
        
        // Register HTTP clients
        services.AddHttpClient(CloudixFileUploadService.ClientName, client =>
        {
            var baseUrl = configuration["ExternalServices:CloudixApi:BaseUrl"] ?? "https://cloudix.tarabezah.com";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
} 