// TimeTracker.API/Program.cs
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Mapping;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Infrastructure.Services; // <-- vérifiez le bon namespace

var builder = WebApplication.CreateBuilder(args);

// Enregistrer le DbContext qui doit hériter de IdentityDbContext<…>
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
    .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
);

// Enregistrer Identity COMPLET : UserManager, RoleManager, SignInManager, etc.
//    + Ajouter le store EF Core basé sur ApplicationDbContext
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>() // ← impératif : ApplicationDbContext hérite de IdentityDbContext
.AddDefaultTokenProviders();

// Configurer JWT Bearer (schéma “Bearer”)
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]!;

// Vous pouvez valider qu’aucune n’est vide ou nulle
if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience) ||
    string.IsNullOrWhiteSpace(jwtSecretKey))
{
    throw new InvalidOperationException("Vérifiez la configuration Jwt (Issuer, Audience, SecretKey).");
}
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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)
        )
    };
});

builder.Services
  .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.LoginPath = "/Account/Login";
      options.LogoutPath = "/Account/Logout";
      // options.ExpireTimeSpan, etc. selon vos besoins
  });

// Ajouter l’autorisation (politiques basées sur les rôles)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
});

// Enregistrement des repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();

builder.Services
    .AddAutoMapper(typeof(ApplicationMappingProfile).Assembly)
    .AddScoped<IEmployeeService, EmployeeService>()
    .AddScoped<IAuthService, AuthService>();
builder.Services
    .AddScoped<ITimeEntryService, TimeEntryService>();

// 5) Ajouter les contrôleurs (API)
builder.Services.AddControllers();

// (Optionnel) : Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── Seeder ────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    // Attention aux bons génériques ici !
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


//  Pipeline HTTP : swagger si dev, HTTPS, authentification/autorisation, map controllers
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// **L’ordre ici est fondamental :**
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
