using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Tarabezah.Application.Commands.CreateFloorplan;
using Tarabezah.Application.Services;
using Tarabezah.Infrastructure;
using Tarabezah.Infrastructure.SignalR;
using Tarabezah.Web.Middleware;
using Tarabezah.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var jordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["TimeZone"]);
builder.Services.AddSingleton(jordanTimeZone);

// Add services to the container.
builder.Services.AddControllers();

// Register application services
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateFloorplanCommand).Assembly));

// Register infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Register SignalR
builder.Services.AddSignalR();

// Register notification service - use the existing implementation
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// Environment-specific configurations
if (builder.Environment.IsProduction())
{
    // Configure response compression for production
    if (builder.Configuration.GetValue<bool>("Performance:EnableResponseCompression"))
    {
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
        });
    }

    // Configure caching
    builder.Services.AddMemoryCache();
}

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tarabezah API",
        Version = "v1",
        Description = "API for Tarabezah restaurant management system"
    });

    // Use fully qualified type names to avoid schema ID conflicts
    c.CustomSchemaIds(type => type.FullName);

    // Add API Key Authorization
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key Authentication",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tarabezah API v1");
    });
}
else
{
    // Production environment
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    // Use response compression in production if enabled
    if (app.Configuration.GetValue<bool>("Performance:EnableResponseCompression"))
    {
        app.UseResponseCompression();
    }
}

app.UseHttpsRedirection();

// Add middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseRouting();

// Use CORS before authorization
app.UseCors("CorsPolicy");

app.UseAuthorization();

// Configure SignalR with appropriate CORS policy
app.MapHub<TarabezahHub>("/tarabezahHub").RequireCors("SignalRPolicy");

app.MapControllers();

// Log the current environment
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting in {Environment} environment", app.Environment.EnvironmentName);

app.Run();
