
using System.Net.Sockets;

using Demo.EntraIdAuth.Frontend.Services;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;

namespace Demo.EntraIdAuth.Frontend.Pages;

// [AuthorizeForScopes(Scopes = ["api://8d70de13-f741-4c10-b1d6-6a3571392620/Weather.Read"])]
[AuthorizeForScopes(ScopeKeySection = "Backend:Scopes")]
public class IndexModel : PageModel
{
    private readonly IBackendViaDownstreamApiService _backendViaDownstreamApiService;
    private readonly IBackendWithoutHelperClassService _backendWithoutHelperClassService;

    public IndexModel(IBackendViaDownstreamApiService backendViaDownstreamApiService, 
        IBackendWithoutHelperClassService backendWithoutHelperClassService)
    {
        _backendViaDownstreamApiService = backendViaDownstreamApiService;
        _backendWithoutHelperClassService = backendWithoutHelperClassService;
    }

    public string? WeatherDataWithHelperClass { get; private set; } = string.Empty;
    public string? WeatherDataWithoutHelperClass { get; private set; } = string.Empty;

    public async Task OnGet()
    {
        WeatherDataWithHelperClass = await GetContentWithHelperClass();
        WeatherDataWithoutHelperClass = await GetContentWithoutHelperClass();
    }

    private async Task<string?> GetContentWithHelperClass()
    {
        var user = User;
        try
        {
            var content = await _backendViaDownstreamApiService.GetWatherDataAsync().ConfigureAwait(false);
            return content;
        }
        catch (HttpRequestException hrex)
        {
            if(hrex.InnerException is SocketException)
            {
                return "Unable to connect to backend";
            }

            throw;
        }
        
    }

    private async Task<string?> GetContentWithoutHelperClass()
    {
        try
        {
            var content = await _backendWithoutHelperClassService.GetWatherDataAsync().ConfigureAwait(false);
            return content;
        }
        catch (HttpRequestException hrex)
        {
            if(hrex.InnerException is SocketException)
            {
                return "Unable to connect to backend";
            }

            throw;
        }
    }
}
