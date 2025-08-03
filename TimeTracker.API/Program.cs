using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Mapping;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ─────── BDD & Identity ──────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ─────── JWT Bearer Authentication ───────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];
var jwtSecretKey = jwtSettings["SecretKey"];

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience) ||
    string.IsNullOrWhiteSpace(jwtSecretKey))
{
    throw new InvalidOperationException("Vérifiez la configuration Jwt (Issuer, Audience, SecretKey).");
}

// Seul JwtBearer doit être le schéma par défaut pour l’API REST
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };

    // ────── Ajout du filtrage des claims NameIdentifier ──────
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var identity = context.Principal?.Identity as ClaimsIdentity;
            if (identity != null)
            {
                // Filtre tous les claims NameIdentifier : on garde celui qui ressemble à un id numérique
                var nameIdClaims = identity.FindAll(ClaimTypes.NameIdentifier).ToList();

                if (nameIdClaims.Count > 1)
                {
                    foreach (var claim in nameIdClaims)
                    {
                        if (!int.TryParse(claim.Value, out _))
                        {
                            // Supprime ceux qui ne sont pas numériques (ex : le username)
                            identity.RemoveClaim(claim);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    };
});

// ─────── Authorization policies (optionnel) ──────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
});

// ─────── Services et DI ──────────────────────────────
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
builder.Services
    .AddAutoMapper(typeof(MappingProfile).Assembly)
    .AddScoped<IEmployeeService, EmployeeService>()
    .AddScoped<IAuthService, AuthService>()
    .AddScoped<ITokenService, TokenService>()
    .AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddTransient<LocalizationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ─────── MVC / Controllers ───────────────────────────
builder.Services.AddControllers();

// ─────── Swagger/OpenAPI ─────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TimeTracker API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ─────── Localisation middleware ─────────────────────
var supportedCultures = new[] { "en", "fr", "it" };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("it"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
});

// ─────── Seeder rôles/admin au démarrage ─────────────
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var roleName in Enum.GetNames<UserRole>())
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }

    const string adminRoleName = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        await roleManager.CreateAsync(new IdentityRole<int>(adminRoleName));
    }

    var admins = await userManager.GetUsersInRoleAsync(adminRoleName);
    if (admins.Count == 0)
    {
        var defaultAdmin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(defaultAdmin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(defaultAdmin, adminRoleName);
            Console.WriteLine("Admin créé : admin/Admin123!");
        }
        else
        {
            foreach (var err in result.Errors)
                Console.WriteLine($"Erreur création Admin : {err.Code} – {err.Description}");
        }
    }
}

// ─────── Pipeline HTTP ───────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }