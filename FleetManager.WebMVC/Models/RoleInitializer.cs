using FleetManager.Domain.Models;
using FleetManager.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace FleetManager.WebMVC
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, FleetContext dbContext)
        {
            Console.WriteLine("--- Початок перевірки ролей та адміна ---");

            if (!dbContext.Roles.Any())
            {
                Console.WriteLine("[БД] Таблиця Roles порожня. Додаємо ролі...");
                dbContext.Roles.AddRange(new Role { Name = "superadmin" }, new Role { Name = "admin" }, new Role { Name = "user" });
                await dbContext.SaveChangesAsync();
            }

            string[] roles = { "superadmin", "admin", "user" };
            foreach (var role in roles)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                {
                    Console.WriteLine($"[Identity] Створюємо роль: {role}");
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var requiredStatuses = new[] { "in progress", "died", "waiting" };
            var existingStatuses = dbContext.RobotStatuses.Select(s => s.Name).ToList();
            var missingStatuses = requiredStatuses.Except(existingStatuses, StringComparer.OrdinalIgnoreCase).ToList();
            if (missingStatuses.Any())
            {
                foreach (var s in missingStatuses)
                {
                    dbContext.RobotStatuses.Add(new RobotStatus { Name = s });
                }
                await dbContext.SaveChangesAsync();
            }

            var requiredSeverities = new[] { "fatal", "info", "warning" };
            var existingSeverities = dbContext.LogSeverities.Select(s => s.Name).ToList();
            var missingSeverities = requiredSeverities.Except(existingSeverities, StringComparer.OrdinalIgnoreCase).ToList();
            if (missingSeverities.Any())
            {
                foreach (var s in missingSeverities)
                {
                    dbContext.LogSeverities.Add(new LogSeverity { Name = s });
                }
                await dbContext.SaveChangesAsync();
            }

            string adminUserName = "admin";
            string password = "admin";

            var existingSuperadmins = await userManager.GetUsersInRoleAsync("superadmin");

            var userWithAdminName = await userManager.FindByNameAsync(adminUserName);

            if (userWithAdminName != null)
            {
                if (!await userManager.IsInRoleAsync(userWithAdminName, "superadmin"))
                {
                    await userManager.AddToRoleAsync(userWithAdminName, "superadmin");
                }
                try
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(userWithAdminName);
                    await userManager.ResetPasswordAsync(userWithAdminName, token, password);
                }
                catch { }
                foreach (var other in existingSuperadmins.Where(u => u.Id != userWithAdminName.Id))
                {
                    await userManager.RemoveFromRoleAsync(other, "superadmin");
                    if (await roleManager.FindByNameAsync("admin") != null)
                        await userManager.AddToRoleAsync(other, "admin");
                }
            }
            else if (existingSuperadmins.Any())
            {
                var primary = existingSuperadmins.First();
                primary.UserName = adminUserName;
                primary.Email = adminUserName;
                await userManager.UpdateAsync(primary);
                try
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(primary);
                    await userManager.ResetPasswordAsync(primary, token, password);
                }
                catch { }
                foreach (var other in existingSuperadmins.Skip(1))
                {
                    await userManager.RemoveFromRoleAsync(other, "superadmin");
                    if (await roleManager.FindByNameAsync("admin") != null)
                        await userManager.AddToRoleAsync(other, "admin");
                }
            }
            else
            {
                var superAdmin = new User
                {
                    Email = adminUserName,
                    UserName = adminUserName,
                    FullName = "Головний Адміністратор",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(superAdmin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "superadmin");
                }
            }
        }
    }
}