using System;
using System.Collections.Generic;
using BookingCareManagement.WinForms.Shared.Models;

namespace BookingCareManagement.WinForms.Shared.State;

/// <summary>
/// Holds authentication and tenant information for the lifetime of the WinForms app.
/// </summary>
public sealed class SessionState
{
    private readonly object _syncRoot = new();

    private string? _accessToken;
    private string? _refreshToken;
    private string? _currentUserId;
    private string? _displayName;
    private string? _email;
    private string[] _roles = Array.Empty<string>();
    private bool _isAdmin;
    private bool _isDoctor;
    private bool _cookieAuthenticated;

    public event EventHandler? StateChanged;

    public bool IsAuthenticated => _cookieAuthenticated || !string.IsNullOrWhiteSpace(_accessToken);
    public bool HasCookieSession => _cookieAuthenticated;

    public string? AccessToken
    {
        get => _accessToken;
        set
        {
            lock (_syncRoot)
            {
                _accessToken = value;
                OnStateChanged();
            }
        }
    }

    public string? RefreshToken
    {
        get => _refreshToken;
        set
        {
            lock (_syncRoot)
            {
                _refreshToken = value;
                OnStateChanged();
            }
        }
    }

    public string? CurrentUserId
    {
        get => _currentUserId;
        set
        {
            lock (_syncRoot)
            {
                _currentUserId = value;
                OnStateChanged();
            }
        }
    }

    public string DisplayName
    {
        get => _displayName ?? string.Empty;
        private set
        {
            lock (_syncRoot)
            {
                _displayName = value;
                OnStateChanged();
            }
        }
    }

    public string Email
    {
        get => _email ?? string.Empty;
        private set
        {
            lock (_syncRoot)
            {
                _email = value;
                OnStateChanged();
            }
        }
    }

    public IReadOnlyList<string> Roles => _roles;
    public bool IsAdmin => _isAdmin;
    public bool IsDoctor => _isDoctor;

    public void ApplyProfile(UserProfileDto profile)
    {
        if (profile is null)
        {
            return;
        }

        lock (_syncRoot)
        {
            _currentUserId = string.IsNullOrWhiteSpace(profile.UserId) ? null : profile.UserId;
            _displayName = string.IsNullOrWhiteSpace(profile.FullName) ? profile.Email : profile.FullName;
            _email = profile.Email;
            _roles = profile.Roles ?? Array.Empty<string>();
            _isAdmin = profile.IsAdmin;
            _isDoctor = profile.IsDoctor;
            OnStateChanged();
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _accessToken = null;
            _refreshToken = null;
            _currentUserId = null;
            _displayName = null;
            _email = null;
            _roles = Array.Empty<string>();
            _isAdmin = false;
            _isDoctor = false;
            _cookieAuthenticated = false;
            OnStateChanged();
        }
    }

    public void MarkCookieAuthenticated()
    {
        lock (_syncRoot)
        {
            _cookieAuthenticated = true;
            OnStateChanged();
        }
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public static SessionState CreateUnauthenticated() => new();
}
