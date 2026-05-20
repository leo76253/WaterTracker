using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using WaterTracker.Api.Dtos;
using WaterTracker.Api.Models;
using WaterTracker.Api.Services.Interfaces;

namespace WaterTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ICustomerService customerService, UserManager<Customer> userManager) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await customerService.RegisterAsync(request);
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(OpenIddictConstants.Claims.Subject)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await userManager.FindByIdAsync(userId);

            return Ok(new
            {
                email = user.Email,
                forename = user.Forename,
                surname = user.Surname,
                userId = user.Id
            });
        }
    }
}
