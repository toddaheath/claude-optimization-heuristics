using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ProblemDefinition> ProblemDefinitions => Set<ProblemDefinition>();
    public DbSet<AlgorithmConfiguration> AlgorithmConfigurations => Set<AlgorithmConfiguration>();
    public DbSet<OptimizationRun> OptimizationRuns => Set<OptimizationRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalProvider).HasMaxLength(50);
            entity.Property(e => e.ExternalId).HasMaxLength(256);
            entity.HasIndex(e => new { e.ExternalProvider, e.ExternalId })
                .HasFilter("\"ExternalProvider\" IS NOT NULL");
            entity.Property(e => e.IsActive);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.ExpiresAt);
            entity.Property(e => e.IsRevoked);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(256);
            entity.Property(e => e.RevokedReason).HasMaxLength(200);
            entity.Property(e => e.CreatedAt);
        });

        modelBuilder.Entity<ProblemDefinition>(entity =>
        {
            entity.ToTable("problem_definitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Cities).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<City>>(v, JsonOptions) ?? new List<City>(),
                    new ValueComparer<List<City>>(
                        (c1, c2) => JsonSerializer.Serialize(c1, JsonOptions) == JsonSerializer.Serialize(c2, JsonOptions),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => JsonSerializer.Deserialize<List<City>>(JsonSerializer.Serialize(c, JsonOptions), JsonOptions)!));
            entity.Property(e => e.CityCount);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<AlgorithmConfiguration>(entity =>
        {
            entity.ToTable("algorithm_configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.AlgorithmType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Parameters).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, JsonOptions) ?? new Dictionary<string, object>(),
                    new ValueComparer<Dictionary<string, object>>(
                        (c1, c2) => JsonSerializer.Serialize(c1, JsonOptions) == JsonSerializer.Serialize(c2, JsonOptions),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(c, JsonOptions), JsonOptions)!));
            entity.Property(e => e.MaxIterations);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<OptimizationRun>(entity =>
        {
            entity.ToTable("optimization_runs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.BestDistance);
            entity.Property(e => e.BestRoute).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<int>>(v, JsonOptions),
                    new ValueComparer<List<int>?>(
                        (c1, c2) => JsonSerializer.Serialize(c1, JsonOptions) == JsonSerializer.Serialize(c2, JsonOptions),
                        c => c != null ? c.Aggregate(0, HashCode.Combine) : 0,
                        c => c != null ? JsonSerializer.Deserialize<List<int>>(JsonSerializer.Serialize(c, JsonOptions), JsonOptions) : null));
            entity.Property(e => e.IterationHistory).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<IterationResult>>(v, JsonOptions),
                    new ValueComparer<List<IterationResult>?>(
                        (c1, c2) => JsonSerializer.Serialize(c1, JsonOptions) == JsonSerializer.Serialize(c2, JsonOptions),
                        c => c != null ? c.Count : 0,
                        c => c != null ? JsonSerializer.Deserialize<List<IterationResult>>(JsonSerializer.Serialize(c, JsonOptions), JsonOptions) : null));
            entity.Property(e => e.TotalIterations);
            entity.Property(e => e.ExecutionTimeMs);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);

            entity.HasOne(e => e.AlgorithmConfiguration)
                .WithMany()
                .HasForeignKey(e => e.AlgorithmConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProblemDefinition)
                .WithMany()
                .HasForeignKey(e => e.ProblemDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            entity.HasIndex(e => e.UserId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is ProblemDefinition pd)
            {
                pd.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) pd.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is AlgorithmConfiguration ac)
            {
                ac.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) ac.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is OptimizationRun or_)
            {
                or_.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) or_.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is User u)
            {
                u.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added) u.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is RefreshToken rt && entry.State == EntityState.Added)
            {
                rt.CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
