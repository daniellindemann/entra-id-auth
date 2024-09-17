namespace Demo.EntraIdAuth.Backend;

public class FeatureHelper
{
    public bool UseGroupAuthorization { get; set; }
    public bool UseAppRoleAuthorization { get; set; }

    public static FeatureHelper GetFeatures(IConfigurationSection configurationSection)
    {
        var features = new FeatureHelper();
        configurationSection.Bind(features);
        return features;
    }
}