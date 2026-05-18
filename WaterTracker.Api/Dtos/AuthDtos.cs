using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WaterTracker.Api.Dtos
{
    public record RegisterRequest(
        [Required][EmailAddress] string Email,
        [Required][MinLength(8)] string Password,
        [Required] string Forename,
        [Required] string Surname
    );

    public record RegisterResponse(
        string UserId,
        string Email,
        string Forename,
        string Surname
    );

    public record LoginRequest(
        [Required][EmailAddress] string Email,
        [Required] string Password
    );

    public record TokenRequest(
        [FromForm(Name = "grant_type")] string GrantType,
        [FromForm(Name = "username")] string Username,
        [FromForm(Name = "password")] string Password,
        [FromForm(Name = "client_id")] string? ClientId
    );
}