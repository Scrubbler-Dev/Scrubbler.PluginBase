using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.PluginBase.Plugin;

[Microsoft.UI.Xaml.Data.Bindable]
public partial class ScrobbleTimeViewModel : ObservableObject, IDisposable
{
    private readonly TimeProvider _timeProvider;
    private bool _disposed;

    private DateTimeOffset _date;
    private TimeSpan _time;
    private bool _useCurrentTime;

    public DateTimeOffset Date
    {
        get => UseCurrentTime ? _timeProvider.GetLocalNow().Date : _date;
        set
        {
            if (Date != value)
            {
                _date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }

    public TimeSpan Time
    {
        get => UseCurrentTime ? _timeProvider.GetLocalNow().TimeOfDay : _time;
        set
        {
            if (Time != value)
            {
                _time = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }

    public bool UseCurrentTime
    {
        get => _useCurrentTime;
        set
        {
            if (_useCurrentTime != value)
            {
                if (!value)
                {
                    var now = _timeProvider.GetLocalNow();
                    _date = new DateTimeOffset(now.Date, now.Offset);
                    _time = now.TimeOfDay;
                }

                _useCurrentTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(Date));
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }

    public DateTimeOffset Timestamp => Date + Time;

    public bool IsTimeValid
    {
        get
        {
            var now = _timeProvider.GetLocalNow();
            return Timestamp >= now.AddDays(-14) && Timestamp < now.AddDays(1);
        }
    }

    public ScrobbleTimeViewModel(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        UseCurrentTime = true;

    }

    /// <summary>
    /// Refreshes time-dependent bindings on the caller's thread. The time control
    /// calls this from its UI DispatcherTimer; this model owns no background work.
    /// </summary>
    public void RefreshCurrentTime()
    {
        if (_disposed)
            return;

        if (UseCurrentTime)
        {
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(Date));
            OnPropertyChanged(nameof(Timestamp));
        }

        OnPropertyChanged(nameof(IsTimeValid));
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
