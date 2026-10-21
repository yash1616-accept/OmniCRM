using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OmniCRM.Models;

namespace OmniCRM.Data;

/// <summary>
/// Entity Framework Core database context for the OmniCRM system.
/// Manages entity sets, schema mappings, and SQL Server persistence.
/// </summary>
public class OmniCrmDbContext : DbContext
{
    public OmniCrmDbContext()
    {
    }

    public OmniCrmDbContext(DbContextOptions<OmniCrmDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers => Set<Customer>();
    public virtual DbSet<Lead> Leads => Set<Lead>();
    public virtual DbSet<FollowUp> FollowUps => Set<FollowUp>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Build configuration from appsettings.json if present
            string connectionString = GetDefaultConnectionString();
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    public static string GetDefaultConnectionString()
    {
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connStr = config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                return connStr;
            }
        }
        catch
        {
            // Fallback if config loading fails
        }

        // Standard developer local connection string for MSSQLSERVER
        return "Server=localhost;Database=OmniCrmDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer Entity Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(120);
            entity.Property(c => c.Phone).HasMaxLength(25);
            entity.Property(c => c.CompanyName).HasMaxLength(100);
            entity.Property(c => c.Address).HasMaxLength(250);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.Name);
        });

        // Lead Entity Configuration
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Title).IsRequired().HasMaxLength(150);
            entity.Property(l => l.EstimatedValue).HasColumnType("decimal(18,2)");
            entity.Property(l => l.Notes).HasMaxLength(500);
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(l => l.Customer)
                  .WithMany(c => c.Leads)
                  .HasForeignKey(l => l.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(l => l.Status);
        });

        // FollowUp Entity Configuration
        modelBuilder.Entity<FollowUp>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Notes).IsRequired().HasMaxLength(1000);
            entity.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(f => f.Customer)
                  .WithMany(c => c.FollowUps)
                  .HasForeignKey(f => f.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => f.ScheduledDate);
            entity.HasIndex(f => f.IsCompleted);
        });
    }
}
