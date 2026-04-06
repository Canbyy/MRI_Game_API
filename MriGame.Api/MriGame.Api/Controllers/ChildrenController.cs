using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MriGame.Api.Data;
using MriGame.Api.DTOs;
using MriGame.Api.Models;
using System.Security.Claims;

namespace MriGame.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChildrenController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChildrenController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateChildRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            return BadRequest("Gender moet Jongen of Meisje zijn.");

        var child = new ChildProfile
        {
            Id = Guid.NewGuid(),
            ParentUserId = Guid.Parse(userIdString),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Age = request.Age,
            Gender = gender
        };

        _context.ChildProfiles.Add(child);
        await _context.SaveChangesAsync();

        var progress = new GameProgress
        {
            Id = Guid.NewGuid(),
            ChildProfileId = child.Id,
            CurrentLevel = 1,
            TutorialCompleted = false,
            MriPreparationCompleted = false,
            ScanCompleted = false,
            Stars = 0,
            UpdatedAt = DateTime.UtcNow
        };

        _context.GameProgresses.Add(progress);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            child.Id,
            child.FirstName,
            child.LastName,
            child.Age,
            Gender = child.Gender.ToString()
        });
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var children = await _context.ChildProfiles
            .Where(x => x.ParentUserId == userId)
            .Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.Age,
                Gender = x.Gender.ToString()
            })
            .ToListAsync();

        return Ok(children);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var child = await _context.ChildProfiles
            .Include(x => x.Progress)
            .FirstOrDefaultAsync(x => x.Id == id && x.ParentUserId == userId);

        if (child == null)
            return NotFound();

        return Ok(new
        {
            child.Id,
            child.FirstName,
            child.LastName,
            child.Age,
            Gender = child.Gender.ToString(),
            Progress = child.Progress == null ? null : new
            {
                child.Progress.CurrentLevel,
                child.Progress.TutorialCompleted,
                child.Progress.MriPreparationCompleted,
                child.Progress.ScanCompleted,
                child.Progress.Stars,
                child.Progress.LastPlayedAt,
                child.Progress.UpdatedAt
            }
        });
    }
}