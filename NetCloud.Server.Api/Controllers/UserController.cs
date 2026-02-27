using DotNetCloud.Core.Models;
using DotNetCloud.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetCloud.Server.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DotNetCloudDbContext _db;
        public UserController(DotNetCloudDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _db.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var user = await _db.Users.FindAsync(dto.UserId);
            var role = await _db.Roles.FindAsync(dto.RoleId);
            if (user == null || role == null)
                return NotFound();
            if (await _db.UserRoles.AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId))
                return BadRequest("Role already assigned.");
            _db.UserRoles.Add(new UserRole { UserId = dto.UserId, RoleId = dto.RoleId });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            var userRole = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);
            if (userRole == null)
                return NotFound();
            _db.UserRoles.Remove(userRole);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }

    public class AssignRoleDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}
