using System.Security.Cryptography;
using System.Text;

namespace PortfolioManager.Api.Helpers;

public static class LoggingHelper
{
    /// <summary>
    /// Hash email for safe logging (first 8 chars of SHA256)
    /// </summary>
    public static string HashEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "empty";

        var emailLower = email.ToLowerInvariant();
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(emailLower));
        return Convert.ToHexString(hashBytes)[..8];
    }

    /// <summary>
    /// Mask email for safe logging (show first 2 chars and domain)
    /// </summary>
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "***";

        var parts = email.Split('@');
        if (parts.Length != 2)
            return "***";

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"**@{domain}";

        return $"{localPart[..2]}***@{domain}";
    }
}
