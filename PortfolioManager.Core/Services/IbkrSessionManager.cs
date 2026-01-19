using System.Net;
using Microsoft.Extensions.Logging;

namespace PortfolioManager.Core.Services;

/// <summary>
/// Session manager for IBKR web portal authentication.
/// Handles cookie-based session management for web portal access.
/// </summary>
public interface IIbkrSessionManager
{
    /// <summary>
    /// Store session cookies after user authenticates through IBKR website
    /// </summary>
    void SetSessionCookies(string userId, CookieContainer cookies);
    
    /// <summary>
    /// Get stored session cookies for a user
    /// </summary>
    CookieContainer? GetSessionCookies(string userId);
    
    /// <summary>
    /// Check if user has valid session
    /// </summary>
    bool HasValidSession(string userId);
    
    /// <summary>
    /// Clear session for a user
    /// </summary>
    void ClearSession(string userId);
}

public class IbkrSessionManager : IIbkrSessionManager
{
    private readonly Dictionary<string, SessionData> _sessions = new();
    private readonly ILogger<IbkrSessionManager> _logger;
    
    public IbkrSessionManager(ILogger<IbkrSessionManager> logger)
    {
        _logger = logger;
    }
    
    public void SetSessionCookies(string userId, CookieContainer cookies)
    {
        _sessions[userId] = new SessionData
        {
            Cookies = cookies,
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };
        
        _logger.LogInformation("Session stored for user: {UserId}", userId);
    }
    
    public CookieContainer? GetSessionCookies(string userId)
    {
        if (_sessions.TryGetValue(userId, out var session))
        {
            // Check if session is expired (e.g., 24 hours)
            if (DateTime.UtcNow - session.CreatedAt < TimeSpan.FromHours(24))
            {
                session.LastAccessedAt = DateTime.UtcNow;
                return session.Cookies;
            }
            
            // Session expired, remove it
            _sessions.Remove(userId);
            _logger.LogWarning("Session expired for user: {UserId}", userId);
        }
        
        return null;
    }
    
    public bool HasValidSession(string userId)
    {
        return GetSessionCookies(userId) != null;
    }
    
    public void ClearSession(string userId)
    {
        if (_sessions.Remove(userId))
        {
            _logger.LogInformation("Session cleared for user: {UserId}", userId);
        }
    }
    
    private class SessionData
    {
        public CookieContainer Cookies { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
    }
}
