/*
 * File: IJwtTokenService.cs
 * Description: Issues signed JWTs with the contract claims: sub, role, nic.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.Services;

public interface IJwtTokenService
{
    // Creates a signed token containing sub, role and nic (prosumers only).
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
