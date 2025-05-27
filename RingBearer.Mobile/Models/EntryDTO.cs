using CommunityToolkit.Mvvm.ComponentModel;
using RingBearer.Core.Models;

namespace RingBearer.Mobile.Models;

public partial class EntryDTO : ObservableObject, IEntryModel
{
    private bool _isSelected;
    private bool _isDeleteMode = false;
    private string _selectImageSource = "";

    private string _key = string.Empty;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _notes = string.Empty;

    public string Key
    {
        get => _key;
        set => SetProperty(ref _key, value);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string SelectImageSource
    {
        get => _selectImageSource;
        set => SetProperty(ref _selectImageSource, value);
    }

    public bool IsDeleteMode
    {
        get => _isDeleteMode;
        set => SetProperty(ref _isDeleteMode, value);       
    }

}
