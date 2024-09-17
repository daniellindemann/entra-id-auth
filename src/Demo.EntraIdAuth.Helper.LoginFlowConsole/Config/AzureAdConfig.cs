namespace Demo.EntraIdAuth.Helper.LoginFlowConsole.Config;

public class AzureAdConfig
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Scope { get; set; } = null!;

    public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}";
    // public string Authority => $"https://sts.windows.net/{TenantId}/";
}
