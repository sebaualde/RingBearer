namespace RingBearer.Core.Language;
public class AvailableLang
{
    public static bool IsLanguageSupported(string languageCode)
    {
        return Enum.TryParse<LanguagesEnum>(languageCode.ToLower(), true, out _);
    }

    public static string GetAvailableLanguages()
    {
        return string.Join(", ", Enum.GetNames<LanguagesEnum>()).ToLower();
    }
}


