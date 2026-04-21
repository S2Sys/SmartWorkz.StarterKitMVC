namespace SmartWorkz.Mobile.Tests.ViewModels;

using System.Windows.Input;
using SmartWorkz.Mobile;

public class AsyncCommandTests
{
    [Fact]
    public async Task ExecuteAsync_InvokesSuppliedFunction()
    {
        bool executed = false;
        var cmd = new AsyncCommand(async () => { executed = true; await Task.CompletedTask; });
        await cmd.ExecuteAsync(null);
        Assert.True(executed);
    }

    [Fact]
    public void CanExecute_ReturnsTrueByDefault()
    {
        var cmd = new AsyncCommand(async () => await Task.CompletedTask);
        Assert.True(cmd.CanExecute(null));
    }

    [Fact]
    public async Task CanExecute_ReturnsFalseWhileExecuting()
    {
        var tcs = new TaskCompletionSource<bool>();
        var cmd = new AsyncCommand(async () => await tcs.Task);
        var executeTask = cmd.ExecuteAsync(null);
        Assert.False(cmd.CanExecute(null));
        tcs.SetResult(true);
        await executeTask;
        Assert.True(cmd.CanExecute(null));
    }

    [Fact]
    public async Task ExecuteAsync_CapturesException_DoesNotThrow()
    {
        Exception? captured = null;
        var cmd = new AsyncCommand(
            async () => { await Task.CompletedTask; throw new InvalidOperationException("oops"); },
            onException: ex => captured = ex);
        await cmd.ExecuteAsync(null);
        Assert.IsType<InvalidOperationException>(captured);
        Assert.Equal("oops", captured!.Message);
    }

    [Fact]
    public async Task Execute_ICommand_DispatchesToExecuteAsync()
    {
        bool executed = false;
        ICommand cmd = new AsyncCommand(async () => { executed = true; await Task.CompletedTask; });
        cmd.Execute(null);
        await Task.Delay(50);
        Assert.True(executed);
    }
}
