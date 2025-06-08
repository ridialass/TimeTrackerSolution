using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;

namespace TimeTracker.Infrastructure.Data
{
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

            // 1) Configure ApplicationUser.Role pour qu’il soit stocké en INT
            builder.Entity<ApplicationUser>()
                   .Property(u => u.Role)
                   .HasConversion<int>()                                  // enum → int
                   .HasDefaultValue(UserRole.Employee);                  // valeur par défaut

            // 2) Votre mapping TimeEntry ←→ ApplicationUser
            builder.Entity<TimeEntry>()
                   .HasOne(te => te.User)
                   .WithMany(u => u.TimeEntries)
                   .HasForeignKey(te => te.UserId);
        }
    }
}






