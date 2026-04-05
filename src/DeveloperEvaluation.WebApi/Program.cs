using DeveloperEvaluation.Application;
using DeveloperEvaluation.Common.HealthChecks;
using DeveloperEvaluation.Common.Logging;
using DeveloperEvaluation.Common.Security;
using DeveloperEvaluation.Common.Validation;
using DeveloperEvaluation.IoC;
using DeveloperEvaluation.ORM;
using DeveloperEvaluation.ORM.Extensions;
using DeveloperEvaluation.ORM.Seeding;
using DeveloperEvaluation.WebApi.Middleware;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

namespace DeveloperEvaluation.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            
            builder.AddDefaultLogging();

            builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                    opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
            
            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();
            
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Developer Evaluation API",
                    Version = "v1",
                    Description = @"Complete API for managing products, carts, users, branches, sales and authentication with JWT security.
                    
**Authentication:**
- Use POST /api/auth/login to get JWT token
- Click 'Authorize' button and enter: Bearer YOUR_TOKEN
                    
**Endpoints Available:**
- **Authentication**: Login endpoint (public)
- **Health**: Health check endpoint (public)
- **Products**: Full CRUD with pagination
- **Carts**: Full CRUD with pagination  
- **Users**: Full CRUD with pagination
- **Branches**: Full CRUD for branch management (Admin only)
- **Sales**: Complete sales management with business rules",
                    Contact = new OpenApiContact
                    {
                        Name = "Lucas Albuquerque",
                        Email = "lucas.albuquerque.gk@gmail.com"
                    }
                });

                // Include XML comments for better documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Ensure all controllers are included
                c.DocInclusionPredicate((name, api) => true);
                
                // Group by controller name
                c.TagActionsBy(api => 
                {
                    var controllerName = api.ActionDescriptor.RouteValues["controller"];
                    return new[] { controllerName ?? "Default" };
                });
                
                // Enable annotations
                c.EnableAnnotations();
                
                // Custom schema IDs to avoid conflicts
                c.CustomSchemaIds(type => type.FullName);
            });

            try
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                
                builder.Services.AddDbContext<DefaultContext>(options =>
                    options.UseNpgsql(
                        connectionString,
                        b => b.MigrationsAssembly("DeveloperEvaluation.ORM")
                    )
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to configure database context");
                throw;
            }

            try
            {
                builder.Services.AddCQRSInfrastructure(builder.Configuration);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to configure CQRS Infrastructure");
                throw;
            }

            try
            {
                builder.Services.AddJwtAuthentication(builder.Configuration);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to configure JWT Authentication");
                throw;
            }

            try
            {
                builder.RegisterDependencies();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to register dependencies");
                throw;
            }

            try
            {
                builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to configure AutoMapper");
                throw;
            }

            try
            {
                builder.Services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblies(
                        typeof(ApplicationLayer).Assembly,
                        typeof(Program).Assembly
                    );
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to configure MediatR");
                throw;
            }

            try
            {
                builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to add validation behavior");
                throw;
            }

            WebApplication app;
            try
            {
                app = builder.Build();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to build application");
                throw;
            }
            
            try
            {
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Developer Evaluation API v1");
                        c.RoutePrefix = "swagger";
                        c.DocumentTitle = "Developer Evaluation API";
                        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                        c.DisplayRequestDuration();
                        c.EnableDeepLinking();
                        c.EnableFilter();
                        c.ShowExtensions();
                        c.EnableValidator();
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to configure development environment");
                throw;
            }

            try
            {
                app.UseMiddleware<ValidationExceptionMiddleware>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to configure validation middleware");
                throw;
            }

            try
            {
                app.UseHttpsRedirection();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to add HTTPS redirection");
                throw;
            }

            try
            {
                app.UseAuthentication();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to add authentication middleware");
                throw;
            }

            try
            {
                app.UseAuthorization();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to add authorization middleware");
                throw;
            }

            try
            {
                app.UseBasicHealthChecks();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to add health checks middleware");
                throw;
            }

            try
            {
                app.MapControllers();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to map controllers");
                throw;
            }

            // Check if seeding was requested
            if (args.Length > 0 && args[0] == "--seed")
            {
                await SeedDataAsync(app);
                return;
            }

            // Seeding removed from automatic startup
            // Use: dotnet run --seed to execute seeding manually
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ APPLICATION TERMINATED UNEXPECTEDLY");
            Log.Fatal("Exception Type: {ExceptionType}", ex.GetType().Name);
            Log.Fatal("Exception Message: {Message}", ex.Message);
            Log.Fatal("Stack Trace: {StackTrace}", ex.StackTrace);
            throw; // Re-throw to see the full exception details
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task SeedDataAsync(WebApplication app)
    {
        try
        {
            
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();
            
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
            
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred during data seeding");
        }
    }
}
