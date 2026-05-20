using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WaterTracker.Services
{
    public class AuthService(HttpClient http, TokenStorageService tokenStorage, AuthStateProvider authStateProvider)
    {
        private record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken, [property: JsonPropertyName("token_type")] string TokenType, [property: JsonPropertyName("expires_in")] int ExpiresIn);
        private record UserResponse([property: JsonPropertyName("email")] string Email, [property: JsonPropertyName("forename")] string Forename, [property: JsonPropertyName("surname")] string Surname, [property: JsonPropertyName("userId")] string UserId);

        public async Task<bool> LoginAsync(string email, string password)
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", email),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("client_id", "watertracker-blazor")
            });

            var response = await http.PostAsync("/connect/token", form);
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Login failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (result is null || string.IsNullOrEmpty(result.AccessToken))
            {
                Console.Error.WriteLine("Login failed: Invalid token response");
                return false;
            }

            tokenStorage.AccessToken = result.AccessToken;

            var userResponse = await http.GetFromJsonAsync<UserResponse>("/api/auth/me");
            if (userResponse is null)
            {
                Console.Error.WriteLine("Login failed: Unable to retrieve user information");
                // Even if we fail to get user info, we still have a valid token so we can consider the login successful. We just won't have the user info in our app until they refresh and we try to load it from the token again (which will also fail but at least they can see some kind of logged in state instead of being immediately logged out again).
            }

            // Store the access token and user information
            tokenStorage.AccessToken = result.AccessToken;
            tokenStorage.Email = userResponse?.Email ?? string.Empty;
            tokenStorage.Forename = userResponse?.Forename ?? string.Empty;
            tokenStorage.Surname = userResponse?.Surname ?? string.Empty;
            tokenStorage.CustomerId = userResponse?.UserId ?? string.Empty;

            await tokenStorage.SaveToSessionStorageAsync();
            authStateProvider.NotifyAuthenticationStateChanged();
            return true;
        }

        public async Task Logout()
        {
            await tokenStorage.ClearSessionStorageAsync();
            tokenStorage.Clear();
            authStateProvider.NotifyAuthenticationStateChanged();
        }
    }
}