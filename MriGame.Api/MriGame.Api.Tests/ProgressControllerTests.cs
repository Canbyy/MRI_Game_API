using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MriGame.Api.Controllers;
using MriGame.Api.Data;
using MriGame.Api.DTOs;
using MriGame.Api.Models;
using System.Security.Claims;

namespace MriGame.Api.Tests;

public class ProgressControllerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private ProgressController CreateController(AppDbContext context, Guid userId)
    {
        var controller = new ProgressController(context);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }, "test"))
            }
        };
        return controller;
    }

    private async Task<(ChildProfile kind, GameProgress progress)> MaakKindMetProgressAan(
        AppDbContext context, Guid userId)
    {
        var kind = new ChildProfile
        {
            Id = Guid.NewGuid(),
            ParentUserId = userId,
            FirstName = "Emma",
            LastName = "Jansen",
            Age = 8,
            Gender = Gender.Meisje
        };
        context.ChildProfiles.Add(kind);

        var progress = new GameProgress
        {
            Id = Guid.NewGuid(),
            ChildProfileId = kind.Id,
            CurrentLevel = 1,
            TutorialCompleted = false,
            MriPreparationCompleted = false,
            ScanCompleted = false,
            Stars = 0,
            UpdatedAt = DateTime.UtcNow
        };
        context.GameProgresses.Add(progress);
        await context.SaveChangesAsync();

        return (kind, progress);
    }

    // Test 1: Progressie ophalen van eigen kind geeft Ok terug
    [Fact]
    public async Task GetProgress_ReturnsOk_AlsKindVanEigenOuderIs()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var (kind, _) = await MaakKindMetProgressAan(context, userId);

        var controller = CreateController(context, userId);

        var result = await controller.GetProgress(kind.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    // Test 2: Progressie ophalen van kind van andere ouder geeft NotFound
    [Fact]
    public async Task GetProgress_GeeftNotFound_AlsKindVanAndereOuderIs()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var andereUserId = Guid.NewGuid();
        var (kind, _) = await MaakKindMetProgressAan(context, andereUserId);

        var controller = CreateController(context, userId);

        var result = await controller.GetProgress(kind.Id);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // Test 3: Progressie updaten slaat nieuwe waarden op
    [Fact]
    public async Task UpdateProgress_SlaatNieuweWaardenOp()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var (kind, _) = await MaakKindMetProgressAan(context, userId);

        var controller = CreateController(context, userId);
        var request = new UpdateProgressRequest
        {
            CurrentLevel = 3,
            TutorialCompleted = true,
            MriPreparationCompleted = true,
            ScanCompleted = false,
            Stars = 5
        };

        var result = await controller.UpdateProgress(kind.Id, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var progress = Assert.IsType<GameProgress>(okResult.Value);
        Assert.Equal(3, progress.CurrentLevel);
        Assert.True(progress.TutorialCompleted);
        Assert.Equal(5, progress.Stars);
    }

    // Test 4: Progressie updaten van kind van andere ouder geeft NotFound
    [Fact]
    public async Task UpdateProgress_GeeftNotFound_AlsKindVanAndereOuderIs()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var andereUserId = Guid.NewGuid();
        var (kind, _) = await MaakKindMetProgressAan(context, andereUserId);

        var controller = CreateController(context, userId);
        var request = new UpdateProgressRequest { CurrentLevel = 2 };

        var result = await controller.UpdateProgress(kind.Id, request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // Test 5: Progressie ophalen van niet-bestaand kind geeft NotFound
    [Fact]
    public async Task GetProgress_GeeftNotFound_AlsKindNietBestaat()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var controller = CreateController(context, userId);

        var result = await controller.GetProgress(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }
}