using Microsoft.EntityFrameworkCore;
using PostService.Models;

namespace PostService.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options) // Change here
    {

        // DbSet representing the Users table in the database
        public required DbSet<User> Users { get; set; }

        // Override OnModelCreating if you need custom configurations for your models
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User model properties as needed
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id); // Set Id as primary key
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.Country).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasColumnType("bit"); // Use 'bit' type for boolean in SQL Server
            });
        }
    }
}
