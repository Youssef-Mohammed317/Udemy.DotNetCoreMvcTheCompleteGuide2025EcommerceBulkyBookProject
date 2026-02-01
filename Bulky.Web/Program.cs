using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.DbInitializer;
using BulkyBook.DataAccesss.Repository;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BulkyBook.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(nameof(StripeSettings)));
            builder.Services.Configure<BrevoSettings>(
                builder.Configuration.GetSection("BrevoSettings"));

            builder.Services.AddScoped<IEmailSender, BrevoEmailSender>();
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<HttpClient>();
            builder.Services.AddRazorPages();

            builder.Services.AddScoped<IDbInitializer, DbInitializer>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(100);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "3202745713238434";
                options.AppSecret = "337b93d7db874a3accc104ba46ce1826";
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                dbInitializer.Initialize();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages();
            app.Run();
        }
    }
}
