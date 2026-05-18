using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore;
using WaterTracker.Api.Models;

namespace WaterTracker.Api.Data
{
    public class AppDbContext : IdentityDbContext<Customer>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<WaterIntake> WaterIntakes => Set<WaterIntake>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.UseOpenIddict();

            builder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.Forename).HasMaxLength(80).IsRequired();
                entity.Property(c => c.Surname).HasMaxLength(80).IsRequired();
                entity.Property(c => c.CreatedAt).IsRequired();
                entity.Property(c => c.UpdatedAt).IsRequired();
            });

            builder.Entity<WaterIntake>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasIndex(e => new { e.CustomerId, e.Date });
                entity.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}