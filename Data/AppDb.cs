using Microsoft.EntityFrameworkCore;
using WeightliftingApi.Models;

namespace WeightliftingApi.Data;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<Movement> Movements => Set<Movement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movement>().HasIndex(m => m.Name).IsUnique();
    }
}
