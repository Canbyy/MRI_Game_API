using Microsoft.AspNetCore.Identity;
using MriGame.Api.Models;

namespace MriGame.Api.Services;

public class PasswordService
{
    private readonly PasswordHasher<ParentUser> _passwordHasher = new();

    public string HashPassword(ParentUser user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(ParentUser user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success;
    }
}