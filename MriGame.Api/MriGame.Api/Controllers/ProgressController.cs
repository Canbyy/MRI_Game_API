using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MriGame.Api.Data;
using MriGame.Api.DTOs;
using System.Security.Claims;

namespace MriGame.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProgressController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{childId}")]
    public async Task<ActionResult> GetProgress(Guid childId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var childExists = await _context.ChildProfiles
            .AnyAsync(x => x.Id == childId && x.ParentUserId == userId);

        if (!childExists)
            return NotFound("Kind niet gevonden.");

        var progress = await _context.GameProgresses
            .FirstOrDefaultAsync(x => x.ChildProfileId == childId);

        if (progress == null)
            return NotFound("Progressie niet gevonden.");

        return Ok(progress);
    }

    [HttpPut("{childId}")]
    public async Task<ActionResult> UpdateProgress(Guid childId, UpdateProgressRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var childExists = await _context.ChildProfiles
            .AnyAsync(x => x.Id == childId && x.ParentUserId == userId);

        if (!childExists)
            return NotFound("Kind niet gevonden.");

        var progress = await _context.GameProgresses
            .FirstOrDefaultAsync(x => x.ChildProfileId == childId);

        if (progress == null)
            return NotFound("Progressie niet gevonden.");

        progress.CurrentLevel = request.CurrentLevel;
        progress.TutorialCompleted = request.TutorialCompleted;
        progress.MriPreparationCompleted = request.MriPreparationCompleted;
        progress.ScanCompleted = request.ScanCompleted;
        progress.Stars = request.Stars;
        progress.LastPlayedAt = DateTime.UtcNow;
        progress.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(progress);
    }
}