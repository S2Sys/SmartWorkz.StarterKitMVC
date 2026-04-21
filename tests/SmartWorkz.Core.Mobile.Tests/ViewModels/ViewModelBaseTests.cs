namespace SmartWorkz.Mobile.Tests.ViewModels;

using System.ComponentModel;

public class ViewModelBaseTests
{
    private class TestViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Task CallInitializeAsync() => InitializeAsync();
        public Task CallRunBusyAsync(Func<Task> action) => RunBusyAsync(action);
    }

    [Fact]
    public void IsBusy_DefaultFalse()
    {
        var vm = new TestViewModel();
        Assert.False(vm.IsBusy);
        Assert.True(vm.IsNotBusy);
    }

    [Fact]
    public async Task RunBusyAsync_SetsBusyDuringExecution()
    {
        var vm = new TestViewModel();
        bool wasBusyDuringExec = false;

        await vm.CallRunBusyAsync(async () =>
        {
            wasBusyDuringExec = vm.IsBusy;
            await Task.CompletedTask;
        });

        Assert.True(wasBusyDuringExec);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task RunBusyAsync_OnException_ClearsIsBusyAndSetsError()
    {
        var vm = new TestViewModel();

        await vm.CallRunBusyAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Test error");
        });

        Assert.False(vm.IsBusy);
        Assert.True(vm.IsError);
        Assert.Equal("Test error", vm.ErrorMessage);
    }

    [Fact]
    public void SetProperty_RaisesPropertyChanged()
    {
        var vm = new TestViewModel();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Name = "Alice";

        Assert.Contains("Name", changed);
    }

    [Fact]
    public void SetProperty_SameValue_DoesNotRaisePropertyChanged()
    {
        var vm = new TestViewModel();
        vm.Name = "Alice";
        int count = 0;
        ((INotifyPropertyChanged)vm).PropertyChanged += (_, _) => count++;

        vm.Name = "Alice"; // same value

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task InitializeAsync_DoesNothingByDefault()
    {
        var vm = new TestViewModel();

        await vm.CallInitializeAsync();

        Assert.False(vm.IsError);
    }
}
