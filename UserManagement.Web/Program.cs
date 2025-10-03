using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Services.Implementations;
using UserManagement.Services.Interfaces;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDataAccess(builder.Configuration, builder.Environment)
    .AddDomainServices()
    .AddMarkdown()
    .AddControllersWithViews();

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/bundles/site.css", "wwwroot/css/app.css");
    pipeline.AddJavaScriptBundle("/bundles/site.js", "wwwroot/js/site.js");
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/wasm" });
});

builder.Services.AddScoped<IUserLogService, UserLogService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    if (db.Database.IsSqlite())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
}

app.UseWebOptimizer();
app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int year = 60 * 60 * 24 * 365;
        ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={year}";
    }
});

app.UseHsts();
app.UseHttpsRedirection();
app.UseMarkdown();

app.UseRouting();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
