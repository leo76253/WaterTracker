using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace WaterTracker.Services
{
    public class AuthStateProvider(TokenStorageService tokenStorageService) : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!tokenStorageService.IsAuthenticated)
            {
                await tokenStorageService.LoadFromSessionStorageAsync();
            }

            if (!tokenStorageService.IsAuthenticated)
            {
                return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, tokenStorageService.CustomerId ?? ""),
                new(ClaimTypes.Name, tokenStorageService.Email ?? ""),
                new(ClaimTypes.Email, tokenStorageService.Email ?? "")
            };

            var identity = new ClaimsIdentity(claims, "password");
            var principal = new ClaimsPrincipal(identity);

            return await Task.FromResult(new AuthenticationState(principal));
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}