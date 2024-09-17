using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

using Demo.EntraIdAuth.Frontend.Options;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace Demo.EntraIdAuth.Frontend.Pages;

// [AuthorizeForScopes(Scopes = ["api://8d70de13-f741-4c10-b1d6-6a3571392620/Weather.Read"])]
[AuthorizeForScopes(ScopeKeySection = "Backend:Scopes")]
public class AccessTokenModel : PageModel
{
    private static readonly string[] DefaultHighlightKeys = ["aud", "iss", "appid", "groups", "roles", "scp"];

    private readonly BackendOptions _backendOptions;
    private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;

    public AccessTokenModel(IOptions<BackendOptions> backendOptions,
        IAuthorizationHeaderProvider authorizationHeaderProvider)
    {
        _backendOptions = backendOptions.Value;
        _authorizationHeaderProvider = authorizationHeaderProvider;
    }

    public string? AccessToken { get; private set; } = string.Empty;
    public string? DecodedAccessToken { get; private set; } = string.Empty;

    public async Task OnGet()
    {
        // access token
        var scopes = _backendOptions.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string authorizationHeader = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
        AccessToken = authorizationHeader.Split(' ')[1];

        // decoded access token
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(AccessToken) as JwtSecurityToken;
        var headerJson = Encoding.UTF8.GetString(Convert.FromBase64String(jsonToken?.RawHeader!));
        var payloadJson = Encoding.UTF8.GetString(ParseBase64WithoutPadding(jsonToken?.RawPayload!));
        DecodedAccessToken = SimpleJsonHtmlify(headerJson) + "<br/>.<br/>" + SimpleJsonHtmlify(payloadJson) + "<br/>.<br/>[Signature]";
    }

    private string? SimpleJsonHtmlify(string jsonString, params string[] highlightKeys)
    {
        var jsonDoc = JsonDocument.Parse(jsonString);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var prettyJson = JsonSerializer.Serialize(jsonDoc.RootElement, options);

        var newText = prettyJson.Replace(" ", "&nbsp;&nbsp;")
            .Replace("\n", "<br/>");
        
        foreach (var key in highlightKeys.Length == 0 ? DefaultHighlightKeys : highlightKeys)
        {
            newText = newText.Replace($"\"{key}\":", $"<b>\"{key}\"</b>:");
        }
        
        return newText;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
