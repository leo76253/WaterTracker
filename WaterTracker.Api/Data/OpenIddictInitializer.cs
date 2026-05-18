using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WaterTracker.Api.Data;

public static class OpenIddictInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var existing = await manager.FindByClientIdAsync("watertracker-blazor");
        if (existing is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "watertracker-blazor",
                DisplayName = "Water Tracker Blazor Client",
                ConsentType = ConsentTypes.Implicit,
                ClientType = ClientTypes.Public,
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.Password,
                    Permissions.Prefixes.Scope + "api"
                }
            });
        }
    }
}
