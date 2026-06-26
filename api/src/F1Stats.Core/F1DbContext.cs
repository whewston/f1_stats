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
    public DbSet<DriverStanding> DriverStandings => Set<DriverStanding>();
    public DbSet<ConstructorStanding> ConstructorStandings => Set<ConstructorStanding>();
    public DbSet<Prediction> Predictions => Set<Prediction>();

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
        
        b.Entity<DriverStanding>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.Year, s.DriverId }).IsUnique();
            e.HasOne(s => s.Season).WithMany().HasForeignKey(s => s.Year);
            e.HasOne(s => s.Driver).WithMany().HasForeignKey(s => s.DriverId);
            e.HasOne(s => s.Constructor).WithMany().HasForeignKey(s => s.ConstructorId);
        });
        
        b.Entity<ConstructorStanding>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.Year, s.ConstructorId }).IsUnique();
            e.HasOne(s => s.Season).WithMany().HasForeignKey(s => s.Year);
            e.HasOne(s => s.Constructor).WithMany().HasForeignKey(s => s.ConstructorId);
        });
        
        b.Entity<Prediction>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => new { p.Year, p.Round, p.DriverId }).IsUnique();
            e.HasOne(p => p.Driver).WithMany().HasForeignKey(p => p.DriverId);
        });
    }
}