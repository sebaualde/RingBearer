using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using RingBearer.Mobile.AppConfigurations;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.ViewModels;

public partial class ShellViewModel : ObservableObject, IShellViewModel
{
    private readonly IStringLocalizer _localizer;
    private readonly IUserConfigurations _userConfigurations;

    #region UI Messages

    public string AppSubtitle => _localizer["HeaderSubtitleMAUI"];
    public string EntriesPageTitle => _localizer["EntriesListPageTitleMAUI"];
    public string LoginPageTitle => _localizer["LoginPageTitleMAUI"];
    public string SettingsPageTitle => _localizer["SettingsPageTitleMAUI"];
    public string PasswordPageTitle => _localizer["PasswordPageTitleMAUI"];
    public string LanguagePageTitle => _localizer["LanguagePageTitleMAUI"];
    public string AboutPageTitle => _localizer["AboutPageTitleMAUI"];
    public string DevelopedTitle => _localizer["DevelopedByTextMAUI"];
    public string CloseText => _localizer["CloseAppTextMAUI"];
    public string ImportExportPageTitle => _localizer["ImportExportPageTitleMAUI"];
    public string DeletePageTitle => _localizer["DeletePageTitleMAUI"];

    #endregion

    #region Attributes

    private bool _showLogoutButton = false;
    private string _currentPageTitle = string.Empty;
    private bool _isDarkTheme = false;

    #endregion

    #region Properties

    public bool ShowLogoutButton
    {
        get => _showLogoutButton;
        set => SetProperty(ref _showLogoutButton, value);
    }

    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        set => SetProperty(ref _currentPageTitle, value);
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set => SetProperty(ref _isDarkTheme, value);
    }

    #endregion

    public ShellViewModel(IStringLocalizer localizer, IUserConfigurations userConfigurations)
    {
        _localizer = localizer;
        _userConfigurations = userConfigurations;
        IsDarkTheme = _userConfigurations.IsDarkThemeSelected();
    }

    public void ChangeAppTheme()
    {
        _userConfigurations.ToggleAppTheme();
        IsDarkTheme = _userConfigurations.IsDarkThemeSelected();
    }

    public void ChangeLanguage(string culture)
    {
        _userConfigurations.SetLanguage(culture);
    }
}