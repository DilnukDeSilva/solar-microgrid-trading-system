/*
 * File: PasswordService.cs
 * Description: Hashes and verifies passwords with ASP.NET Core Identity PasswordHasher.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 *
 * Uses Microsoft.AspNetCore.Identity.PasswordHasher as described in:
 * https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
 */

using Microsoft.AspNetCore.Identity;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    // Creates a salted hash of the given plaintext password.
    public string Hash(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    // Returns true when the plaintext matches the stored hash.
    public bool Verify(User user, string password)
    {
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
