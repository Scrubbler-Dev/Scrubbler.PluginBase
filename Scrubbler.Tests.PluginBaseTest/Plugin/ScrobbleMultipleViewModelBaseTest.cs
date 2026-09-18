using Moq;
using Scrubbler.PluginBase;

namespace Scrubbler.Tests.PluginBaseTest.Plugin;

[TestFixture]
internal class ScrobbleMultipleViewModelBaseTest
{
    private static Mock<IScrobbableObjectViewModel> CreateScrobbleMock(
        bool toScrobble = false,
        bool isSelected = false,
        bool canBeScrobbled = true)
    {
        var mock = new Mock<IScrobbableObjectViewModel>();

        var toScrobbleField = toScrobble;
        var isSelectedField = isSelected;

        // required string properties
        mock.SetupProperty(x => x.TrackName, "Track");
        mock.SetupProperty(x => x.ArtistName, "Artist");
        mock.SetupProperty(x => x.AlbumName, null);
        mock.SetupProperty(x => x.AlbumArtistName, null);

        mock.SetupGet(x => x.CanBeScrobbled).Returns(canBeScrobbled);

        // ToScrobble
        mock.SetupGet(x => x.ToScrobble).Returns(() => toScrobbleField);
        mock.SetupSet(x => x.ToScrobble = It.IsAny<bool>())
            .Callback<bool>(value =>
            {
                if (toScrobbleField != value)
                {
                    toScrobbleField = value;
                    mock.Raise(x => x.ToScrobbleChanged += null, EventArgs.Empty);
                }
            });

        mock.Setup(x => x.UpdateToScrobbleSilent(It.IsAny<bool>()))
            .Callback<bool>(value => toScrobbleField = value);

        // IsSelected
        mock.SetupGet(x => x.IsSelected).Returns(() => isSelectedField);
        mock.SetupSet(x => x.IsSelected = It.IsAny<bool>())
            .Callback<bool>(value =>
            {
                if (isSelectedField != value)
                {
                    isSelectedField = value;
                    mock.Raise(x => x.IsSelectedChanged += null, EventArgs.Empty);
                }
            });

        mock.Setup(x => x.UpdateIsSelectedSilent(It.IsAny<bool>()))
            .Callback<bool>(value => isSelectedField = value);

        return mock;
    }

    [Test]
    public void CheckAll_SkipsIneligibleItems()
    {
        var eligible = CreateScrobbleMock();
        var expired = CreateScrobbleMock(canBeScrobbled: false);
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([eligible.Object, expired.Object]);

        vm.CheckAllCommand.Execute(null);

        Assert.That(eligible.Object.ToScrobble, Is.True);
        Assert.That(expired.Object.ToScrobble, Is.False);
        Assert.That(vm.ToScrobbleCount, Is.EqualTo(1));
    }

    [Test]
    public void CheckSelected_RequiresEligibilityAndSkipsIneligibleItems()
    {
        var eligible = CreateScrobbleMock(isSelected: true);
        var expired = CreateScrobbleMock(isSelected: true, canBeScrobbled: false);
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([expired.Object]);
        Assert.That(vm.CheckSelectedCommand.CanExecute(null), Is.False);
        Assert.DoesNotThrow(() => vm.CheckSelectedCommand.Execute(null));

        vm.Scrobbles.Add(eligible.Object);
        Assert.That(vm.CheckSelectedCommand.CanExecute(null), Is.True);
        vm.CheckSelectedCommand.Execute(null);

        Assert.That(eligible.Object.ToScrobble, Is.True);
        Assert.That(expired.Object.ToScrobble, Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Uncheck_CanClearIneligibleItems(bool selectedOnly)
    {
        var expired = CreateScrobbleMock(toScrobble: true, isSelected: true, canBeScrobbled: false);
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([expired.Object]);

        (selectedOnly ? vm.UncheckSelectedCommand : vm.UncheckAllCommand).Execute(null);

        Assert.That(expired.Object.ToScrobble, Is.False);
    }

    [Test]
    public void EmptyBulkCommands_DoNotThrow()
    {
        var vm = new TestScrobbleMultipleViewModel();
        Assert.DoesNotThrow(() => vm.CheckAllCommand.Execute(null));
        Assert.DoesNotThrow(() => vm.CheckSelectedCommand.Execute(null));
        Assert.DoesNotThrow(() => vm.UncheckAllCommand.Execute(null));
        Assert.DoesNotThrow(() => vm.UncheckSelectedCommand.Execute(null));
    }

    [Test]
    public void InitialCollection_AddAndItemChangesNotifyBindingsAndCommands()
    {
        var vm = new TestScrobbleMultipleViewModel();
        var item = CreateScrobbleMock();
        var changes = new List<string?>();
        var commandChanges = 0;
        vm.PropertyChanged += (_, e) => changes.Add(e.PropertyName);
        vm.CheckSelectedCommand.CanExecuteChanged += (_, _) => commandChanges++;

        vm.Scrobbles.Add(item.Object);
        Assert.That(changes, Does.Contain(nameof(vm.MaxToScrobbleCount)));
        changes.Clear();
        item.Object.ToScrobble = true;
        Assert.That(changes, Does.Contain(nameof(vm.CanScrobble)));
        changes.Clear();
        commandChanges = 0;
        item.Object.IsSelected = true;
        Assert.That(changes, Does.Contain(nameof(vm.SelectedCount)));
        Assert.That(commandChanges, Is.EqualTo(1));
    }

    [TestCase("replace")]
    [TestCase("clear")]
    [TestCase("remove")]
    [TestCase("collection")]
    public void RemovedItems_StopNotifyingAndNewItemsNotify(string operation)
    {
        var oldItem = CreateScrobbleMock();
        var newItem = CreateScrobbleMock();
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([oldItem.Object]);
        var oldCollection = vm.Scrobbles;
        switch (operation)
        {
            case "replace": vm.Scrobbles[0] = newItem.Object; break;
            case "clear": vm.Scrobbles.Clear(); vm.Scrobbles.Add(newItem.Object); break;
            case "remove": vm.Scrobbles.Remove(oldItem.Object); vm.Scrobbles.Add(newItem.Object); break;
            case "collection": vm.SetScrobbles([newItem.Object]); break;
        }
        var changes = new List<string?>();
        vm.PropertyChanged += (_, e) => changes.Add(e.PropertyName);

        oldItem.Object.ToScrobble = true;
        oldItem.Object.IsSelected = true;
        if (operation == "collection")
            oldCollection.Add(CreateScrobbleMock().Object);
        Assert.That(changes, Is.Empty);

        newItem.Object.ToScrobble = true;
        Assert.That(changes, Does.Contain(nameof(vm.CanScrobble)));
        changes.Clear();
        newItem.Object.IsSelected = true;
        Assert.That(changes, Does.Contain(nameof(vm.SelectedCount)));
    }

    [Test]
    public void DuplicateItems_StaySubscribedUntilLastOccurrenceIsRemoved()
    {
        var item = CreateScrobbleMock();
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([item.Object, item.Object]);
        vm.Scrobbles.Move(0, 1);
        vm.Scrobbles.RemoveAt(0);
        var notifications = 0;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.CanScrobble)) notifications++; };

