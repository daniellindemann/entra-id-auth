using Demo.EntraIdAuth.Frontend.Options;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;

namespace Demo.EntraIdAuth.Frontend.Services;

public interface IBackendViaDownstreamApiService : IBackendService
{
}

public class BackendViaDownstreamApiService : IBackendViaDownstreamApiService
{
    private static readonly string DownstreamApi = "Demo.EntraIdAuth.Backend";

    private readonly BackendOptions _backendOptions;
    private readonly IDownstreamApi _downstreamApi;

    public BackendViaDownstreamApiService(IOptions<BackendOptions> backendOptions, IDownstreamApi downstreamApi)
    {
        _backendOptions = backendOptions.Value;
        _downstreamApi = downstreamApi;
    }

    public async Task<string?> GetWatherDataAsync()
    {
        // https://learn.microsoft.com/en-us/entra/identity-platform/scenario-web-app-call-api-call-api?tabs=aspnetcore#option-2-call-a-downstream-web-api-with-the-helper-class
        // https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/master/4-WebApp-your-API/4-1-MyOrg/Client/Controllers/TodoListController.cs

        var response = await _downstreamApi.CallApiForUserAsync(DownstreamApi, options =>
        {
            options.HttpMethod = "Get";
            options.RelativePath = "/weatherforecast";
            options.Scopes = _backendOptions.Scopes?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        string? content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return content;
    }
}
