
using CommunityToolkit.Mvvm.ComponentModel;

namespace RingBearer.Core.Models;

public partial class AppConfigMAUI : ObservableObject
{
    private string _language = "en";
    private int _theme = 1; // 0=Unspecified, 1=Light, 2=Dark
    private bool _isFirstTime = true;

    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }
    public int Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }
    public bool IsFirstTime
    {
        get => _isFirstTime;
        set => SetProperty(ref _isFirstTime, value);
    }
}