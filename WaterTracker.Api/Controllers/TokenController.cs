using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using WaterTracker.Api.Models;

namespace WaterTracker.Api.Controllers
{
    [ApiController]
    // [Route("api/[controller]")]
    [AllowAnonymous]
    public class TokenController(
        SignInManager<Customer> signInManager,
        UserManager<Customer> userManager) : ControllerBase
    {
        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange(
            [FromForm] string grant_type,
            [FromForm] string username,
            [FromForm] string password,
            [FromForm] string? client_id)
        {
            if (grant_type != "password")
                return BadRequest(new { error = "unsupported_grant_type" });

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return BadRequest(new { error = "invalid_request" });

            var user = await userManager.FindByNameAsync(username);
            if (user is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var result = await signInManager.CheckPasswordSignInAsync(user, password, true);

            if (result.IsLockedOut)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var principal = await signInManager.CreateUserPrincipalAsync(user);
            principal.SetClaim(OpenIddictConstants.Claims.Subject, user.Id);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
