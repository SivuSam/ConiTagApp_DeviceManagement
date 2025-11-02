using DeviceManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Asset> Assets { get; set; }

        // DTO for posting assets
        public class ApiAssetDto
        {
            public string asset_tag { get; set; }
            public string asset_name { get; set; }
            public string room { get; set; }
            public string condition { get; set; }
            public string location { get; set; }
            public string notes { get; set; }
            public string verified { get; set; } = "N";
        }

        // DTO for posting users
        public class ApiUserDto
        {
            public string username { get; set; }
            public string password { get; set; } = "Pass@123"; // Default password
            public string full_name { get; set; }
            public string email { get; set; }
            public int role_id { get; set; } = 4; // Default staff role
            public string status { get; set; } = "A";
            public string staff_number { get; set; }
            public string student_number { get; set; }
            public string sa_id { get; set; }
        }

        public static class DbInitializer
        {
            /// <summary>
            /// Sync all assets to API with console output for each record.
            /// </summary>
            public static async Task SyncDatabaseToApiAsync(IServiceProvider serviceProvider)
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "sync_log.txt");

                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    using var httpClient = new HttpClient();

                    var assets = await context.Assets
                        .AsNoTracking()
                        .Select(a => new ApiAssetDto
                        {
                            asset_tag = a.AssetTag,
                            asset_name = a.AssetName,
                            room = string.IsNullOrEmpty(a.Room) ? "Unknown" : a.Room,
                            condition = string.IsNullOrEmpty(a.Condition) ? "Good" : a.Condition,
                            location = string.IsNullOrEmpty(a.Location) ? "Office" : a.Location,
                            notes = a.Notes ?? "",
                            verified = string.IsNullOrEmpty(a.Verified) ? "N" : a.Verified
                        })
                        .ToListAsync();

                    string apiUrl = "https://oracleapex.com/ords/holdingtechsa/admin/Assert/allassert";
                    Console.WriteLine($"📦 Found {assets.Count} assets to sync...");

                    foreach (var asset in assets)
                    {
                        try
                        {
                            var json = JsonConvert.SerializeObject(asset);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var response = await httpClient.PostAsync(apiUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"✅ Asset '{asset.asset_name}' synced successfully.");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                var error = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"⚠️ Failed to sync asset '{asset.asset_name}': {response.StatusCode} - {error}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"❌ Error syncing asset '{asset.asset_name}': {ex.Message}");
                        }
                    }

                    Console.ResetColor();
                    File.WriteAllText(logPath, $"✅ Asset sync completed successfully at {DateTime.Now}");
                    Console.WriteLine("\n✅ All assets processed.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    var errorMessage = $"❌ Asset sync failed at {DateTime.Now}\nError: {ex.Message}\nInner: {ex.InnerException?.Message}";
                    File.WriteAllText(logPath, errorMessage);
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                }
            }

            /// <summary>
            /// Sync all users to API with console output for each record (skip duplicates).
            /// </summary>
            public static async Task SyncUsersToApiAsync(IServiceProvider serviceProvider)
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "user_sync_log.txt");

                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    using var httpClient = new HttpClient();

                    var users = await context.Users
                        .AsNoTracking()
                        .Select(u => new ApiUserDto
                        {
                            username = u.UserName,
                            password = "Pass@123",
                            full_name = u.FullName,
                            email = u.Email,
                            role_id = 4,
                            status = "A",
                            staff_number = string.IsNullOrEmpty(u.StudentNumber) ? "N/A" : u.StudentNumber,
                            student_number = string.IsNullOrEmpty(u.StudentNumber) ? "N/A" : u.StudentNumber,
                            sa_id = string.IsNullOrEmpty(u.SAIdNumber) ? "0000000000000" : u.SAIdNumber
                        })
                        .ToListAsync();

                    string apiUrl = "https://oracleapex.com/ords/holdingtechsa/campus-users/users/";
                    Console.WriteLine($"👤 Found {users.Count} users to sync...");

                    foreach (var user in users)
                    {
                        try
                        {
                            var json = JsonConvert.SerializeObject(user);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var response = await httpClient.PostAsync(apiUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"✅ User '{user.full_name}' synced successfully.");
                            }
                            else
                            {
                                var error = await response.Content.ReadAsStringAsync();

                                // Skip existing users already in API
                                if (error.Contains("unique constraint"))
                                {
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine($"ℹ️ Skipped existing user '{user.full_name}'.");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"⚠️ Failed to sync user '{user.full_name}': {response.StatusCode} - {error}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"❌ Error syncing user '{user.full_name}': {ex.Message}");
                        }
                    }

                    Console.ResetColor();
                    File.WriteAllText(logPath, $"✅ User sync completed successfully at {DateTime.Now}");
                    Console.WriteLine("\n✅ All users processed.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    var error = $"❌ User sync failed at {DateTime.Now}\nError: {ex.Message}\nInner: {ex.InnerException?.Message}";
                    File.WriteAllText(logPath, error);
                    Console.WriteLine(error);
                    Console.ResetColor();
                }
            }

            /// <summary>
            /// Adds a default Agent user to the database if not already present.
            /// </summary>
            public static async Task AddAgentAsync(IServiceProvider serviceProvider)
            {
                using var scope = serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string roleName = "Agent";
                string username = "agent@holdingtechsa.co.za";
                string email = "agent@holdingtechsa.co.za";
                string fullName = "Default Agent";
                string saIdNumber = "0000000000000";

                try
                {
                    // Ensure Agent role exists
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (roleResult.Succeeded)
                            Console.WriteLine($"✅ Role '{roleName}' created successfully.");
                        else
                            Console.WriteLine($"⚠️ Failed to create role '{roleName}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }

                    // Check if user already exists
                    var existingUser = await userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        Console.WriteLine($"ℹ️ Agent user '{email}' already exists. Skipping creation.");
                        return;
                    }

                    // Create new Agent user
                    var newUser = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        FullName = fullName,
                        SAIdNumber = saIdNumber,
                        EmailConfirmed = true,
                        CompanyName = "Holding Tech SA",
                        CellphoneNumber = "0000000000",
                        BusinessNumber="N/A",
                        Address = "Head Office",
                        Gender = "Other",
                        MonthlyEarnings = 0
                    };

                    var createResult = await userManager.CreateAsync(newUser, "Pass@123");
                    if (createResult.Succeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✅ Agent user '{email}' created successfully.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠️ Failed to create Agent user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    // Assign to Agent role
                    var addRoleResult = await userManager.AddToRoleAsync(newUser, roleName);
                    if (addRoleResult.Succeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✅ User '{email}' assigned to role '{roleName}'.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠️ Failed to assign role '{roleName}' to '{email}': {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Error adding Agent user: {ex.Message}");
                }
                finally
                {
                    Console.ResetColor();
                }
            }

        }
    }
}
