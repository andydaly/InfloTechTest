using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserManagement.Data.Entities;

namespace UserManagement.Data;

public class DataContext : DbContext, IDataContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "usersinfo.db");
            if (File.Exists(dbPath))
                options.UseSqlite($"Data Source={dbPath}");
            else
                options.UseInMemoryDatabase("UserManagement.Data.DataContext");
        }
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        var dtoConverter = new DateTimeOffsetToBinaryConverter();
        model.Entity<UserLog>()
             .Property(p => p.OccurredAt)
             .HasConversion(dtoConverter)
             .HasColumnType("INTEGER");

        model.Entity<User>().HasData(new[]
        {
            new User { Id = 1, Forename = "Peter", Surname = "Loew", Email = "ploew@example.com", IsActive = true, DateOfBirth = new DateTime(1980, 1, 15), Password = "mypassword1" },
            new User { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", Email = "bfgates@example.com", IsActive = true, DateOfBirth = new DateTime(1974, 7, 4), Password = "mypassword1" },
            new User { Id = 3, Forename = "Castor", Surname = "Troy", Email = "ctroy@example.com", IsActive = false, DateOfBirth = new DateTime(1971, 11, 23), Password = "mypassword1" },
            new User { Id = 4, Forename = "Memphis", Surname = "Raines", Email = "mraines@example.com", IsActive = true, DateOfBirth = new DateTime(1976, 5, 3), Password = "mypassword1" },
            new User { Id = 5, Forename = "Stanley", Surname = "Goodspeed", Email = "sgodspeed@example.com", IsActive = true, DateOfBirth = new DateTime(1972, 2, 29), Password = "mypassword1" },
            new User { Id = 6, Forename = "H.I.", Surname = "McDunnough", Email = "himcdunnough@example.com", IsActive = true, DateOfBirth = new DateTime(1960, 9, 9), Password = "mypassword1" },
            new User { Id = 7, Forename = "Cameron", Surname = "Poe", Email = "cpoe@example.com", IsActive = false, DateOfBirth = new DateTime(1970, 12, 12), Password = "mypassword1" },
            new User { Id = 8, Forename = "Edward", Surname = "Malus", Email = "emalus@example.com", IsActive = false, DateOfBirth = new DateTime(1968, 6, 1), Password = "mypassword1" },
            new User { Id = 9, Forename = "Damon", Surname = "Macready", Email = "dmacready@example.com", IsActive = false, DateOfBirth = new DateTime(1965, 3, 30), Password = "mypassword1" },
            new User { Id = 10, Forename = "Johnny", Surname = "Blaze", Email = "jblaze@example.com", IsActive = true, DateOfBirth = new DateTime(1984, 10, 31), Password = "mypassword1" },
            new User { Id = 11, Forename = "Robin", Surname = "Feld", Email = "rfeld@example.com", IsActive = true, DateOfBirth = new DateTime(1955, 8, 20), Password = "mypassword1" },
        });
    }

    public DbSet<User>? Users { get; set; }
    public DbSet<UserLog>? UserLogs { get; set; }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        => base.Set<TEntity>();

    public override ValueTask<TEntity?> FindAsync<TEntity>(params object?[]? keyValues)
        where TEntity : class
        => base.FindAsync<TEntity>(keyValues);

    public async Task CreateAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await AddAsync(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class
    {
        Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class
    {
        Remove(entity);
        await SaveChangesAsync();
    }
}
