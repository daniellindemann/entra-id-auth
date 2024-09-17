
namespace Demo.EntraIdAuth.Frontend.Options;

public class BackendOptions
{
    public static readonly string BackendOptionsKey = "Backend";

    public required string BaseUrl { get; set; }
    public required string Scopes { get; set; }
}

