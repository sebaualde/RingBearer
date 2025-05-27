using CommunityToolkit.Mvvm.ComponentModel;

namespace RingBearer.Mobile.Models;

public partial class ChangeMasterKeyResult : ObservableObject
{
    private bool _isSuccess;
    public bool IsSuccess
    {
        get => _isSuccess;
        set => SetProperty(ref _isSuccess, value);
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _showMessage;
    public bool IsMessageVisible
    {
        get => _showMessage;
        set
        {
            SetProperty(ref _showMessage, value);
            OnPropertyChanged(nameof(IsMessageVisible));
        }
    }
}
