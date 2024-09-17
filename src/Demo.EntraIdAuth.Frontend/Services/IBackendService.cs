namespace Demo.EntraIdAuth.Frontend.Services;

public interface IBackendService
{
    Task<string?> GetWatherDataAsync();
}
