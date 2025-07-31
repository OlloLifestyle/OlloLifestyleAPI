namespace OlloLifestyleAPI.Configuration;

/// <summary>
/// Predefined rate limiting policies for common scenarios
/// </summary>
public static class RateLimitPolicies
{
    public const string Auth = "AuthPolicy";
    public const string Api = "ApiPolicy";
    public const string Upload = "UploadPolicy";
    public const string Public = "PublicPolicy";
}