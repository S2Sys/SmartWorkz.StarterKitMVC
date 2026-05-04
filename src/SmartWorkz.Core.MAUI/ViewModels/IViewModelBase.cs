namespace SmartWorkz.Mobile;

using System.ComponentModel;

public interface IViewModelBase : INotifyPropertyChanged
{
    bool IsBusy { get; }
    bool IsNotBusy { get; }
    bool IsError { get; }
    string? ErrorMessage { get; }
    Task InitializeAsync();
}
