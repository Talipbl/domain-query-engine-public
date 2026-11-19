using DomainQueryEngine.Application;
using DomainQueryEngine.Application.Lookup;
using DomainQueryEngine.Application.Services;
using DomainQueryEngine.Http;
using DomainQueryEngine.Infrastructure;
using DomainQueryEngine.PostgreSQL;
using DomainQueryEngine.SearchUI.Extensions;
using Microsoft.EntityFrameworkCore;
using Polly;
using IHttpClientFactory = DomainQueryEngine.Http.IHttpClientFactory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddRazorRuntimeCompilation();
builder.Services.AddSingleton<DomainQueryEngine.SearchUI.Helpers.SiteUrl>();

// Razor encodes everything outside Basic Latin by default ("ı" → "&#x131;"). Turkish text and symbols
// such as "≈" and "₺" are safe to emit as they are; HTML-special characters are still encoded.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options =>
    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All));

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

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<ILookupService, CachedLookupService>();
builder.Services.AddScoped<PriceComparisonService>();
builder.Services.AddScoped<DomainQueryEngine.SearchUI.Services.DomainSearchService>();
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

builder.Services.AddInfrastructureServices();
builder.Services.AddPostgreSQLServices();
builder.Services.AddDataSources(builder.Configuration);
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

// Turkish only for now: text comes from Resources/*.tr.resx and numbers and dates are formatted the
// same way on every server, whatever its own culture is. Add "en-US" here once English texts exist.
var supportedCultures = new[] { "tr-TR" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Lets the page tests start the app with WebApplicationFactory<Program>.
public partial class Program { }
