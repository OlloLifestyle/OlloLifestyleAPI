using System.Text.RegularExpressions;
using System.Text.Json;

namespace OlloLifestyleAPI.Application.Authorization;

/// <summary>
/// Helper to normalize permission scopes/actions and build JWT claim types.
/// Ensures we only emit/compare against the allowed action set.
/// </summary>
public static class PermissionClaimHelper
{
    private static readonly Dictionary<string, string> ActionNormalizationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["access"] = "access",
        ["read"] = "read",
        ["view"] = "read",
        ["write"] = "write",
        ["create"] = "write",
        ["update"] = "write",
        ["edit"] = "write",
        ["delete"] = "delete",
        ["remove"] = "delete",
        ["suspend"] = "write"
    };

    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "access",
        "read",
        "write",
        "delete"
    };

    public static string GetClaimType(string scope) => $"permission_{NormalizeScope(scope)}";

    public static string NormalizeScope(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return "unknown";
        }

        var cleaned = new string(scope.Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray());

        cleaned = Regex.Replace(cleaned, "_+", "_").Trim('_');
        return string.IsNullOrWhiteSpace(cleaned) ? "unknown" : cleaned;
    }

    public static string? NormalizeAction(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return null;
        }

        var key = action.Trim().ToLowerInvariant();
        if (ActionNormalizationMap.TryGetValue(key, out var normalized))
        {
            return normalized;
        }

        if (AllowedActions.Contains(key))
        {
            return key;
        }

        return null;
    }

    public static (string Scope, string? Action) ParsePermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return ("unknown", null);
        }

        var trimmed = permission.Trim();
        var lastDotIndex = trimmed.LastIndexOf('.');
        if (lastDotIndex <= 0 || lastDotIndex == trimmed.Length - 1)
        {
            return (NormalizeScope(trimmed), null);
        }

        var scope = trimmed[..lastDotIndex];
        var action = trimmed[(lastDotIndex + 1)..];
        return (NormalizeScope(scope), NormalizeAction(action));
    }

    public static IEnumerable<string> ExtractActionsFromClaimValue(string claimValue)
    {
        var normalizedActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            var trimmed = claimValue.Trim();
            if (trimmed.StartsWith("[", StringComparison.Ordinal))
            {
                try
                {
                    var doc = JsonDocument.Parse(trimmed);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var element in doc.RootElement.EnumerateArray())
                        {
                            if (element.ValueKind == JsonValueKind.String)
                            {
                                var normalized = NormalizeAction(element.GetString() ?? string.Empty);
                                if (normalized is not null)
                                {
                                    normalizedActions.Add(normalized);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // If parsing fails, fall through to single-value handling
                }
            }

            var single = NormalizeAction(claimValue);
            if (single is not null)
            {
                normalizedActions.Add(single);
            }
        }

        return normalizedActions;
    }
}
