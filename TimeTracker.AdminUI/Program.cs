using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using TimeTracker.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// ---------- DB / Identity ----------
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ❌ Supprime AddAuthentication().AddCookie(...)
// ✅ Configure le cookie Identity (schéma: Identity.Application)
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.LogoutPath = "/Account/Logout";
    opt.AccessDeniedPath = "/Account/AccessDenied";
    opt.ExpireTimeSpan = TimeSpan.FromHours(48);
    opt.SlidingExpiration = true;
    opt.Cookie.HttpOnly = true;
    opt.Cookie.SameSite = SameSiteMode.Strict;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // Optionnel :
    // opt.Cookie.Name = ".TimeTracker.Identity";
});

builder.Services.AddAuthorization(o =>
    o.AddPolicy("RequireAdminRole", p => p.RequireRole("Admin")));

// ---------- Localisation (SharedResource unique) ----------
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AddPageRoute("/Home", "");                  // "/" -> Home
        options.Conventions.AddPageRoute("/Admin/AdminDashboard", "admin");
        options.Conventions.AddPageRoute("/UserPage", "user");
    });

// Cultures supportées
var supportedCultures = new[] { "fr", "en", "it" }.Select(c => new CultureInfo(c)).ToArray();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("fr");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// ---------- JSON global ----------
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opt.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// ---------- HttpClient (Bearer depuis cookie jwt_token) ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtCookieAuthHandler>();

var apiBase = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl manquant dans appsettings.json.");

builder.Services.AddHttpClient("TimeTrackerAPI", c =>
{
    c.BaseAddress = new Uri(apiBase);
    c.DefaultRequestHeaders.Accept.Clear();
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<JwtCookieAuthHandler>()
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        // DEV seulement — retire en PROD
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

var app = builder.Build();

// ---------- Pipeline ----------
app.UseRequestLocalization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();

public sealed class JwtCookieAuthHandler(IHttpContextAccessor http) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var jwt = http.HttpContext?.Request?.Cookies["jwt_token"];
        if (!string.IsNullOrWhiteSpace(jwt))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return base.SendAsync(request, ct);
    }
}
