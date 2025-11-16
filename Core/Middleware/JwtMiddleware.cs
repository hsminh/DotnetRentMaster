using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentMaster.Core.Backend.Auth.Types.enums;
using RentMaster.Core.types.enums;
using RentMaster.Data;

namespace RentMaster.Core.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var endpoint = context.GetEndpoint();
        var hasAdminScope = endpoint?.Metadata.GetMetadata<Attributes.AdminScopeAttribute>() != null;
        var hasUserScope = endpoint?.Metadata.GetMetadata<Attributes.UserScopeAttribute>() != null;
        var hasLandLordScope = endpoint?.Metadata.GetMetadata<Attributes.LandLordScopeAttribute>() != null;

        if (!hasAdminScope && !hasUserScope && !hasLandLordScope)
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing or invalid token");
            return;
        }

        try
        {
            var jwt = ValidateJwtToken(token);
            if (jwt == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token");
                return;
            }

            var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;

            if (!Guid.TryParse(userId, out var uid))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid user id");
                return;
            }

            if ((hasAdminScope && role != nameof(UserTypes.Admin)) ||
                (hasUserScope && role != nameof(UserTypes.Consumer)) ||
                (hasLandLordScope && role != nameof(UserTypes.LandLord)))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: role not allowed");
                return;
            }

            if (!Enum.TryParse<UserTypes>(role, out var roleEnum))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid role");
                return;
            }

            var user = await GetUserByRoleAsync(db, uid, roleEnum);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("User not found");
                return;
            }

            context.Items["user"] = user;
            context.Items["role"] = role;
            context.Items["uid"] = uid;

            await _next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JWT Error: {ex.Message}");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token validation failed");
        }
    }

    private JwtSecurityToken? ValidateJwtToken(string token)
    {
        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(key);

        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        }, out SecurityToken validatedToken);

        return (JwtSecurityToken)validatedToken;
    }

private async Task<object?> GetUserByRoleAsync(AppDbContext db, Guid uid, UserTypes role)
{
    switch (role)
    {
        case UserTypes.Admin:
            return await db.Admins
                .FirstOrDefaultAsync(u => u.Uid == uid && u.Status == UserStatus.Active.ToString());

        case UserTypes.LandLord:
            return await db.LandLords
                .FirstOrDefaultAsync(u => u.Uid == uid && u.Status == UserStatus.Active.ToString());

        case UserTypes.Consumer:
            return await db.Consumers
                .FirstOrDefaultAsync(u => u.Uid == uid && u.Status == UserStatus.Active.ToString());

        default:
            return null;
    }
}
}
