using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UserManagement.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var cs = config.GetConnectionString("Users");
        if (!string.IsNullOrWhiteSpace(cs))
        {
            cs = cs.Replace("%CONTENTROOTPATH%", env.ContentRootPath);
            services.AddDbContext<DataContext>(o => o.UseSqlite(cs));
        }
        else
        {
            services.AddDbContext<DataContext>(o => o.UseInMemoryDatabase("UserManagement.Data.DataContext"));
        }

        services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());
        return services;
    }
}
