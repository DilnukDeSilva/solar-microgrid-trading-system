/*
 * File: Program.cs
 * Description: Composition root for the FAT Web API: pipeline, Mongo, JWT, Swagger and seed.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using SmartSolar.Api.Data;
using SmartSolar.Api.Extensions;
using SmartSolar.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSmartSolarServices(builder.Configuration);
builder.Services.AddSmartSolarJwt(builder.Configuration);
builder.Services.AddSmartSolarSwagger();
builder.Services.AddSmartSolarCors();

var app = builder.Build();

await EnsureDatabaseAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Solar API v1");
    options.RoutePrefix = "swagger";
});

// LAN/IIS hosting is HTTP. Redirecting to HTTPS would break phones and Swagger on the lab Wi-Fi.

app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Creates indexes and loads seed data once before the first HTTP request.
static async Task EnsureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var indexes = scope.ServiceProvider.GetRequiredService<MongoIndexInitializer>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await indexes.EnsureIndexesAsync();
    await seeder.SeedAsync();
}
