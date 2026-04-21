namespace SmartWorkz.Mobile;

using System.Windows.Input;

public sealed class AsyncCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private readonly Action<Exception>? _onException;
    private bool _isBusy;

    public AsyncCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        Action<Exception>? onException = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute(), onException) { }

    public AsyncCommand(
        Func<object?, Task> execute,
        Func<object?, bool>? canExecute = null,
        Action<Exception>? onException = null)
    {
        _execute     = Guard.NotNull(execute, nameof(execute));
        _canExecute  = canExecute;
        _onException = onException;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) =>
        !_isBusy && (_canExecute?.Invoke(parameter) ?? true);

    public void Execute(object? parameter) =>
        _ = ExecuteAsync(parameter);

    public async Task ExecuteAsync(object? parameter = null)
    {
        if (!CanExecute(parameter)) return;
        _isBusy = true;
        RaiseCanExecuteChanged();
        try { await _execute(parameter); }
        catch (Exception ex) { if (_onException is not null) _onException(ex); else throw; }
        finally { _isBusy = false; RaiseCanExecuteChanged(); }
    }

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
