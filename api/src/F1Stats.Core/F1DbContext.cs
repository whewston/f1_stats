using F1Stats.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1Stats.Core;

public class F1DbContext(DbContextOptions<F1DbContext> options) : DbContext(options)
{
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Circuit> Circuits => Set<Circuit>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Constructor> Constructors => Set<Constructor>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Season>().HasKey(s => s.Year);
        b.Entity<Circuit>().HasKey(c => c.CircuitId);
        b.Entity<Driver>().HasKey(d => d.DriverId);
        b.Entity<Constructor>().HasKey(c => c.ConstructorId);

        b.Entity<Race>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => new { r.Year, r.Round }).IsUnique();
            e.HasOne(r => r.Season).WithMany(s => s.Races).HasForeignKey(r => r.Year);
            e.HasOne(r => r.Circuit).WithMany(c => c.Races).HasForeignKey(r => r.CircuitId);
        });

        b.Entity<Result>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => new { r.RaceId, r.DriverId }).IsUnique();
            e.HasOne(r => r.Race).WithMany(ra => ra.Results).HasForeignKey(r => r.RaceId);
            e.HasOne(r => r.Driver).WithMany(d => d.Results).HasForeignKey(r => r.DriverId);
            e.HasOne(r => r.Constructor).WithMany(c => c.Results).HasForeignKey(r => r.ConstructorId);
        });
    }
}