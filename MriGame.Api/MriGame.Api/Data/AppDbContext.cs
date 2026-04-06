using Microsoft.EntityFrameworkCore;
using MriGame.Api.Models;

namespace MriGame.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ParentUser> ParentUsers => Set<ParentUser>();
    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();
    public DbSet<GameProgress> GameProgresses => Set<GameProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParentUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<ChildProfile>()
            .HasOne(c => c.ParentUser)
            .WithMany(p => p.Children)
            .HasForeignKey(c => c.ParentUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameProgress>()
            .HasOne(p => p.ChildProfile)
            .WithOne(c => c.Progress)
            .HasForeignKey<GameProgress>(p => p.ChildProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}