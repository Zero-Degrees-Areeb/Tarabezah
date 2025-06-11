using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Tarabezah.Data.Context;

// This class is used by Entity Framework Core tools like migrations
public class TarabezahDbContextFactory : IDesignTimeDbContextFactory<TarabezahDbContext>
{
    public TarabezahDbContext CreateDbContext(string[] args)
    {
        try
        {
            // Find the project root directory with appsettings.json
            var basePath = Directory.GetCurrentDirectory();
            var webProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "../Tarabezah.Web");
            
            string configPath;
            if (Directory.Exists(webProjectPath) && File.Exists(Path.Combine(webProjectPath, "appsettings.json")))
            {
                configPath = webProjectPath;
                Console.WriteLine($"Using configuration from: {configPath}");
            }
            else
            {
                configPath = basePath;
                Console.WriteLine($"Using configuration from current directory: {configPath}");
            }

            // Get the configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(configPath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TarabezahDbContext>();
            
            // Use the connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in appsettings.json");
            }
            
            Console.WriteLine($"Using connection string: {connectionString}");
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Tarabezah.Data"));

            return new TarabezahDbContext(optionsBuilder.Options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating DbContext: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
            }
            throw;
        }
    }
} 