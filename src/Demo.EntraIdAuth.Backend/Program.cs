using System.IdentityModel.Tokens.Jwt;

using Demo.EntraIdAuth.Backend;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

var features = FeatureHelper.GetFeatures(builder.Configuration.GetSection("Features"));

// This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
// By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
// 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
// This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    // .AddJwtBearer(options =>
    // {
    //     options.Authority = "https://sts.windows.net/3eef8910-0332-4feb-9436-8c4579d2696d/";    // the issuer
    //     options.Audience = "api://8d70de13-f741-4c10-b1d6-6a3571392620";
    //     options.MapInboundClaims = false;
    // });

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    if(features.UseGroupAuthorization)
    {
        options.TokenValidationParameters.RoleClaimType = "groups";
    }
    else if(features.UseAppRoleAuthorization)
    {
        options.TokenValidationParameters.RoleClaimType = "roles";
    }
});
    
builder.Services.AddAuthorization(options =>
{
    // check scope
    options.AddPolicy("Scope:Weather.Read", policy => policy.RequireScope("Weather.Read"));

    // check user permissions based on groups
    if(features.UseGroupAuthorization)
    {
        var weatherUsersGroupObjectId = builder.Configuration.GetValue<string>("Groups:WeatherUsers")!;
        options.AddPolicy("WeaterUsersGroupRequired", policy => policy.RequireRole([weatherUsersGroupObjectId]));
    }

    // check user permissions based on app roles
    if(features.UseAppRoleAuthorization)
    {
        var weatherGetAppRole = builder.Configuration.GetValue<string>("AppRoles:WeatherGet")!;
        options.AddPolicy("WeatherGetAppRoleRequired", policy => policy.RequireRole([weatherGetAppRole]));
    }
});

// The following flag can be used to get more descriptive errors in development environments
// Enable diagnostic logging to help with troubleshooting. For more details, see https://aka.ms/IdentityModel/PII.
// You might not want to keep this following flag on for production
IdentityModelEventSource.ShowPII = true;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// var scopeRequiredByApi = app.Configuration["AzureAd:Scopes"] ?? "";
var authorizationPolicies = new List<string>()
{
    "Scope:Weather.Read",
};
if(features.UseGroupAuthorization)
{
    authorizationPolicies.Add("WeaterUsersGroupRequired");
}
else if(features.UseAppRoleAuthorization)
{
    authorizationPolicies.Add("WeatherGetAppRoleRequired");
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/weatherforecast", (HttpContext httpContext) =>
{
    // httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi()
.RequireAuthorization(authorizationPolicies.ToArray());

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
