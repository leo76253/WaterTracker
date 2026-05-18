using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WaterTracker;
using WaterTracker.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl configuration is missing.");

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<TokenMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<TokenMessageHandler>();
    return new HttpClient(handler) { BaseAddress = new Uri(apiUrl) };
});

await builder.Build().RunAsync();
