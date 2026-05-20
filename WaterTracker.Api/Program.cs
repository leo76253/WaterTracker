using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using WaterTracker.Api.Data;
using WaterTracker.Api.Models;
using WaterTracker.Api.Services;
using WaterTracker.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("WaterTrackerTesting"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddIdentity<Customer, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<AppDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");

        if (builder.Environment.IsDevelopment())
            options.SetIssuer(new Uri("https://localhost:7184"));

        options.AllowPasswordFlow()
               .SetAccessTokenLifetime(TimeSpan.FromHours(24));

        options.AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey();

        var aspNetCoreOptions = options.UseAspNetCore()
                                        .EnableTokenEndpointPassthrough();

        if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
            aspNetCoreOptions.DisableTransportSecurityRequirement();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IWaterIntakeService, WaterIntakeService>();

//TODO: Add rate limiting if I get time at the end (to token endpoint at least, maybe also registration endpoint).

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Water Tracker API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Paste your Bearer token here"
    };
    options.AddSecurityDefinition("Bearer", securityScheme);

    options.OperationFilter<FormDataOperationFilter>();
    options.DocumentFilter<BearerSecurityFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7124", "http://localhost:5064")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

await OpenIddictInitializer.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    // Supposedly no longer the recommended/default anymore (I think from .net 8 but could be wrong) but I like the swagger UI and its not a production application so whats the harm?
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Skip HTTPS redirect in development - it strips Authorization headers from Swagger UI requests
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Global error handling middleware - Some see this as lazy but its a good way to ensure we catch all unhandled exceptions and return a consistent error response format. I would also log the exception here if this were a real application.
// Admittedly this was a nice snippet given to me by copilot as I was going to be very lazy and just console log and return straight 500 given its a non production environment.
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        await context.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected error occurred",
            error = app.Environment.IsDevelopment() ? exception?.Message : null
        });
    });
});

app.Run();

public class FormDataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath is string path
            && path.Contains("connect/token")
            && operation.RequestBody?.Content.ContainsKey("multipart/form-data") == true)
        {
            operation.RequestBody.Content["application/x-www-form-urlencoded"] =
                operation.RequestBody.Content["multipart/form-data"];
            operation.RequestBody.Content.Remove("multipart/form-data");
        }
    }
}

public class BearerSecurityFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        document.Security ??= [];

        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document, null);

        document.Security.Add(new OpenApiSecurityRequirement
        {
            { schemeRef, [] }
        });
    }
}


