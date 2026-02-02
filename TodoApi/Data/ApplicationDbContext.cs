using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add DbSet for Todos
    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Todo entity
        modelBuilder.Entity<Todo>(entity =>
        {
            // Primary key
            entity.HasKey(t => t.Id);

            // Indexes for performance
            entity.HasIndex(t => t.UserId)
                .HasDatabaseName("IX_Todos_UserId");

            entity.HasIndex(t => t.CreatedAt)
                .HasDatabaseName("IX_Todos_CreatedAt");

            // Foreign key relationship
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Required fields
            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Description)
                .HasMaxLength(1000);

            entity.Property(t => t.UserId)
                .IsRequired();
        });
    }
}
