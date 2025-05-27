using Microsoft.Extensions.Localization;
using RingBearer.Core.Constants;
using RingBearer.Core.Models;
using System.Text.Json;

namespace RingBearer.Core.Language;

public class LanguageConfig : ILanguageConfig
{
    private readonly AppConfigCLI _config;
    private readonly IStringLocalizer _localizer;
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _cachedJsonSerializerOptions = new() { WriteIndented = true };


    public string Language => _config.Language;

    public LanguageConfig(IStringLocalizer localizer, string? filePath = null)
    {
        _filePath = filePath ?? AppConstants.AppConfigsFilePath;
        _localizer = localizer;
        _config = LoadOrCreate();
    }

    /// <summary>
    /// Set the language for the application.
    /// </summary>
    /// <param name="languageCode"> The language code to set.</param>
    public void SetLanguage(string languageCode)
    {

        if (!AvailableLang.IsLanguageSupported(languageCode))
        {
            throw new ArgumentException($"{_localizer["LanguageNotSuported"]} {AvailableLang.GetAvailableLanguages()})");
        }

        _config.Language = languageCode;
        Save();
    }

    /// <summary>
    /// Load the configuration from the file or create a new one if it doesn't exist.
    /// </summary>
    /// <returns> The loaded or newly created AppConfig object.</returns>
    private AppConfigCLI LoadOrCreate()
    {
        if (File.Exists(_filePath)) // Use the instance field _filePath
        {
            string json = File.ReadAllText(_filePath); // Use the instance field _filePath
            return JsonSerializer.Deserialize<AppConfigCLI>(json) ?? new AppConfigCLI();
        }
        return new AppConfigCLI();
    }

    /// <summary>
    /// Save the current configuration to the file.
    /// </summary>
    private void Save()
    {
        //hacer comprobacion de que el lenguaje seleccionado existe

        string json = JsonSerializer.Serialize(_config, _cachedJsonSerializerOptions);
        File.WriteAllText(_filePath, json);
    }
}
