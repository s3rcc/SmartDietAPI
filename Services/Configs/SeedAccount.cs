using BusinessObjects.Entity;
using DataAccessObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Services.Configs
{
    public class SeedAccount
    {
        private readonly ILogger<SeedAccount> _logger;
        private readonly SmartDietDbContext _context;
        private readonly UserManager<SmartDietUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public SeedAccount(
            ILogger<SeedAccount> logger,
            SmartDietDbContext context, 
            UserManager<SmartDietUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task InitialiseAsync()
        {
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    if (!_context.Database.CanConnect())
                    {
                        await _context.Database.EnsureDeletedAsync();
                        await _context.Database.MigrateAsync();
                    }
                    else
                    {
                        await _context.Database.MigrateAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }
        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
        private async Task TrySeedAsync()
        {
            _logger.LogInformation("Starting to seed data...");
            await SeedRolesAsync();
            await SeedUsersAsync();
            _logger.LogInformation("Data seeding completed.");
        }
        #region Roles
        private async Task SeedRolesAsync()
        {
            var roles = new[]
            {
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "Staff" },
                new IdentityRole { Name = "Member" }
            };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role.Name))
                {
                    await _roleManager.CreateAsync(role);
                    _logger.LogInformation($"Role {role.Name} created successfully.");
                }
            }
        }
        #endregion Roles

        #region User
        private async Task SeedUsersAsync()
        {
            var users = new List<(SmartDietUser User, string Role)>
    {
        (CreateUser("admin", "admin@example.com", "Admin"), "Admin"),
        (CreateUser("staff", "staff@example.com", "Staff"), "Staff"),
        (CreateUser("staff2", "staff2@example.com", "Staff"), "Staff"),
        (CreateUser("member", "member@example.com", "Member"), "Member"),
        (CreateUser("member2", "member2@example.com", "Member"), "Member")
    };

            foreach (var (user, role) in users)
            {
                await CreateUserIfNotExist(user, role);
            }
        }

        #endregion User

        #region addition function
        private SmartDietUser CreateUser(string userName, string email, string role)
        {
            return new SmartDietUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
            };
        }
        private UserProfile CreateUserProfile(SmartDietUser user)
        {
            return new UserProfile
            {
                SmartDietUserId = user.Id,
                FullName = user.UserName,
                ProfilePicture = "",
                TimeZone = "UTC",
                PreferredLanguage = "en",
                EnableEmailNotifications = true,
                EnableNotifications = true,
                EnablePushNotifications = true,
                CreatedBy = "root",
            };
        }
        private async Task CreateUserIfNotExist(SmartDietUser user, string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser is null)
            {
                var result = await _userManager.CreateAsync(user, "123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    _logger.LogInformation($"User {user.Email} created and assigned to role {role}.");

                    var userProfile = CreateUserProfile(user);
                    await AddUserProfile(userProfile);
                }
                else
                {
                    _logger.LogError($"Failed to create user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private async Task AddUserProfile(UserProfile userProfile)
        {
            if (!_context.UserProfiles.Any(p => p.CreatedBy == userProfile.CreatedBy))
            {
                await _context.UserProfiles.AddAsync(userProfile);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User profile seeded successfully.");
            }
        }
        #endregion
    }
}

