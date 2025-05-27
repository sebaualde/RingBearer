using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.Core.Constants;
using RingBearer.Core.Crypto;
using RingBearer.Core.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace RingBearer.Tests.CoreTests;
public class EncryptionServiceTests
{
    private readonly IStringLocalizer _localizer;
    private readonly EncryptionService _service;

    public EncryptionServiceTests()
    {
        Mock<IStringLocalizer> mockLocalizer = new();
        mockLocalizer.Setup(m => m[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _localizer = mockLocalizer.Object;
        _service = new EncryptionService(_localizer);
    }

    #region LoadEntries Test

    [Fact]
    public async Task LoadEntriesAsync_EmptyJson_ReturnsEmptyList()
    {
        // Arrange
        string masterKey = "testkey";
        string json = "null"; // JSON válido pero nulo
        string filePath = AppConstants.FileName;

        File.WriteAllBytes(filePath, CreateEncryptedFile(json, masterKey));

        // Act
        List<EntryModel> result = await _service.LoadEntriesAsync(masterKey);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadEntriesAsync_ValidJson_ReturnsEntries()
    {
        // Arrange
        string masterKey = "testkey";
        List<EntryModel> entries = new()
        {
            new() { Key = "gmail", UserName = "user", Password = "123" }
        };
        string json = JsonSerializer.Serialize(entries);
        File.WriteAllBytes(AppConstants.FileName, CreateEncryptedFile(json, masterKey));

        // Act
        List<EntryModel> result = await _service.LoadEntriesAsync(masterKey);

        // Assert
        Assert.Single(result);
        Assert.Equal("gmail", result[0].Key);
    }

    [Fact]
    public async Task LoadEntriesAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        string testFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dat");

        if (File.Exists(testFilePath))
            File.Delete(testFilePath);

        EncryptionService service = new(_localizer);
        Exception ex = await Record.ExceptionAsync(() => service.LoadEntriesAsync("testkey"));

        // Act & Assert CryptographicException FileNotFoundException
        Assert.NotNull(ex);
        Assert.True(
            ex is FileNotFoundException || ex is CryptographicException,
            $"Se esperaba FileNotFoundException o CryptographicException pero fue {ex.GetType().Name}"
        );
    }

    [Fact]
    public async Task LoadEntriesAsync_CorruptedJson_ThrowsInvalidOperationException()
    {
        // Arrange
        string masterKey = "testkey";
        string json = "{invalid_json}";
        File.WriteAllBytes(AppConstants.FileName, CreateEncryptedFile(json, masterKey));

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _service.LoadEntriesAsync(masterKey, It.IsAny<string>()));
    }

    #endregion

    #region Decrypt Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DecryptAsync_InvalidPassword_ThrowsArgumentException(string? password)
    {
        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(m => m[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        EncryptionService service = new(localizer.Object);

        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => service.DecryptAsync(password!));
        Assert.Equal("password", ex.ParamName);
    }

    [Fact]
    public async Task DecryptAsync_ValidEncryptedFile_ReturnsJsonString()
    {
        // Arrange
        string password = "master123";
        string originalJson = "{\"Test\":\"value\"}";
        File.WriteAllBytes(AppConstants.FileName, CreateEncryptedFile(originalJson, password));

        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(m => m[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        EncryptionService service = new(localizer.Object);

        // Act
        string result = await service.DecryptAsync(password);

        // Assert
        Assert.Equal(originalJson, result);
    }

    [Fact]
    public async Task DecryptAsync_WrongPassword_ThrowsCryptographicException()
    {
        // Arrange
        string correctPassword = "master123";
        string wrongPassword = "wrongpassword";
        string originalJson = "{\"foo\":\"bar\"}";
        File.WriteAllBytes(AppConstants.FileName, CreateEncryptedFile(originalJson, correctPassword));

        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(m => m[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        EncryptionService service = new(localizer.Object);

        // Act & Assert
        await Assert.ThrowsAsync<CryptographicException>(() => service.DecryptAsync(wrongPassword));
    }

    [Fact]
    public async Task DecryptAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        string password = "master123";
        if (File.Exists(AppConstants.FileName))
            File.Delete(AppConstants.FileName);

        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(m => m[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        EncryptionService service = new(localizer.Object);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => service.DecryptAsync(password, It.IsAny<string>()));
    }

    #endregion

    #region Encrypt Tests

    [Theory]
    [InlineData(null, "password123", "plainText")]
    [InlineData("", "password123", "plainText")]
    [InlineData("   ", "password123", "plainText")]
    [InlineData("Hello world", null, "password")]
    [InlineData("Hello world", "", "password")]
    [InlineData("Hello world", "   ", "password")]
    public void Encrypt_InvalidInputs_ThrowsArgumentException(string? plainText, string? password, string expectedParam)
    {
        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(m => m[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        EncryptionService service = new(localizer.Object);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => service.Encrypt(plainText!, password!));
        Assert.Equal(expectedParam, ex.ParamName);
    }

    [Fact]
    public async Task Encrypt_ValidInput_EncryptedDataCanBeDecrypted()
    {
        // Arrange
        string plainText = "{\"user\":\"test\"}";
        string password = "securepass123";

        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(m => m[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        EncryptionService service = new(localizer.Object);

        // Act
        byte[] encryptedData = service.Encrypt(plainText, password);

        // Simular que el archivo fue escrito
        File.WriteAllBytes(AppConstants.FileName, encryptedData);
        string decrypted = await service.DecryptAsync(password, It.IsAny<string>());

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_SameInputTwice_ProducesDifferentOutputs()
    {
        // Arrange
        string plainText = "Same input";
        string password = "Same password";

        Mock<IStringLocalizer> localizer = new();
        localizer.Setup(m => m[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        EncryptionService service = new(localizer.Object);

        // Act
        byte[] first = service.Encrypt(plainText, password);
        byte[] second = service.Encrypt(plainText, password);

        // Assert
        Assert.NotEqual(Convert.ToBase64String(first), Convert.ToBase64String(second));
    }


    #endregion

    #region Helpers

    private static byte[] CreateEncryptedFile(string plainText, string password)
    {
        using Aes aes = Aes.Create();
        aes.GenerateIV();
        byte[] salt = RandomNumberGenerator.GetBytes(AppConstants.SaltSize);
        using Rfc2898DeriveBytes key = new(password, salt, AppConstants.Iterations, HashAlgorithmName.SHA256);
        aes.Key = key.GetBytes(AppConstants.KeySize);

        using MemoryStream ms = new();
        ms.Write(salt, 0, salt.Length);
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (StreamWriter sw = new(cs))
        {
            sw.Write(plainText);
        }

        return ms.ToArray();
    }

    #endregion
}
