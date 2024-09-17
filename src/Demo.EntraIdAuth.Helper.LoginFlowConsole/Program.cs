using System.Text.Json;

using Demo.EntraIdAuth.Helper.LoginFlowConsole;
using Demo.EntraIdAuth.Helper.LoginFlowConsole.Config;

using IdentityModel.OidcClient;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IHostBuilder builder = Host.CreateDefaultBuilder(args);
using IHost host = builder.Build();

IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

var azureAdConfig = new AzureAdConfig();
config.GetSection("AzureAd").Bind(azureAdConfig);

var apiConfig = new ApiConfig();
config.GetSection("Api").Bind(apiConfig);

var systemBrowserConfig = new SystemBrowserConfig();
config.GetSection("SystemBrowser").Bind(systemBrowserConfig);

var browser = new SystemBrowser(string.Empty, systemBrowserConfig.Port);
var redirectUri = string.Format($"http://localhost:{browser.Port}");

var options = new OidcClientOptions
{
    Authority = azureAdConfig.Authority,
    ClientId = azureAdConfig.ClientId,
    ClientSecret = azureAdConfig.ClientSecret,
    RedirectUri = redirectUri,
    Scope = azureAdConfig.Scope,

    ProviderInformation = new ProviderInformation
    {
        TokenEndpoint = $"{azureAdConfig.Authority}/oauth2/v2.0/token",
        AuthorizeEndpoint = $"{azureAdConfig.Authority}/oauth2/v2.0/authorize",
        IssuerName = $"{azureAdConfig.Authority}/v2.0",
        UserInfoEndpoint = "https://graph.microsoft.com/oidc/userinfo"
    },

    FilterClaims = false,
    Browser = browser,
    DisablePushedAuthorization = true,
    // IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
    RefreshTokenInnerHttpHandler = new SocketsHttpHandler(),

    LoggerFactory = loggerFactory
};
options.Policy.Discovery.ValidateIssuerName = false;
options.Policy.Discovery.RequireKeySet = false;

if(azureAdConfig.Scope.Contains("api:", StringComparison.InvariantCultureIgnoreCase))
{
    options.LoadProfile = false;
}

OidcClient oidcClient = new(options);
var result = await oidcClient.LoginAsync(new LoginRequest());

if (result.IsError)
{
    Console.WriteLine($"Error: {result.Error}");
    return;
}

Console.WriteLine("\n\nClaims:");
foreach (var claim in result.User.Claims)
{
    Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
}

var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.TokenResponse.Raw!);

Console.WriteLine($"token response...");
foreach (var item in values!)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}

Console.WriteLine("Press any key to call api");
Console.ReadLine();

// Do api call with token
HttpClient apiClient = new(result.RefreshTokenHandler)
{
    BaseAddress = new Uri(apiConfig.BaseUrl)
};

var httpResponse = await apiClient.GetAsync(apiConfig.Path);

if(httpResponse.IsSuccessStatusCode)
{
    var content = await httpResponse.Content.ReadAsStringAsync();
    Console.WriteLine(content);
}
else
{
    Console.WriteLine($"Error: {httpResponse.StatusCode}");
}

