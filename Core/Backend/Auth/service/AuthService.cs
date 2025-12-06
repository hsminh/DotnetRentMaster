using System.Text;
using RentMaster.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentMaster.Core.Backend.Auth.Interface;
using RentMaster.Core.Backend.Auth.Types.enums;
using RentMaster.Core.Backend.Auth.Types.Response;
using RentMaster.Core.types.enums;

namespace RentMaster.Core.Backend.Auth.service;

public class AuthService : IAuthService
{
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(string gmail, string password, UserTypes type)
        {
            if (string.IsNullOrEmpty(gmail) || string.IsNullOrEmpty(password))
                return null;

            var user = await GetUserByTypeAsync(gmail, type);
            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            var token = GenerateJwtToken(user, type);
            
            // Create a clean user object without sensitive data
            var userResponse = new 
            {
                user.Uid,
                user.FirstName,
                user.LastName,
                user.Gmail,
                user.PhoneNumber,
                user.CreatedAt,
                user.IsVerified,
                user.Avatar
            };

            return new LoginResponse(token, userResponse);
        }

        private string GenerateJwtToken(BaseAuth user, UserTypes type)
        {
            var keyString = Environment.GetEnvironmentVariable("JWT_KEY") 
                            ?? _configuration["Jwt:Key"];

            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                         ?? _configuration["Jwt:Issuer"];

            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                           ?? _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Gmail),
                new Claim("uid", user.Uid.ToString()),
                new Claim("name", $"{user.FirstName} {user.LastName}"),
                new Claim("role", type.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        private async Task<BaseAuth?> GetUserByTypeAsync(string gmail, UserTypes type)
        {
            var activeStatus = UserStatus.Active.ToString();

            return type switch
            {
                UserTypes.Consumer => await _context.Consumers
                    .FirstOrDefaultAsync(u => u.Gmail == gmail && u.Status == activeStatus),

                UserTypes.Admin => await _context.Admins
                    .FirstOrDefaultAsync(u => u.Gmail == gmail && u.Status == activeStatus),

                UserTypes.LandLord => await _context.LandLords
                    .FirstOrDefaultAsync(u => u.Gmail == gmail && u.Status == activeStatus),

                _ => null
            };
        }
}
