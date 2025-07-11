// TimeTracker.Infrastructure.Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;

/// <summary>
/// AppDbContext est la "porte d'entrée" d'Entity Framework Core vers ta base de données.
/// Il gère la configuration des entités (tables), les relations, et la connexion à la base.
/// Dans cette app, il hérite de IdentityDbContext pour intégrer la gestion des utilisateurs/roles ASP.NET Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    /// <summary>
    /// DbSet pour les tokens de rafraîchissement (authentification JWT).
    /// EF Core générera une table RefreshTokens avec les propriétés définies dans l'entité.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; } = default!;

    // Ajoute ici d'autres DbSet pour tes entités métier (ex: Projects, Tasks, etc.)
    // public DbSet<Project> Projects { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configure les relations entre les entités, les clés, les contraintes, etc.
    /// Appelé automatiquement par EF Core au démarrage.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Vous n'avez plus besoin de mapper ApplicationUser.UserName
        // Identity s'occupe déjà de UserName → AspNetUsers.UserName

        builder.Entity<TimeEntry>()
               .HasOne(te => te.User)         // flèche vers la propriété navigation
               .WithMany(u => u.TimeEntries)
               .HasForeignKey(te => te.UserId);

        // Configuration de la relation RefreshToken <-> ApplicationUser
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
