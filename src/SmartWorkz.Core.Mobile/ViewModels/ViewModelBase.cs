namespace SmartWorkz.Mobile;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class ViewModelBase : IViewModelBase
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isBusy;
    private string? _errorMessage;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !_isBusy;

    public bool IsError => _errorMessage is not null;

    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsError));
        }
    }

    protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;
        backingStore = value;
        OnPropertyChanged(propertyName);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected AsyncCommand CreateCommand(Func<Task> execute, Func<bool>? canExecute = null) =>
        new AsyncCommand(execute, canExecute, ex => ErrorMessage = ex.Message);

    protected async Task RunBusyAsync(Func<Task> action)
    {
        IsBusy = true;
        ErrorMessage = null;
        try { await action(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;
}
