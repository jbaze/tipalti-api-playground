using FluentValidation;
using Serilog;
using CleanApi.Commands;
using CleanApi.Services;
using CleanApi.Services.Interfaces;
using CleanApi.Validators;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Tipalti Invoice API (Clean Architecture)");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Register Controllers
    builder.Services.AddControllers();

    // Register MediatR for CQRS pattern
    // Automatically discovers and registers all IRequestHandler implementations
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetInvoicesCommand).Assembly));

    // Register InvoiceService as Singleton (data storage layer)
    builder.Services.AddSingleton<IInvoiceService, InvoiceService>();

    // Register ResponseHandlerService as Singleton (response handling service)
    builder.Services.AddSingleton<IResponseHandlerService, ResponseHandlerService>();

    // Register FluentValidation validators
    builder.Services.AddValidatorsFromAssemblyContaining<InvoiceRequestValidator>();

    // Add Swagger/OpenAPI support
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Tipalti Invoice API (Clean Architecture)",
            Version = "v1",
            Description = "A clean architecture implementation of the invoice API with MediatR Commands, Controllers, and Services separation",
            Contact = new()
            {
                Name = "Tipalti Development Team",
                Url = new Uri("https://tipalti.com")
            }
        });

        // Include XML comments for better API documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Add CORS policy for frontend apps
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontendApps", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",  // Angular dev server
                    "http://localhost:5173"   // Angular 2 dev server
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Enable Swagger
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tipalti Invoice API v1 (Clean)");
        options.RoutePrefix = string.Empty;
    });

    // Enable CORS
    app.UseCors("AllowFrontendApps");

    // Mock Authentication Middleware
    app.Use(async (context, next) =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader))
        {
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var tokenPreview = token.Length > 20 ? token.Substring(0, 20) + "..." : token;
                Log.Information("Request authenticated with token: {TokenPreview}", tokenPreview);
            }
            else
            {
                Log.Warning("Authorization header present but not in Bearer format");
            }
        }
        else
        {
            Log.Debug("No authorization header present (mock auth - continuing anyway)");
        }

        await next();
    });

    // Map Controllers
    app.MapControllers();

    Log.Information("API is ready. Swagger UI available at: http://localhost:{Port}",
        app.Configuration["ASPNETCORE_HTTP_PORTS"] ?? "5000");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
