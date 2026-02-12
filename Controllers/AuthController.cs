using LernDotnet.Data;
using LernDotnet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LernDotnet.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IPasswordHasher<AppUser> passwordHasher, IConfiguration config)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    public sealed record AuthResponse(string AccessToken, string TokenType, long ExpiresInSeconds, UsersController.UserResponse User);

    public sealed record RegisterRequest(string Email, string FirstName, string LastName, string Password);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters long.");

        var exists = await _db.Users.AnyAsync(x => x.Email == email, ct);
        if (exists)
            return Conflict("Email already exists.");

        var user = new AppUser
        {
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return Ok(BuildAuthResponse(user));
    }

    public sealed record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user is null)
            return Unauthorized();

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        return Ok(BuildAuthResponse(user));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UsersController.UserResponse>> Me(CancellationToken ct)
    {
        var sub =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync([userId], ct);
        if (user is null)
            return Unauthorized();

        return new UsersController.UserResponse(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAt, user.UpdatedAt);
    }

    private AuthResponse BuildAuthResponse(AppUser user)
    {
        var expiresDays = _config.GetValue<int>("Jwt:ExpiresDays", 1);
        var expires = DateTime.UtcNow.AddDays(expiresDays);

        var token = CreateToken(user, expires);

        var expiresInSeconds = (long)(expires - DateTime.UtcNow).TotalSeconds;

        var userDto = new UsersController.UserResponse(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAt, user.UpdatedAt);
        return new AuthResponse(token, "Bearer", expiresInSeconds, userDto);
    }

    private string CreateToken(AppUser user, DateTime expiresUtc)
    {
        var key = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Jwt:Key is not configured.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            claims: claims,
            expires: expiresUtc,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}