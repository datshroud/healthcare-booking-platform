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

        // tao du 3 role neu co
        string[] roles = {"Admin", "Doctor", "Customer"};

        foreach (var role in roles)
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                var createRole = await roleMgr.CreateAsync(new AppRole { Name = role });
                if (!createRole.Succeeded)
                    throw new Exception($"Seed role '{role}' failed: {string.Join("; ", createRole.Errors.Select(e => e.Description))}");
            }
        }

        // tao admin mac dinh (chi can role admin)

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
            var created = await userMgr.CreateAsync(admin, "Admin123");
            if (!created.Succeeded)
                throw new Exception($"Seed admin failed: {string.Join("; ", created.Errors.Select(e => e.Description))}");

            var addRole = await userMgr.AddToRoleAsync(admin, "Admin");
            if (!addRole.Succeeded)
                throw new Exception($"Assign role to admin failed: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
        }

        // seed user doctor
        var doctor = await userMgr.FindByEmailAsync("doctor@local.dev");
        if (doctor == null)
        {
            doctor = new AppUser
            {
                UserName = "doctor@local.dev",
                Email = "doctor@local.dev",
                EmailConfirmed = true,
                FullName = "Default Doctor"
            };

            var created = await userMgr.CreateAsync(doctor, "Doctor123");
            if (!created.Succeeded)
                throw new Exception($"Seed doctor failed: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            var addRole = await userMgr.AddToRoleAsync(doctor, "Doctor");
            if (!addRole.Succeeded)
                throw new Exception($"Assign role to doctor failed: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
        }


        // seec user customer
        var customer = await userMgr.FindByEmailAsync("customer@local.dev");
        if (customer == null)
        {
            customer = new AppUser
            {
                UserName = "customer@local.dev",
                Email = "customer@local.dev",
                EmailConfirmed = true,
                FullName = "Default Customer"
            };

            var created = await userMgr.CreateAsync(customer, "Customer123");
            if (!created.Succeeded)
                throw new Exception($"Seed customer failed: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            var addRole = await userMgr.AddToRoleAsync(customer, "Customer");
            if (!addRole.Succeeded)
                throw new Exception($"Assign role to customer failed: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
        }
    }
}
