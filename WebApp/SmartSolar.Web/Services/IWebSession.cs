/*
 * File: IWebSession.cs
 * Description: Cookie + session helpers that store the JWT after login. No API rules live here.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Web.Models;

namespace SmartSolar.Web.Services;

public interface IWebSession
{
    // Writes the JWT to session and signs the auth cookie from the login payload.
    Task SignInAsync(LoginResponseDto login);

    // Clears the JWT session key and the auth cookie.
    Task SignOutAsync();

    // Returns the bearer token used by ApiClient, or null when the user is not signed in.
    string? GetJwt();
}
