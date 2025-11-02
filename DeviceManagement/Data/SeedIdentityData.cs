using DeviceManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Data
{
    public static class SeedDataIdentity
    {
        private const string adminUser = "Admin";
        private const string adminPassword = "SysAdmin@123";
        private const string adminEmail = "systemadmin@htsa.co.za";
        private const string adminRole = "Admin";

        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            ApplicationDbContext context = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            UserManager<ApplicationUser> userManager = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            RoleManager<IdentityRole> roleManager = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            if (await userManager.FindByNameAsync(adminUser) == null)
            {
                if (await roleManager.FindByNameAsync(adminRole) == null)
                {
                    await roleManager.CreateAsync(new IdentityRole(adminRole));
                }

                ApplicationUser user = new ApplicationUser
                {
                    UserName = adminUser,
                    Email = adminEmail,
                    FullName = "Siv",
                    Address = "123 Main Street, Qwaqwa",
                    CellphoneNumber = "0821234567",
                    Gender = "Male",
                    CompanyName = "Tech Solutions Pty Ltd",
                    StudentNumber = null, // Not a student
                    BusinessNumber = "1234567890",
                    DateOfBirth = new System.DateTime(2000, 5, 20),
                    SAIdNumber = "8505201234567",  
                    MonthlyEarnings = 50000.00m  
                };


                IdentityResult result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, adminRole);
                }

            }
        }
    }
}
