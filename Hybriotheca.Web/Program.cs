using Hybriotheca.Web.Data;
using Hybriotheca.Web.Data.Authentication;
using Hybriotheca.Web.Helpers;
using Hybriotheca.Web.Helpers.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var connectionString = builder.Configuration.GetConnectionString("LocalDb");
            builder.Services.AddDbContext<DataContext>(
                cfg => cfg.UseSqlServer(connectionString));

            builder.Services.AddIdentity<AppUser, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = true;
                cfg.Password.RequireLowercase = true;
                cfg.Password.RequireNonAlphanumeric = true;
                cfg.Password.RequireUppercase = true;
                cfg.Password.RequiredLength = 8;
                cfg.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddTransient<SeedDb>();

            builder.Services.AddScoped<IMailHelper, MailHelper>();
            builder.Services.AddScoped<IUserHelper, UserHelper>();

            builder.Services.AddControllersWithViews();


            var app = builder.Build();

            await SeedDatabase(app);

            static async Task SeedDatabase(IHost host)
            {
                var scopedFactory = host.Services.GetService<IServiceScopeFactory>();

                using var scope = scopedFactory?.CreateScope();

                var seeder = scope?.ServiceProvider.GetService<SeedDb>();
                await seeder?.SeedAsync();
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}