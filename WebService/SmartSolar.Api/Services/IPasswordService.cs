/*
 * File: IPasswordService.cs
 * Description: Password hashing/verification contract. Never store or log plaintext passwords.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.Services;

public interface IPasswordService
{
    // Creates a salted hash of the given plaintext password.
    string Hash(User user, string password);

    // Returns true when the plaintext matches the stored hash.
    bool Verify(User user, string password);
}
