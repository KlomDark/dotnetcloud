using DotNetCloud.Core.Models;
using DotNetCloud.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NetCloud.Server.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly DotNetCloudDbContext _db;
        public TeamController(DotNetCloudDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _db.Teams.Include(t => t.UserTeams).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(Guid id)
        {
            var team = await _db.Teams.Include(t => t.UserTeams).FirstOrDefaultAsync(t => t.Id == id);
            if (team == null) return NotFound();
            return team;
        }

        [HttpPost]
        public async Task<ActionResult<Team>> CreateTeam([FromBody] Team team)
        {
            team.Id = Guid.NewGuid();
            _db.Teams.Add(team);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] Team team)
        {
            var existing = await _db.Teams.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = team.Name;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(Guid id)
        {
            var team = await _db.Teams.FindAsync(id);
            if (team == null) return NotFound();
            _db.Teams.Remove(team);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{teamId}/users/{userId}")]
        public async Task<IActionResult> AddUserToTeam(Guid teamId, Guid userId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            var user = await _db.Users.FindAsync(userId);
            if (team == null || user == null) return NotFound();
            if (_db.UserTeams.Any(ut => ut.TeamId == teamId && ut.UserId == userId)) return BadRequest("User already in team");
            _db.UserTeams.Add(new UserTeam { TeamId = teamId, UserId = userId });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{teamId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromTeam(Guid teamId, Guid userId)
        {
            var userTeam = await _db.UserTeams.FirstOrDefaultAsync(ut => ut.TeamId == teamId && ut.UserId == userId);
            if (userTeam == null) return NotFound();
            _db.UserTeams.Remove(userTeam);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
