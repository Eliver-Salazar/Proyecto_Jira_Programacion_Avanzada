using System;

using JiraClone.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JiraClone.Infrastructure.Data
{
    public class ApplicationIdentityDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationIdentityDbContext(
            DbContextOptions<ApplicationIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.AvatarPath)
                    .HasMaxLength(500);

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                entity.Property(u => u.CreatedAt)
                    .HasPrecision(3);

                entity.Property(u => u.UpdatedAt)
                    .HasPrecision(3);
            });
        }
    }
}
