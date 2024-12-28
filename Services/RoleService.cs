using AutoMapper;
using DTOs.RoleDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class RoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        public RoleService(RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleResponse>> GetRoles()
        {
            List<IdentityRole> roles = await _roleManager.Roles.ToListAsync();
            return _mapper.Map<IEnumerable<RoleResponse>>(roles);
        }
    }
}
