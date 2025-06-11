using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using TimeTracker.Core.Entities;

namespace TimeTracker.AdminUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── 1) Enregistrer ApplicationDbContext pour Identity ────────────────────────
            // Veillez à utiliser la même chaîne de connexion que celle de votre API si vous partagez la même base.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ─── 2) Configurer ASP.NET Core Identity (cookie-based) ───────────────────────
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                // Si vous souhaitez conserver les mêmes règles qu’à l’API, reproduisez-les ici :
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // Vous pouvez également personnaliser Lockout, User, SignIn, etc., si besoin.
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ─── 3) Configurer l’authentification par cookie ───────────────────────────────
            // On ne configure pas JWT Bearer dans l’UI, car l’UI se base sur un cookie pour la session.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // Url de la page de login Razor
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                // Optionnel : durée de vie du cookie (48h par exemple)
                options.ExpireTimeSpan = TimeSpan.FromHours(48);
                options.SlidingExpiration = true;

                // Vous pouvez définir une AccessDeniedPath si vous avez des pages spéciales
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // ─── 4) Ajouter l’autorisation (si vous avez des policies selon les rôles) ────
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));
            });

            // ─── 5) Enregistrer Razor Pages ────────────────────────────────────────────────
            builder.Services.AddRazorPages();

            // ─── 6) Configurer IHttpClientFactory pour appeler l’API ───────────────────────
            // Dans appsettings.json, assurez‐vous d’avoir une section ApiSettings:BaseUrl,
            // par exemple : "ApiSettings": { "BaseUrl": "https://localhost:5001/" }
            var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("La clé 'ApiSettings:BaseUrl' est manquante ou vide dans appsettings.json.");
            }

            builder.Services.AddHttpClient("TimeTrackerAPI", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            var app = builder.Build();

            // ─── 7) Pipeline HTTP ───────────────────────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // IMPORTANT : Authentication puis Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Mappez Razor Pages (pour les pages Login, Register, Index, etc.)
            app.MapRazorPages();

            app.Run();
        }
    }
}
