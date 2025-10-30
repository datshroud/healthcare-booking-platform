using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BookingCareManagement.Infrastructure.Persistence.Seed;

public class DbSeeder
{
    public DbSeeder() { }

    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        if (!await roleMgr.RoleExistsAsync("Admin"))
            await roleMgr.CreateAsync(new AppRole { Name = "Admin" });

        var admin = await userMgr.FindByEmailAsync("admin@local.dev");
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = "admin@local.dev",
                Email = "admin@local.dev",
                EmailConfirmed = true,
                FullName = "System Admin"
            };
            await userMgr.CreateAsync(admin, "Admin123");
            await userMgr.AddToRoleAsync(admin, "Admin");
        }

    }
}
