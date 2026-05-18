using System.Net.Http.Json;

namespace WaterTracker.Services
{
    public class AuthService(HttpClient http, TokenStorageService tokenStorage, AuthStateProvider authStateProvider)
    {
        private record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
        private record UserResponse(string Email, string Forename, string Surname, string UserId);

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
                return false;

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (result is null || string.IsNullOrEmpty(result.AccessToken))
                return false;

            var userResponse = await http.GetFromJsonAsync<UserResponse>("/api/user/me");
            if (userResponse is null)
                return false;

            // Store the access token and user information
            tokenStorage.AccessToken = result.AccessToken;
            tokenStorage.Email = userResponse.Email;
            tokenStorage.Forename = userResponse.Forename;
            tokenStorage.Surname = userResponse.Surname;
            tokenStorage.CustomerId = userResponse.UserId;

            authStateProvider.NotifyAuthenticationStateChanged();
            return true;
        }

        public void Logout()
        {
            tokenStorage.Clear();
            authStateProvider.NotifyAuthenticationStateChanged();
        }
    }
}