using System.Linq;
using BookingCareManagement.Domain.Aggregates.ClinicRoom;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        string[] roles = { "Admin", "Doctor", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                var createRole = await roleMgr.CreateAsync(new AppRole { Name = role });
                if (!createRole.Succeeded)
                {
                    throw new Exception($"Seed role '{role}' failed: {string.Join("; ", createRole.Errors.Select(e => e.Description))}");
                }
            }
        }

        var admin = await userMgr.FindByEmailAsync("admin@local.dev");
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = "admin@local.dev",
                Email = "admin@local.dev",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin"
            };

            var created = await userMgr.CreateAsync(admin, "Admin123");
            if (!created.Succeeded)
            {
                throw new Exception($"Seed admin failed: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }

            var addRole = await userMgr.AddToRoleAsync(admin, "Admin");
            if (!addRole.Succeeded)
            {
                throw new Exception($"Assign role to admin failed: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
            }
        }

        var doctorUser = await userMgr.FindByEmailAsync("doctor@local.dev");
        if (doctorUser is null)
        {
            doctorUser = new AppUser
            {
                UserName = "doctor@local.dev",
                Email = "doctor@local.dev",
                EmailConfirmed = true,
                FirstName = "Default",
                LastName = "Doctor"
            };

            var created = await userMgr.CreateAsync(doctorUser, "Doctor123");
            if (!created.Succeeded)
            {
                throw new Exception($"Seed doctor failed: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }

            var addRole = await userMgr.AddToRoleAsync(doctorUser, "Doctor");
            if (!addRole.Succeeded)
            {
                throw new Exception($"Assign role to doctor failed: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
            }
        }

        var doctorUsers = await userMgr.GetUsersInRoleAsync("Doctor");
        foreach (var doc in doctorUsers)
        {
            var exists = await dbContext.Doctors
                .AsNoTracking()
                .AnyAsync(d => d.AppUserId == doc.Id);

            if (!exists)
            {
                dbContext.Doctors.Add(new Doctor(doc.Id));
            }
        }

        await dbContext.SaveChangesAsync();

        if (!await dbContext.ClinicRooms.AsNoTracking().AnyAsync())
        {
            dbContext.ClinicRooms.Add(new ClinicRoom("CR-001", 1));
            await dbContext.SaveChangesAsync();
        }

        var customer = await userMgr.FindByEmailAsync("customer@local.dev");
        if (customer is null)
        {
            customer = new AppUser
            {
                UserName = "customer@local.dev",
                Email = "customer@local.dev",
                EmailConfirmed = true,
                FirstName = "Default",
                LastName = "Customer"
            };

            var created = await userMgr.CreateAsync(customer, "Customer123");
            if (!created.Succeeded)
            {
                throw new Exception($"Seed customer failed: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }

            var addRole = await userMgr.AddToRoleAsync(customer, "Customer");
            if (!addRole.Succeeded)
            {
                throw new Exception($"Assign role to customer failed: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
            }
        }
    }
}
