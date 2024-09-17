using System.IdentityModel.Tokens.Jwt;

using Demo.EntraIdAuth.Frontend.Options;
using Demo.EntraIdAuth.Frontend.Services;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

// This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
// By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
// 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
// This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Add services to the container.
/*
AddMicrosoftIdentityWebApp() wraps the OpenID Connect and Cookie authentication handlers

    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)        
        .AddOpenIdConnect(options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.SignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.Authority = "https://login.microsoftonline.com/00000000-0000-0000-0000-000000000000/v2.0/";
            options.ClientId = "00000000-0000-0000-0000-000000000000";
            options.ClientSecret = "[Read from a configuration source]";
            ...
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
*/
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    // .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);

        // The following lines code instruct the asp.net core middleware to use the data in the "roles" claim in the [Authorize] attribute, policy.RequireRole() and User.IsInRole()
        // See https://docs.microsoft.com/aspnet/core/security/authorization/roles for more info.
        options.TokenValidationParameters.RoleClaimType = "groups";
    })
    // allow the app to get an token for the api
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDownstreamApi("Demo.EntraIdAuth.Backend", builder.Configuration.GetSection("Backend"))
    .AddInMemoryTokenCaches();
builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

// The following flag can be used to get more descriptive errors in development environments
// Enable diagnostic logging to help with troubleshooting. For more details, see https://aka.ms/IdentityModel/PII.
// You might not want to keep this following flag on for production
IdentityModelEventSource.ShowPII = true;

builder.Services.AddOptions<BackendOptions>()
    .Bind(builder.Configuration.GetSection(BackendOptions.BackendOptionsKey));

builder.Services.AddTransient<IBackendViaDownstreamApiService, BackendViaDownstreamApiService>();
builder.Services.AddHttpClient<IBackendWithoutHelperClassService, BackendWithoutHelperClassService>((serviceProvider, httpClient) =>
{
    var backendOptions = serviceProvider.GetRequiredService<IOptions<BackendOptions>>().Value;
    httpClient.BaseAddress = new Uri(backendOptions.BaseUrl);
});

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
