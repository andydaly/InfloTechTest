using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserManagement.Data;

public class DataDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "usersinfo.db");
        var cs = $"Data Source={dbPath}";

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(cs)
            .Options;

        return new DataContext(options);
    }
}
