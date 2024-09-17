
using Demo.EntraIdAuth.Frontend.Options;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;

namespace Demo.EntraIdAuth.Frontend.Services;

public interface IBackendWithoutHelperClassService : IBackendService
{
}

public class BackendWithoutHelperClassService : IBackendWithoutHelperClassService
{
    private readonly BackendOptions _backendOptions;
    private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;
    private readonly HttpClient _httpClient;

    public BackendWithoutHelperClassService(IOptions<BackendOptions> backendOptions,
        IAuthorizationHeaderProvider authorizationHeaderProvider,
        HttpClient httpClient)
    {
        _backendOptions = backendOptions.Value;
        _authorizationHeaderProvider = authorizationHeaderProvider;
        _httpClient = httpClient;
    }

    public async Task<string?> GetWatherDataAsync()
    {
        // https://learn.microsoft.com/en-us/entra/identity-platform/scenario-web-app-call-api-call-api?tabs=aspnetcore#option-3-call-a-downstream-web-api-without-the-helper-class

        string authorizationHeader = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(_backendOptions.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        _httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

        var response = await _httpClient.GetAsync("/weatherforecast").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        string? content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return content;
    }
}
