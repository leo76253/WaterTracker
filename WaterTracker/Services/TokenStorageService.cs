namespace WaterTracker.Services;

public class TokenStorageService
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


}