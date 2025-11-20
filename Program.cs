using System;
using Microsoft.EntityFrameworkCore;
using ST10287116_PROG6212_POE_P2.Services;
using ST10287116_PROG6212_POE_P2.Models; // Add this for User / UserRole

namespace ST10287116_PROG6212_POE_P2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<ClaimService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("AppInMemoryDb"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "root-to-lecturer",
                pattern: "",
                defaults: new { area = "Lecturer", controller = "Dashboard", action = "Index" });

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            // Seed data (place BEFORE app.Run)
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated(); // InMemory: harmless

                if (!context.User.Any())
                {
                    // If your User model DOES NOT have Name/Surname, replace with Username property or add those properties.
                    context.User.AddRange(
                        new User { Username = "Admin HR", Email = "hr@test.com", PasswordHash = "pass123", Role = UserRole.Manager, HourlyRate = 0m },
                        new User { Username = "John Doe", Email = "lecturer@test.com", PasswordHash = "pass123", Role = UserRole.Lecturer, HourlyRate = 25m },
                        new User { Username = "Jane Smith", Email = "coordinator@test.com", PasswordHash = "pass123", Role = UserRole.Coordinator, HourlyRate = 0m },
                        new User { Username = "Bob Manager", Email = "manager@test.com", PasswordHash = "pass123", Role = UserRole.Manager, HourlyRate = 0m }
                    );
                    context.SaveChanges();
                }
            }

            app.Run();
        }
    }
}
