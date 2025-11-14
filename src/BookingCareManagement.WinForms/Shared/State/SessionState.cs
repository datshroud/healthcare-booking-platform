using System;

namespace BookingCareManagement.WinForms.Shared.State;

/// <summary>
/// Holds authentication and tenant information for the lifetime of the WinForms app.
/// </summary>
public sealed class SessionState
{
    private readonly object _syncRoot = new();

    private string? _accessToken;
    private string? _refreshToken;
    private Guid? _currentUserId;

    public event EventHandler? StateChanged;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_accessToken);

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

    public Guid? CurrentUserId
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

    public void Clear()
    {
        lock (_syncRoot)
        {
            _accessToken = null;
            _refreshToken = null;
            _currentUserId = null;
            OnStateChanged();
        }
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public static SessionState CreateUnauthenticated() => new();
}
