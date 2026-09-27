/*
 * File: Program.cs
 * Description: Composition root for the MVC web client. No MongoDB and no business rules live here.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authentication.Cookies;
using SmartSolar.Web.Api;
using SmartSolar.Web.Filters;
using SmartSolar.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "SmartSolar.Web.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "SmartSolar.Web";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddScoped<IWebSession, WebSession>();
builder.Services.AddScoped<ApiUnauthorizedFilter>();
builder.Services.AddHttpClient<ApiClient>(ConfigureApiClient);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiUnauthorizedFilter>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Points the shared HttpClient at the FAT API base URL from configuration.
static void ConfigureApiClient(IServiceProvider services, HttpClient client)
{
    var baseUrl = services.GetRequiredService<IConfiguration>()["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl is missing from configuration.");

    if (!baseUrl.EndsWith('/'))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}
