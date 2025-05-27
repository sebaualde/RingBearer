using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.Core.Language;
using RingBearer.Core.Models;
using System.Text.Json;

namespace RingBearer.Tests.CoreTests;
public class LanguageConfigTests
{
    private LanguageConfig _languageConfig;
    private readonly Mock<IStringLocalizer> _localizerMock;
    private string _testFilePath;
    private static readonly JsonSerializerOptions _cachedJsonSerializerOptions = new () { WriteIndented = true };


    public LanguageConfigTests()
    {
        // Definimos una ruta de archivo temporal para los tests
        _testFilePath = Path.Combine(Path.GetTempPath(), "test_config.json");

        // Creamos el mock del localizador
        _localizerMock = new Mock<IStringLocalizer>();
        _localizerMock
            .Setup(l => l["LanguageNotSuported"])
            .Returns(new LocalizedString("LanguageNotSuported", "Language not supported:"));

        // Inicializamos LanguageConfig con la ruta del archivo de prueba y el localizador mock
        _languageConfig = new LanguageConfig(_localizerMock.Object, _testFilePath);
    }

    #region SetLanguage Tests

    [Fact]
    public void SetLanguage_ShouldUpdateLanguageAndSaveToFile()
    {
        // Arrange
        string newLanguage = "es"; // Nuevo código de idioma

        // Act
        _languageConfig.SetLanguage(newLanguage); // Cambia el idioma y guarda el archivo

        // Assert
        AppConfigCLI? config = JsonSerializer.Deserialize<AppConfigCLI>(File.ReadAllText(_testFilePath)); // Leemos el archivo
        Assert.Equal(newLanguage, config!.Language); // Verificamos que el idioma haya cambiado
    }

    [Fact]
    public void SetLanguage_UnsupportedLanguage_ThrowsArgumentException()
    {
        // Arrange
        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(l => l["LanguageNotSuported"]).Returns(new LocalizedString("LanguageNotSuported", "Language not supported:"));

        string invalidLanguage = "zz"; // Suponiendo que no está en la lista de soportados

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _languageConfig.SetLanguage(invalidLanguage));
        Assert.Contains($"{_localizerMock.Object["LanguageNotSuported"]} {AvailableLang.GetAvailableLanguages()})", exception.Message);
    }

    #endregion

    #region LoadOrCreate

    [Fact]
    public void SetLanguage_FileDoesNotExist_CreatesFileWithDefaultLanguage()
    {
        // Arrange: Asegurarse de que el archivo no exista
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }

        // Act: Llamamos a SetLanguage para que cree el archivo
        _languageConfig.SetLanguage("es");

        // Assert: Verificamos que el archivo fue creado y contiene el valor predeterminado
        Assert.True(File.Exists(_testFilePath));
        string json = File.ReadAllText(_testFilePath);
        AppConfigCLI? config = JsonSerializer.Deserialize<AppConfigCLI>(json);
        Assert.Equal("es", config?.Language); // "es" es el lenguaje que hemos configurado
    }

    [Fact]
    public void SetLanguage_FileExists_SavesNewLanguage()
    {
        // Arrange: Escribimos un archivo de configuración inicial con un lenguaje: por default en inglés
        AppConfigCLI initialConfig = new();
        string json = JsonSerializer.Serialize(initialConfig, _cachedJsonSerializerOptions);
        File.WriteAllText(_testFilePath, json);

        // Act: Cambiamos el idioma a "de"
        _languageConfig.SetLanguage(LanguagesEnum.Es.ToString());

        // Assert: Verificamos que el archivo se haya actualizado con el nuevo idioma
        json = File.ReadAllText(_testFilePath);
        AppConfigCLI? config = JsonSerializer.Deserialize<AppConfigCLI>(json);
        Assert.Equal(LanguagesEnum.Es.ToString(), config?.Language); // "de" es el idioma que hemos configurado
    }

    #endregion
}
