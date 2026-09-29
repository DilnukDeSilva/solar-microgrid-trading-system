/*
 * File: ServiceCollectionExtensions.cs
 * Description: Registers Mongo, repositories, services, JWT, Swagger and CORS.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 *
 * JWT bearer events adapted from Microsoft docs:
 * https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn
 * Swagger bearer button adapted from Swashbuckle v10 docs:
 * https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/configure-and-customize-swaggergen.md
 */

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SmartSolar.Api.Common;
using SmartSolar.Api.Configuration;
using SmartSolar.Api.Data;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Repositories;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "ClientApps";

    // Registers every application service behind the FAT Web API.
    public static IServiceCollection AddSmartSolarServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<MongoContext>();
        services.AddSingleton<MongoIndexInitializer>();
        services.AddSingleton<DatabaseSeeder>();

        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IStationRepository, StationRepository>();
        services.AddSingleton<ISlotRepository, SlotRepository>();
        services.AddSingleton<IReservationRepository, ReservationRepository>();

        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ProsumerService>();
        services.AddScoped<IProsumerService>(provider => provider.GetRequiredService<ProsumerService>());
        services.AddScoped<IReservationService, ReservationService>();
        services.AddSingleton<IReservationQueryRepository, ReservationQueryRepository>();
        services.AddScoped<IReservationQueryService, ReservationQueryService>();
        services.AddScoped<IOperatorService, OperatorService>();
        services.AddSingleton<IActiveAccountGuard, ActiveAccountGuard>();

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var message = context.ModelState.Values
                        .SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage)
                        .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
                        ?? "Validation failed.";

                    return new BadRequestObjectResult(new ApiErrorDto(ErrorCodes.ValidationError, message));
                };
            });

        return services;
    }

    // Configures JWT bearer auth so [Authorize(Roles=...)] reads the `role` claim.
    public static IServiceCollection AddSmartSolarJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters. Set Jwt__Key, user-secrets, or appsettings.Development.json.");
        }

        if (string.Equals(jwt.Key, JwtSettings.PlaceholderKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Jwt:Key is still the example placeholder. Set a random secret of at least 32 characters via Jwt__Key, user-secrets, or appsettings.Production.json.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim names as `sub`, `role`, `nic` (API contract). Default inbound mapping
                // would rename `role` and break [Authorize(Roles)].
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = "role",
                    NameClaimType = "sub"
                };

                options.Events = new JwtBearerEvents
                {
                    // Re-check Users.status so a deactivated account cannot keep using an old JWT.
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirst("sub")?.Value;
                        var guard = context.HttpContext.RequestServices.GetRequiredService<IActiveAccountGuard>();
                        if (!await guard.IsActiveAsync(userId, context.HttpContext.RequestAborted))
                        {
                            context.Fail("Account is not active.");
                        }
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var body = JsonSerializer.Serialize(
                            new ApiErrorDto(ErrorCodes.Unauthorized, "Authentication is required or the token is invalid."),
                            JsonDefaults.Options);
                        await context.Response.WriteAsync(body);
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var body = JsonSerializer.Serialize(
                            new ApiErrorDto(ErrorCodes.Forbidden, "You do not have permission to access this resource."),
                            JsonDefaults.Options);
                        await context.Response.WriteAsync(body);
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    // Enables Swagger UI with an Authorize button that sends Bearer tokens.
    public static IServiceCollection AddSmartSolarSwagger(this IServiceCollection services)
    {
        const string schemeId = "Bearer";

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Smart Solar Microgrid API",
                Version = "v1",
                Description = "FAT service for the SE4040 EAD assignment. All business rules live here."
            });

            options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste the JWT from POST /api/auth/login. Swagger adds the Bearer prefix."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeId, document)] = []
            });
        });

        return services;
    }

    // Allows the MVC web app on LAN. Mobile apps do not use CORS.
    public static IServiceCollection AddSmartSolarCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        return services;
    }
}
