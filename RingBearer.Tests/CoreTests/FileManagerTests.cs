using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.Core.Constants;
using RingBearer.Core.Crypto;
using RingBearer.Core.Models;
using RingBearer.Core.Storage;
using System.Security.Cryptography;

namespace RingBearer.Tests.CoreTests;
public class FileManagerTests
{
    private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
    private readonly Mock<IStringLocalizer> _localizerMock = new();
    private readonly FileManager _fileManager;

    public FileManagerTests()
    {
        _fileManager = new FileManager(_encryptionServiceMock.Object, _localizerMock.Object);
    }

    #region Load Tests

    [Fact]
    public async Task LoadAsync_ThrowsArgumentException_WhenMasterKeyIsNull()
    {
        // Arrange
        string expectedMessage = "MasterKeyNullError";
        _localizerMock.Setup(l => l["MasterKeyNullError"]).Returns(new LocalizedString("MasterKeyNullError", expectedMessage));

        // Act & Assert
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => _fileManager.LoadAsync(null!));
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task LoadAsync_CreatesFileAndReturnsEmptyList_WhenFileDoesNotExist()
    {
        // Arrange
        const string masterKey = "test-key";
        if (File.Exists(AppConstants.FileName))
            File.Delete(AppConstants.FileName);

        _encryptionServiceMock
            .Setup(e => e.Encrypt(It.IsAny<string>(), masterKey))
            .Returns([1, 2, 3]);

        // Act
        List<EntryModel> result = await _fileManager.LoadAsync(masterKey, AppConstants.FileName);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.True(File.Exists(AppConstants.FileName));
    }

    [Fact]
    public async Task LoadAsync_ReturnsDecryptedEntries_WhenFileExists()
    {
        // Arrange
        const string masterKey = "valid-key";
        List<EntryModel> expectedEntries = [new() { Key = "Test" }];
        File.WriteAllBytes(AppConstants.FileName, [1, 2, 3]); // fake file presence

        _encryptionServiceMock
            .Setup(e => e.LoadEntriesAsync(masterKey, It.IsAny<string>()))
            .ReturnsAsync(expectedEntries);

        // Act
        List<EntryModel> result = await _fileManager.LoadAsync(masterKey, AppConstants.FileName);

        // Assert
        Assert.Equal(expectedEntries.Count, result.Count);
        Assert.Equal(expectedEntries[0].Key, result[0].Key);
    }

    [Fact]
    public async Task LoadAsync_ThrowsInvalidOperationException_WhenDecryptionFails()
    {
        // Arrange
        const string masterKey = "wrong-key";
        File.WriteAllBytes(AppConstants.FileName, [1, 2, 3]);

        _localizerMock.Setup(l => l["InvalidMasterKeyError"]).Returns(new LocalizedString("InvalidMasterKeyError", "Invalid key"));

        _encryptionServiceMock
            .Setup(e => e.LoadEntriesAsync(masterKey, It.IsAny<string>()))
            .ThrowsAsync(new CryptographicException());

        // Act & Assert
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _fileManager.LoadAsync(masterKey, AppConstants.FileName));
        Assert.Equal("Invalid key", ex.Message);
    }

    [Fact]
    public async Task LoadAsync_ThrowsInvalidOperationException_WhenGenericExceptionOccurs()
    {
        // Arrange
        const string masterKey = "any-key";
        File.WriteAllBytes(AppConstants.FileName, [1, 2, 3]);

        _localizerMock.Setup(l => l["LoadFileError"]).Returns(new LocalizedString("LoadFileError", "Failed to load"));

        _encryptionServiceMock
            .Setup(e => e.LoadEntriesAsync(masterKey, AppConstants.FileName))
            .ThrowsAsync(new Exception("boom"));

        // Act & Assert
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _fileManager.LoadAsync(masterKey, AppConstants.FileName));
        Assert.Equal("Failed to load", ex.Message);
        Assert.Equal("boom", ex.InnerException?.Message);
    }

    #endregion

    #region Save Tests

    [Fact]
    public async Task SaveAsync_ThrowsArgumentNullException_WhenEntriesIsNull()
    {
        // Arrange
        List<EntryModel>? entries = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _fileManager.SaveAsync(entries!, "key"));
    }

    [Fact]
    public async Task SaveAsync_ThrowsArgumentException_WhenMasterKeyIsNullOrEmpty()
    {
        // Arrange
        List<EntryModel> entries = new()
        { new() };
        _localizerMock.Setup(l => l["MasterKeyNullError"]).Returns(new LocalizedString("MasterKeyNullError", "Master key is required"));

        // Act & Assert
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => _fileManager.SaveAsync(entries, ""));
        Assert.Equal("Master key is required", ex.Message);
    }

    [Fact]
    public async Task SaveAsync_SavesEncryptedFile_WhenDataIsValid()
    {
        // Arrange
        List<EntryModel> entries = [new() { Key = "Test" }];
        const string masterKey = "secure";
        byte[] fakeEncrypted = [1, 2, 3];

        _encryptionServiceMock
            .Setup(s => s.Encrypt(It.IsAny<string>(), masterKey))
            .Returns(fakeEncrypted);

        // Act
        await _fileManager.SaveAsync(entries, masterKey, AppConstants.FileName);

        // Assert
        byte[] actual = await File.ReadAllBytesAsync(AppConstants.FileName);
        Assert.Equal(fakeEncrypted, actual);
    }

    [Fact]
    public async Task SaveAsync_ThrowsInvalidOperationException_WhenEncryptionFails()
    {
        // Arrange
        List<EntryModel> entries = new()
        { new() };
        const string masterKey = "secure";

        _localizerMock.Setup(l => l["SaveFileError"]).Returns(new LocalizedString("SaveFileError", "Could not save file"));

        _encryptionServiceMock
            .Setup(s => s.Encrypt(It.IsAny<string>(), masterKey))
            .Throws(new Exception("encryption error"));

        // Act & Assert
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _fileManager.SaveAsync(entries, masterKey));
        Assert.Equal("Could not save file", ex.Message);
        Assert.Equal("encryption error", ex.InnerException?.Message);
    }

    #endregion
}
