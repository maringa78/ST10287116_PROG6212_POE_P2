using System;
using Microsoft.EntityFrameworkCore;
using ST10287116_PROG6212_POE_P2.Services;
using ST10287116_PROG6212_POE_P2.Models; // For User / UserRole
using ST10287116_PROG6212_POE_P2.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace ST10287116_PROG6212_POE_P2
{
    public class Program
    {
        public static void Main(string[] args)
        {
           
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(30);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = "/Account/Login";
                    o.AccessDeniedPath = "/Account/Login";
                });

            builder.Services.AddAuthorization(opts =>
            {
                // Force auth for all controllers unless [AllowAnonymous]
                opts.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase("AppInMemoryDb"));
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<ClaimService>();

            var app = builder.Build();

            // Middleware (keep order)
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            // Map a logout route that works even when the current URL is inside an Area
            app.MapControllerRoute(
                name: "logout-any-area",
                pattern: "{area:exists}/Account/Logout",
                defaults: new { controller = "Account", action = "Logout" });

            using (var scope = app.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ctx.Database.EnsureCreated();
                if (!ctx.Users.Any())
                {
                    ctx.Users.AddRange(
                        new User { Name = "Alice", Surname = "HR", Email = "hr@test.com", PasswordHash = "pass123", Role = UserRole.HR, HourlyRate = 0m },
                        new User { Name = "John", Surname = "Doe", Email = "lecturer@test.com", PasswordHash = "pass123", Role = UserRole.Lecturer, HourlyRate = 25m },
                        new User { Name = "Jane", Surname = "Smith", Email = "coordinator@test.com", PasswordHash = "pass123", Role = UserRole.Coordinator, HourlyRate = 0m },
                        new User { Name = "Bob", Surname = "Manager", Email = "manager@test.com", PasswordHash = "pass123", Role = UserRole.Manager, HourlyRate = 0m }
                    );
                    ctx.SaveChanges();
                }

                // Seed sample claims so Coordinator/Manager pages show data
                if (!ctx.Claims.Any())
                {
                    var lecturer = ctx.Users.First(u => u.Role == UserRole.Lecturer);
                    ctx.Claims.AddRange(
                        new Claim
                        {
                            Type = ClaimType.Research,
                            Amount = 0m,
                            Description = "Research hours",
                            Status = ClaimStatus.Pending,
                            HoursWorked = 8,
                            HourlyRate = lecturer.HourlyRate,
                            TotalAmount = 8 * lecturer.HourlyRate,
                            ClaimDate = DateTime.Today.AddDays(-2),
                            UserId = lecturer.Id.ToString(),
                            LecturerId = lecturer.Id
                        },
                        new Claim
                        {
                            Type = ClaimType.Development,
                            Amount = 0m,
                            Description = "Course development",
                            Status = ClaimStatus.Verified, // Will show on Manager dashboard
                            HoursWorked = 6,
                            HourlyRate = lecturer.HourlyRate,
                            TotalAmount = 6 * lecturer.HourlyRate,
                            ClaimDate = DateTime.Today.AddDays(-1),
                            UserId = lecturer.Id.ToString(),
                            LecturerId = lecturer.Id
                        }
                    );
                    ctx.SaveChanges();
                }
            }

            app.Run();
        }
    }
}