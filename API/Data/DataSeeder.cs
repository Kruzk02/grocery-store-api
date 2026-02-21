using Infrastructure.Users;

using Microsoft.AspNetCore.Identity;

namespace API.Data;

public class DataSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<
            UserManager<ApplicationUser>
        >();
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<
            RoleManager<IdentityRole>
        >();

        string[] roleNames = ["Admin", "Manager", "Employee"];

        foreach (var role in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                _ = await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@example.com";
        const string adminUsername = "adminUsername";
        const string adminPassword = "Admin@123";

        ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = adminUsername, Email = adminEmail };

            IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                _ = await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
