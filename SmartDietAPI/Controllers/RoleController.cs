using BusinessObjects.Base;
using DTOs.RoleDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet("Get_Roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles()
        {
            IEnumerable<RoleResponse> roles = await _roleService.GetRoles();
            return Ok(ApiResponse<IEnumerable<RoleResponse>>.Success(roles));
        }
    }
}
