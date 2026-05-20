using Microsoft.JSInterop;

namespace WaterTracker.Services;

public class TokenStorageService(IJSRuntime js)
{
    public string? AccessToken { get; set; }
    public string? Email { get; set; }
    public string? Forename { get; set; }
    public string? Surname { get; set; }
    public string? CustomerId { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public void Clear()
    {
        AccessToken = null;
        Email = null;
        Forename = null;
        Surname = null;
        CustomerId = null;
    }

    public async Task LoadFromSessionStorageAsync()
    {
        AccessToken = await js.InvokeAsync<string?>("sessionStorage.getItem", "access_token");
        Email = await js.InvokeAsync<string?>("sessionStorage.getItem", "email");
        Forename = await js.InvokeAsync<string?>("sessionStorage.getItem", "forename");
        Surname = await js.InvokeAsync<string?>("sessionStorage.getItem", "surname");
        CustomerId = await js.InvokeAsync<string?>("sessionStorage.getItem", "customer_id");
    }

    public async Task SaveToSessionStorageAsync()
    {
        await js.InvokeVoidAsync("sessionStorage.setItem", "access_token", AccessToken);
        await js.InvokeVoidAsync("sessionStorage.setItem", "email", Email);
        await js.InvokeVoidAsync("sessionStorage.setItem", "forename", Forename);
        await js.InvokeVoidAsync("sessionStorage.setItem", "surname", Surname);
        await js.InvokeVoidAsync("sessionStorage.setItem", "customer_id", CustomerId);
    }

    public async Task ClearSessionStorageAsync()
    {
        await js.InvokeVoidAsync("sessionStorage.removeItem", "access_token");
        await js.InvokeVoidAsync("sessionStorage.removeItem", "email");
        await js.InvokeVoidAsync("sessionStorage.removeItem", "forename");
        await js.InvokeVoidAsync("sessionStorage.removeItem", "surname");
        await js.InvokeVoidAsync("sessionStorage.removeItem", "customer_id");
    }
}