        item.Object.ToScrobble = true;
        Assert.That(notifications, Is.EqualTo(1));
        vm.Scrobbles.Clear();
        notifications = 0;
        item.Object.ToScrobble = false;
        Assert.That(notifications, Is.Zero);
    }

    [Test]
    public void CanScrobble_True_WhenAnyItemIsMarked()
    {
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles(
        [
            CreateScrobbleMock().Object,
            CreateScrobbleMock(toScrobble: true).Object
        ]);

        Assert.That(vm.CanScrobble, Is.True);
    }

    [Test]
    public void CheckAll_MarksAllAsToScrobble()
    {
        var s1 = CreateScrobbleMock();
        var s2 = CreateScrobbleMock();

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([s1.Object, s2.Object]);

        vm.CheckAllCommand.Execute(null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.Scrobbles.All(s => s.ToScrobble), Is.True);
            Assert.That(vm.ToScrobbleCount, Is.EqualTo(2));
        }
    }

    [Test]
    public void UncheckAll_ClearsAllToScrobble()
    {
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles(
        [
            CreateScrobbleMock(toScrobble: true).Object,
            CreateScrobbleMock(toScrobble: true).Object
        ]);

        vm.UncheckAllCommand.Execute(null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.Scrobbles.All(s => !s.ToScrobble), Is.True);
            Assert.That(vm.ToScrobbleCount, Is.Zero);
        }
    }

    [Test]
    public void CheckSelected_OnlyAffectsSelectedItems()
    {
        var selected = CreateScrobbleMock(isSelected: true);
        var notSelected = CreateScrobbleMock(isSelected: false);

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([selected.Object, notSelected.Object]);

        vm.CheckSelectedCommand.Execute(null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(selected.Object.ToScrobble, Is.True);
            Assert.That(notSelected.Object.ToScrobble, Is.False);
        }
    }

    [Test]
    public void UncheckSelected_OnlyClearsSelectedItems()
    {
        var selected = CreateScrobbleMock(toScrobble: true, isSelected: true);
        var notSelected = CreateScrobbleMock(toScrobble: true, isSelected: false);

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([selected.Object, notSelected.Object]);

        vm.UncheckSelectedCommand.Execute(null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(selected.Object.ToScrobble, Is.False);
            Assert.That(notSelected.Object.ToScrobble, Is.True);
        }
    }

    [Test]
    public void SelectedCount_Updates_WhenIsSelectedChanges()
    {
        var scrobble = CreateScrobbleMock();

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([scrobble.Object]);

        scrobble.Object.IsSelected = true;

        Assert.That(vm.SelectedCount, Is.EqualTo(1));
    }

    [Test]
    public void ReplacingScrobbles_RewiresEventsCorrectly()
    {
        var oldScrobble = CreateScrobbleMock();
        var newScrobble = CreateScrobbleMock();

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([oldScrobble.Object]);

        vm.SetScrobbles([newScrobble.Object]);

        newScrobble.Object.ToScrobble = true;

        Assert.That(vm.CanScrobble, Is.True);
    }
}
