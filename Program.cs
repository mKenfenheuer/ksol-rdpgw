using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KSol.RDPGateway.Data;
using RDPGW.Extensions;
using RDPGW.AspNetCore;
using KSol.RDPGateway.RDP;

namespace KSol.RDPGateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddControllersWithViews();

        builder.Services.AddRDPGW();
        builder.Services.AddSingleton<IRDPGWAuthenticationHandler, RDPAuthenticationHandler>();
        builder.Services.AddSingleton<IRDPGWAuthorizationHandler, RDPAutorizationHandler>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            if (userManager?.Users.Count() == 0)
            {
                IdentityUser user = new IdentityUser()
                {
                    UserName = "admin@example.com",
                    NormalizedEmail = "admin@example.com".ToUpper(),
                    NormalizedUserName = "admin@example.com".ToUpper(),
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                };
                userManager.CreateAsync(user).Wait();
                user.PasswordHash = userManager.PasswordHasher.HashPassword(user, "rdpgateway");
                context?.Users.Update(user);
                context?.SaveChanges();
            }
        }




        app.UseRDPGW();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
    }
}
