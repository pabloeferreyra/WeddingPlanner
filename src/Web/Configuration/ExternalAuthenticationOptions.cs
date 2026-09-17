namespace WeddingPlanner.Web.Configuration;

public sealed class ExternalAuthenticationOptions
{
    public string Provider { get; set; } = "None";

    public ExternalProviderOptions Firebase { get; set; } = new();

    public ExternalProviderOptions Supabase { get; set; } = new();
}

public sealed class ExternalProviderOptions
{
    public string ProjectId { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;
}