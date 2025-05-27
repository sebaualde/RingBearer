namespace RingBearer.Core.Language;

public interface ILanguageConfig
{
    string Language { get; }

    /// <summary>
    /// Set the language for the application.
    /// </summary>
    /// <param name="languageCode"> The language code to set.</param>
    void SetLanguage(string languageCode);
}