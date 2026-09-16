using Scrubbler.PluginBase.Plugin;

namespace Scrubbler.Tests.PluginBaseTest.Plugin;

[TestFixture]
internal class ScrobbleTimeViewModelTest
{
    [Test]
    public async Task Construction_without_ui_context_does_not_publish_background_notifications()
    {
        using var vm = await Task.Run(() => new ScrobbleTimeViewModel());
        var notifications = 0;
        vm.PropertyChanged += (_, _) => Interlocked.Increment(ref notifications);
        await Task.Delay(1300);
        Assert.That(notifications, Is.Zero);
    }

    [Test]
    public void Refresh_raises_all_clock_notifications_synchronously_on_calling_thread()
    {
        using var vm = new ScrobbleTimeViewModel();
        var thread = Environment.CurrentManagedThreadId;
        var changes = new List<string?>();
        vm.PropertyChanged += (_, e) =>
        {
            Assert.That(Environment.CurrentManagedThreadId, Is.EqualTo(thread));
            changes.Add(e.PropertyName);
        };
        vm.RefreshCurrentTime();
        Assert.That(changes, Is.EqualTo(new[] { "Time", "Date", "Timestamp", "IsTimeValid" }));
    }

    [Test]
    public void Refresh_manual_time_checks_validity_without_changing_time_bindings()
    {
        using var vm = new ScrobbleTimeViewModel { UseCurrentTime = false };
        var changes = new List<string?>();
        vm.PropertyChanged += (_, e) => changes.Add(e.PropertyName);
        vm.RefreshCurrentTime();
        Assert.That(changes, Is.EqualTo(new[] { "IsTimeValid" }));
    }

    [Test]
    public void Disposed_view_model_ignores_late_refresh_and_repeated_disposal()
    {
        var vm = new ScrobbleTimeViewModel();
        vm.Dispose();
        vm.PropertyChanged += (_, _) => Assert.Fail("Disposed model must not refresh bindings.");
        Assert.DoesNotThrow(() => { vm.RefreshCurrentTime(); vm.Dispose(); });
    }

    [Test]
    public void UsesCurrentTime_ByDefault()
    {
        var time = new FakeTimeProvider(DateTimeOffset.Parse("2025-01-01T10:00:00"));
        using var vm = new ScrobbleTimeViewModel(time);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.UseCurrentTime, Is.True);
            Assert.That(vm.Time, Is.EqualTo(TimeSpan.FromHours(10)));
        }
    }

    [Test]
    public void DisablingUseCurrentTime_FreezesTime()
    {
        var time = new FakeTimeProvider(DateTimeOffset.Parse("2025-01-01T10:00:00"));
        using var vm = new ScrobbleTimeViewModel(time);

        vm.UseCurrentTime = false;
        var frozen = vm.Time;

        time.Advance(TimeSpan.FromHours(1));

        Assert.That(vm.Time, Is.EqualTo(frozen));
    }

    [Test]
    public void Timestamp_IsDatePlusTime()
    {
        var time = new FakeTimeProvider(DateTimeOffset.Now);
        using var vm = new ScrobbleTimeViewModel(time)
        {
            UseCurrentTime = false
        };

        vm.Date = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero);
        vm.Time = new TimeSpan(12, 30, 0);

        Assert.That(
            vm.Timestamp,
            Is.EqualTo(new DateTimeOffset(2024, 01, 01, 12, 30, 0, TimeSpan.Zero)));
    }

    [Test]
    public void IsTimeValid_True_ForRecentTime()
    {
        var now = DateTimeOffset.Parse("2025-01-15T10:00:00");
        var time = new FakeTimeProvider(now);

        using var vm = new ScrobbleTimeViewModel(time)
        {
            UseCurrentTime = false
        };

        var valid = now.AddDays(-1);
        vm.Date = valid.Date;
        vm.Time = valid.TimeOfDay;

        Assert.That(vm.IsTimeValid, Is.True);
    }

    [Test]
    public void IsTimeValid_False_ForTooOldTime()
    {
        var now = DateTimeOffset.Parse("2025-01-15T10:00:00");
        var time = new FakeTimeProvider(now);

        using var vm = new ScrobbleTimeViewModel(time)
        {
            UseCurrentTime = false
        };

        var invalid = now.AddDays(-15);
        vm.Date = invalid.Date;
        vm.Time = invalid.TimeOfDay;

        Assert.That(vm.IsTimeValid, Is.False);
    }
}
