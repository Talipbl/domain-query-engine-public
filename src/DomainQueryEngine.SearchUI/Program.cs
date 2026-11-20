using DomainQueryEngine.Application;
using DomainQueryEngine.Application.DomainSearch;
using DomainQueryEngine.Application.Services;
using DomainQueryEngine.Http;
using DomainQueryEngine.Infrastructure;
using DomainQueryEngine.PostgreSQL;
using DomainQueryEngine.SearchUI.Extensions;
using DomainQueryEngine.SearchUI.Helpers;
using DomainQueryEngine.SearchUI.Health;
using DomainQueryEngine.SearchUI.Hosting;
using DomainQueryEngine.SearchUI.Localization;
using DomainQueryEngine.SearchUI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.WebEncoders;
using Polly;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using IHttpClientFactory = DomainQueryEngine.Http.IHttpClientFactory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// UI texts come from Resources/Locales/{tr,en}.json (key/value) instead of .resx files.
builder.Services.AddLocalization();
builder.Services.Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory, JsonStringLocalizerFactory>());
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddRazorRuntimeCompilation();
builder.Services.AddSingleton<SiteUrl>();

// Razor encodes everything outside Basic Latin by default ("ı" → "&#x131;"). Turkish text and symbols
// such as "≈" and "₺" are safe to emit as they are; HTML-special characters are still encoded.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. " +
        "Set it with 'dotnet user-secrets' (development) or the 'ConnectionStrings__DefaultConnection' environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");
builder.Services.AddHostedService<DatabaseHealthCheckHostedService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<ILookupService, CachedLookupService>();
builder.Services.AddScoped<PriceComparisonService>();
builder.Services.AddScoped<DomainSearchService>();
builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
{
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(2, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));
});

builder.Services.AddDomainSearchInfrastructure();
builder.Services.AddTldPricingInfrastructure();
builder.Services.AddPostgreSQLServices();
builder.Services.AddTldPricingServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Apply pending migrations on startup (default). Tests turn this off with Database:MigrateOnStartup=false.
if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Error status codes without a body (e.g. 404) show the site's own page.
app.UseStatusCodePagesWithReExecute("/hata/{0}");

// Texts, numbers and dates follow the request culture (tr-TR or en-US), never the server's own culture.
// The language comes from ?culture=, the language cookie (header switch) or the browser's
// Accept-Language, in that order; Turkish otherwise.
var supportedCultures = LanguagePreference.Cultures;
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(LanguagePreference.DefaultCulture)
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHealthChecks("/health/db");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Lets the page tests start the app with WebApplicationFactory<Program>.
public partial class Program { }
