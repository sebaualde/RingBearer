using RingBearer.Core.Models;
using RingBearer.Mobile.Constants;
using System.Globalization;

namespace RingBearer.Mobile.AppConfigurations;

public class UserConfigurations : IUserConfigurations
{

    #region Attributes

    private readonly AppConfigMAUI _config;

    #endregion

    public AppConfigMAUI Config => _config;

    public UserConfigurations()
    {
        _config = new()
        {
            IsFirstTime = Preferences.Get(PreferencesNames.IsFirstTime, true),
            Language = Preferences.Get(PreferencesNames.Language, "en"),
            Theme = Preferences.Get(PreferencesNames.AppTheme, 1),
        };

        SetUserPreferences();
    }

    private void SetUserPreferences()
    {
        SetUserTheme();
        SetUserLanguage();
    }

    #region App Language

    public void SetLanguage(string language)
    {
        Preferences.Set(PreferencesNames.Language, language);
        _config.Language = language;
        SetUserLanguage();
    }

    private void SetUserLanguage()
    {
        // Establecer la cultura de la aplicación según la configuración del usuario
        CultureInfo culture = new(_config.Language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    #endregion

    #region App Theme

    public void ToggleAppTheme()
    {
        // Alternar entre el tema claro y oscuro
        _config.Theme = IsDarkThemeSelected() ? 1 : 2; // 1 = Light, 2 = Dark, 0 = Unspecified

        Preferences.Set(PreferencesNames.AppTheme, _config.Theme);
        SetUserTheme();
    }

    public bool IsDarkThemeSelected()
    {
        // Determinar el tema actual y alternar
        int actualTheme = Preferences.Get(PreferencesNames.AppTheme, 1);

        return actualTheme == 2; // 2 = Dark, 1 = Light, 0 = Unspecified
    }

    private void SetUserTheme()
    {
        App.Current!.UserAppTheme = _config.Theme switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Light,
        };
    }

    #endregion
}
