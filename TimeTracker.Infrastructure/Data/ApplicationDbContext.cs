// TimeTracker.Infrastructure.Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TimeEntry> TimeEntries { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Vous n'avez plus besoin de mapper ApplicationUser.UserName
        // Identity s'occupe déjà de UserName → AspNetUsers.UserName

        builder.Entity<TimeEntry>()
               .HasOne(te => te.User)         // flèche vers la propriété navigation
               .WithMany(u => u.TimeEntries)
               .HasForeignKey(te => te.UserId);
    }
}
