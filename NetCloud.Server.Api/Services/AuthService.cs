using DotNetCloud.Core.Models;
using DotNetCloud.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DotNetCloud.Server.Api.Services
{
    public class AuthService
    {
        private readonly DotNetCloudDbContext _db;
        private readonly IConfiguration _config;
        public AuthService(DotNetCloudDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<User?> Register(string username, string email, string password)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == username || u.Email == email))
                return null;
            var salt = PasswordHelper.GenerateSalt();
            var hash = PasswordHelper.HashPassword(password, salt);
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = username,
                Email = email,
                PasswordSalt = salt,
                PasswordHash = hash
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<string?> Login(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null) return null;
            if (!PasswordHelper.VerifyPassword(password, user.PasswordSalt, user.PasswordHash)) return null;
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            // Query roles for the user
            var roles = _db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            // Add each role as a claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
