using RingBearer.Core.Models;
using RingBearer.Mobile.Models;

namespace RingBearer.Mobile.AppConfigurations;

public interface IUserConfigurations
{
    AppConfigMAUI Config { get; }

    /// <summary>
    /// Set the language selected by the user in the config file.
    /// </summary>
    /// <param name="language"> The language to set (en, es, pt, it).</param>
    void SetLanguage(string language);

    /// <summary>
    /// Toggle the app theme between light and dark.
    /// </summary>
    void ToggleAppTheme();

    /// <summary>
    /// Indicate if the app theme is dark or light.
    /// </summary>
    /// <returns></returns>
    bool IsDarkThemeSelected();
}
