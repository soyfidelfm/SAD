public sealed class MicrosoftOAuthOptions
{
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string FrontendCallbackUrl { get; set; } = "";
    public string FrontendLoginUrl { get; set; } = "";
    public string FrontendSuccessUrl { get; set; } = "";

}
