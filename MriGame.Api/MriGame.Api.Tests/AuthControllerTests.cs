using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MriGame.Api.Controllers;
using MriGame.Api.Data;
using MriGame.Api.DTOs;
using MriGame.Api.Models;
using MriGame.Api.Services;
using System.Security.Claims;

namespace MriGame.Api.Tests;

public class AuthControllerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private IConfiguration CreateConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSuperSecretKeyVoorJwtTokens123!" },
                { "Jwt:Issuer", "https://test.com" },
                { "Jwt:Audience", "https://test.com" },
                { "Jwt:ExpireMinutes", "60" }
            })
            .Build();
        return config;
    }

    private AuthController CreateController(AppDbContext context)
    {
        var passwordService = new PasswordService();
        var jwtService = new JwtService(CreateConfiguration());
        var controller = new AuthController(context, passwordService, jwtService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    // Test 1: Registreren met geldig verzoek geeft Ok terug
    [Fact]
    public async Task Register_ReturnsOk_AlsGegevensGeldigZijn()
    {
        var context = CreateInMemoryContext();
        var controller = CreateController(context);

        var request = new RegisterParentRequest
        {
            Email = "ouder@test.com",
            Password = "Wachtwoord123!",
            FirstName = "Jan",
            LastName = "Jansen"
        };

        var result = await controller.Register(request);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // Test 2: Registreren met bestaand e-mailadres geeft BadRequest
    [Fact]
    public async Task Register_GeeftBadRequest_AlsEmailAlBestaat()
    {
        var context = CreateInMemoryContext();
        var passwordService = new PasswordService();

        var bestaandeGebruiker = new ParentUser
        {
            Id = Guid.NewGuid(),
            Email = "ouder@test.com",
            FirstName = "Jan",
            LastName = "Jansen"
        };
        bestaandeGebruiker.PasswordHash = passwordService.HashPassword(bestaandeGebruiker, "Wachtwoord123!");
        context.ParentUsers.Add(bestaandeGebruiker);
        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var request = new RegisterParentRequest
        {
            Email = "ouder@test.com",
            Password = "NieuwWachtwoord123!",
            FirstName = "Piet",
            LastName = "Pietersen"
        };

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // Test 3: Inloggen met juiste gegevens geeft Ok terug
    [Fact]
    public async Task Login_ReturnsOk_AlsGegevensKloppen()
    {
        var context = CreateInMemoryContext();
        var passwordService = new PasswordService();

        var gebruiker = new ParentUser
        {
            Id = Guid.NewGuid(),
            Email = "ouder@test.com",
            FirstName = "Jan",
            LastName = "Jansen"
        };
        gebruiker.PasswordHash = passwordService.HashPassword(gebruiker, "Wachtwoord123!");
        context.ParentUsers.Add(gebruiker);
        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var request = new LoginRequest
        {
            Email = "ouder@test.com",
            Password = "Wachtwoord123!"
        };

        var result = await controller.Login(request);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // Test 4: Inloggen met verkeerd wachtwoord geeft Unauthorized
    [Fact]
    public async Task Login_GeeftUnauthorized_AlsWachtwoordFoutIs()
    {
        var context = CreateInMemoryContext();
        var passwordService = new PasswordService();

        var gebruiker = new ParentUser
        {
            Id = Guid.NewGuid(),
            Email = "ouder@test.com",
            FirstName = "Jan",
            LastName = "Jansen"
        };
        gebruiker.PasswordHash = passwordService.HashPassword(gebruiker, "JuistWachtwoord123!");
        context.ParentUsers.Add(gebruiker);
        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var request = new LoginRequest
        {
            Email = "ouder@test.com",
            Password = "FoutWachtwoord!"
        };

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    // Test 5: Inloggen met onbekend e-mailadres geeft Unauthorized
    [Fact]
    public async Task Login_GeeftUnauthorized_AlsEmailNietBestaat()
    {
        var context = CreateInMemoryContext();
        var controller = CreateController(context);

        var request = new LoginRequest
        {
            Email = "onbekend@test.com",
            Password = "Wachtwoord123!"
        };

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}