using LernDotnet.Data;
using LernDotnet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
 
namespace LernDotnet.Controllers;
 
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
 
    public UsersController(AppDbContext db, IPasswordHasher<AppUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }
 
    public sealed record CreateUserRequest(string Email, string FirstName, string LastName, string Password);
 
    public sealed record UserResponse(int Id, string Email, string FirstName, string LastName, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
 
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken ct)
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
 
        return CreatedAtRoute("GetUserById", new { id = user.Id }, ToResponse(user));
    }
 
    [HttpGet("{id:int}", Name = "GetUserById")]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user is null)
            return NotFound();
 
        return ToResponse(user);
    }
 
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll(CancellationToken ct)
    {
        var users = await _db.Users
            .OrderBy(x => x.Id)
            .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(ct);
 
        return users;
    }
 
    private static UserResponse ToResponse(AppUser user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAt, user.UpdatedAt);
}