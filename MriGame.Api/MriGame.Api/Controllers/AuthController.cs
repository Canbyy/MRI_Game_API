using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MriGame.Api.Data;
using MriGame.Api.DTOs;
using MriGame.Api.Models;
using MriGame.Api.Services;
using System.Security.Claims;

namespace MriGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, PasswordService passwordService, JwtService jwtService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    [HttpPost("register-parent")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterParentRequest request)
    {
        if (await _context.ParentUsers.AnyAsync(x => x.Email == request.Email))
            return BadRequest("Email bestaat al.");

        var user = new ParentUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        _context.ParentUsers.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            FirstName = user.FirstName,
            Email = user.Email
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _context.ParentUsers.FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
            return Unauthorized("Ongeldige login.");

        var valid = _passwordService.VerifyPassword(user, request.Password);

        if (!valid)
            return Unauthorized("Ongeldige login.");

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            FirstName = user.FirstName,
            Email = user.Email
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult> Me()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var user = await _context.ParentUsers
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            ChildrenCount = user.Children.Count
        });
    }
}