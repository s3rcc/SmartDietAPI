using Microsoft.AspNetCore.Mvc;
using DTOs.UserProfileDTos;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Only allow in Development/Staging environments
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class TestDataController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public TestDataController(
            IUserService userService,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _userService = userService;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost("seed-users")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedUsersAsync([FromQuery] string roleId)
        {
            // Only allow in non-production environments
            if (_environment.IsProduction())
            {
                return BadRequest(new { message = "This endpoint is not available in production environment" });
            }

            var testUsers = new List<RegisterUserWithRoleRequest>
            {
                new() {
                    Name = "John Smith",
                    Email = "john.smith@smartdiet.test",
                    PhoneNumber = "1234567890",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "Emma Johnson",
                    Email = "emma.johnson@smartdiet.test",
                    PhoneNumber = "2345678901",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "Michael Brown",
                    Email = "michael.brown@smartdiet.test",
                    PhoneNumber = "3456789012",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "Sarah Davis",
                    Email = "sarah.davis@smartdiet.test",
                    PhoneNumber = "4567890123",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "James Wilson",
                    Email = "james.wilson@smartdiet.test",
                    PhoneNumber = "5678901234",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "Lisa Anderson",
                    Email = "lisa.anderson@smartdiet.test",
                    PhoneNumber = "6789012345",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "Robert Taylor",
                    Email = "robert.taylor@smartdiet.test",
                    PhoneNumber = "7890123456",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "Jennifer Martinez",
                    Email = "jennifer.martinez@smartdiet.test",
                    PhoneNumber = "8901234567",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "William Thomas",
                    Email = "william.thomas@smartdiet.test",
                    PhoneNumber = "9012345678",
                    Password = "123456",
                    RoleID = roleId
                },
                new() {
                    Name = "Jessica Garcia",
                    Email = "jessica.garcia@smartdiet.test",
                    PhoneNumber = "0123456789",
                    Password = "123456",
                    RoleID = roleId
                }
            };

            var results = new List<string>();
            foreach (var user in testUsers)
            {
                try
                {
                    await _userService.AddUserWithRoleAsync(user);
                    results.Add($"Successfully created user: {user.Email}");
                }
                catch (Exception ex)
                {
                    results.Add($"Failed to create user {user.Email}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Test data seeding completed",
                details = results
            });
        }
    }
}