using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MriGame.Api.Controllers;
using MriGame.Api.Data;
using MriGame.Api.DTOs;
using MriGame.Api.Models;
using System.Security.Claims;

namespace MriGame.Api.Tests;

public class ChildrenControllerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private ChildrenController CreateController(AppDbContext context, Guid userId)
    {
        var controller = new ChildrenController(context);
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

    // Test 1: Kind aanmaken met geldige gegevens geeft Ok terug
    [Fact]
    public async Task Create_ReturnsOk_AlsGegevensGeldigZijn()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var controller = CreateController(context, userId);

        var request = new CreateChildRequest
        {
            FirstName = "Emma",
            LastName = "Jansen",
            Age = 8,
            Gender = "Meisje"
        };

        var result = await controller.Create(request);

        Assert.IsType<OkObjectResult>(result);
    }

    // Test 2: Kind aanmaken met ongeldig gender geeft BadRequest
    [Fact]
    public async Task Create_GeeftBadRequest_AlsGenderOngeldigIs()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var controller = CreateController(context, userId);

        var request = new CreateChildRequest
        {
            FirstName = "Emma",
            LastName = "Jansen",
            Age = 8,
            Gender = "Ongeldig"
        };

        var result = await controller.Create(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Test 3: Ouder ziet alleen eigen kinderen
    [Fact]
    public async Task GetAll_ReturnsAlleenEigenKinderen()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var andereUserId = Guid.NewGuid();

        context.ChildProfiles.Add(new ChildProfile
        {
            Id = Guid.NewGuid(),
            ParentUserId = userId,
            FirstName = "Emma",
            LastName = "Jansen",
            Age = 8,
            Gender = Gender.Meisje
        });
        context.ChildProfiles.Add(new ChildProfile
        {
            Id = Guid.NewGuid(),
            ParentUserId = andereUserId,
            FirstName = "Lars",
            LastName = "Pietersen",
            Age = 10,
            Gender = Gender.Jongen
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context, userId);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var kinderen = okResult.Value as IEnumerable<object>;
        Assert.NotNull(kinderen);
        Assert.Single(kinderen);
    }

    // Test 4: Kind ophalen dat niet bestaat geeft NotFound
    [Fact]
    public async Task GetById_GeeftNotFound_AlsKindNietBestaat()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var controller = CreateController(context, userId);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    // Test 5: Kind ophalen van andere ouder geeft NotFound
    [Fact]
    public async Task GetById_GeeftNotFound_AlsKindVanAndereOuderIs()
    {
        var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var andereUserId = Guid.NewGuid();

        var kind = new ChildProfile
        {
            Id = Guid.NewGuid(),
            ParentUserId = andereUserId,
            FirstName = "Lars",
            LastName = "Pietersen",
            Age = 10,
            Gender = Gender.Jongen
        };
        context.ChildProfiles.Add(kind);
        await context.SaveChangesAsync();

        var controller = CreateController(context, userId);

        var result = await controller.GetById(kind.Id);

        Assert.IsType<NotFoundResult>(result);
    }
}