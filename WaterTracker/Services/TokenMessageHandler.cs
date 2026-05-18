using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterTracker.Services
{
    public class TokenMessageHandler(TokenStorageService tokenStorageService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (tokenStorageService.IsAuthenticated && request.RequestUri != null && !request.RequestUri.AbsolutePath.Contains("/connect/token"))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenStorageService.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}