    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore.Metadata;
    using SchoolSystem.Data;
    using SchoolSystem.Models;

    namespace SchoolSystem.Services
    {
        public class SeedService
        {
            public static async Task SeedDatabase(IServiceProvider serviceProvider)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

                try
                {
                    logger.LogInformation("Ensuring the database is created.");
                    await context.Database.EnsureCreatedAsync();

                    logger.LogInformation("Ensuring the roles are created.");
                    await AddRoleAsync(roleManager, "Admin");
                    await AddRoleAsync(roleManager, "Student");
                    await AddRoleAsync(roleManager, "Teacher");
                    await AddRoleAsync(roleManager, "Academic");
                    await AddRoleAsync(roleManager, "StudentCouncil");
                    await AddRoleAsync(roleManager, "Director");


                    logger.LogInformation("Seeding users for roles");

                    // Create users for each role
                    await CreateUserWithRoleAsync(userManager, logger, "admin@admin.com", "Admin123456", "Admin", "No.1", "Admin@123", "Admin");
                    await CreateUserWithRoleAsync(userManager, logger, "student@test.com", "StudentUser", "Student", "Test", "Student@123", "Student");
                    await CreateUserWithRoleAsync(userManager, logger, "teacher@test.com", "TeacherUser", "Teacher", "Test", "Teacher@123", "Teacher");
                    await CreateUserWithRoleAsync(userManager, logger, "academic@test.com", "AcademicUser", "Academic", "Test", "Academic@123", "Academic");
                    await CreateUserWithRoleAsync(userManager, logger, "studentcouncil@test.com", "StudentCouncilUser", "StudentCouncil", "Test", "StudentCouncil@123", "StudentCouncil");
                    await CreateUserWithRoleAsync(userManager, logger, "director@test.com", "DirectorUser", "Director", "Test", "Director@123", "Director");
            
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "An error occurred while creating the database.");
                }
            }
            private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role '{roleName}' : {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

        private static async Task CreateUserWithRoleAsync(
            UserManager<Users> userManager,
            ILogger logger,
            string email,
            string username,
            string firstName,
            string lastName,
            string password,
            string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new Users
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = username,
                    NormalizedUserName = username.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    logger.LogInformation($"{role} user '{username}' created successfully.");
                }
                else
                {
                    logger.LogError($"Failed to create {role} user '{username}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"{role} user '{username}' already exists.");
            }
        }
    }
    }
