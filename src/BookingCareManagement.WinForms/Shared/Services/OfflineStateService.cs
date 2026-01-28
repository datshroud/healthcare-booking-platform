using System;

namespace BookingCareManagement.WinForms.Shared.Services;

public sealed class OfflineStateService
{
    private readonly object _syncRoot = new();
    private bool _isOffline;
    private bool _notified;
    private string? _reason;
    private DateTime _lastChangedUtc;

    public event EventHandler? StateChanged;

    public bool IsOffline
    {
        get
        {
            lock (_syncRoot)
            {
                return _isOffline;
            }
        }
    }

    public string Reason
    {
        get
        {
            lock (_syncRoot)
            {
                return _reason ?? string.Empty;
            }
        }
    }

    public DateTime LastChangedUtc
    {
        get
        {
            lock (_syncRoot)
            {
                return _lastChangedUtc;
            }
        }
    }

    public bool MarkOffline(string reason)
    {
        lock (_syncRoot)
        {
            if (_isOffline)
            {
                return false;
            }

            _isOffline = true;
            _reason = reason;
            _lastChangedUtc = DateTime.UtcNow;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool MarkOnline()
    {
        lock (_syncRoot)
        {
            if (!_isOffline)
            {
                return false;
            }

            _isOffline = false;
            _reason = null;
            _notified = false;
            _lastChangedUtc = DateTime.UtcNow;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool TryNotifyOffline(DialogService dialogs)
    {
        lock (_syncRoot)
        {
            if (_notified)
            {
                return false;
            }

            _notified = true;
        }

        dialogs.ShowError("Không có mạng. Đang chuyển sang chế độ offline (dữ liệu cục bộ).");
        return true;
    }
}
