namespace RingBearer.Mobile.ViewModels.Interfaces;

public interface IShellViewModel
{
    #region UI Messages

    string CurrentPageTitle { get; set; }

    string AppSubtitle { get; }
    string EntriesPageTitle { get; }
    string LoginPageTitle { get; }
    string SettingsPageTitle { get; }
    string PasswordPageTitle { get; }
    string LanguagePageTitle { get; }
    string AboutPageTitle { get; }
    string DevelopedTitle { get; }
    string CloseText { get; }
    string ImportExportPageTitle { get; }
    string DeletePageTitle { get; }

    #endregion

    bool IsDarkTheme { get; set; }

    bool ShowLogoutButton { get; set; }

    void ChangeAppTheme();

    /// <summary>
    /// Changes the application language.
    /// </summary>
    /// <param name="culture"> The culture code to set the application language (en, es, pt, it, etc.) .</param>
    void ChangeLanguage(string culture);
}