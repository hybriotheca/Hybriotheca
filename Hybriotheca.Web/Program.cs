using Hybriotheca.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var connectionString = builder.Configuration.GetConnectionString("LocalDb");
            builder.Services.AddDbContext<DataContext>(
                cfg => cfg.UseSqlServer(connectionString));

            builder.Services.AddTransient<SeedDb>();

            builder.Services.AddControllersWithViews();


            var app = builder.Build();

            var scopedFactory = app.Services.GetService<IServiceProvider>();

            using (var scope = scopedFactory?.CreateScope())
            {
                var seeder = scope?.ServiceProvider.GetService<SeedDb>();
                seeder?.Seed();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}