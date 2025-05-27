using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.Core.Constants;
using RingBearer.Core.Manager;
using RingBearer.Core.Models;
using RingBearer.Core.Storage;
using System.Reflection;

namespace RingBearer.Tests.CoreTests;

public class EntriesManagerTests
{
    #region Login Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LoginAsync_ShouldThrowArgumentException_WhenMasterKeyIsNullOrEmpty(string masterKey)
    {
        // Arrange
        // Mock del IFileManager
        Mock<IFileManager> _fileManagerMock = new();
        EntriesManager entriesManager = CreateManagerForLogin(_fileManagerMock);

        // Act
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => entriesManager.LoginAsync(masterKey));

        // Assert
        Assert.Equal("The master key cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowInvalidOperationException_WhenLoadReturnsNull()
    {
        // Arrange
        string key = "validKey";

        // Mock del IFileManager
        Mock<IFileManager> _fileManagerMock = new();
        _fileManagerMock.Setup(fm => fm.LoadAsync(key, It.IsAny<string>())).ReturnsAsync((List<EntryModel>?)null!);
        EntriesManager entriesManager = CreateManagerForLogin(_fileManagerMock);


        // Act & Assert
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => entriesManager.LoginAsync(key));
        Assert.Equal("The master key is invalid.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldSetMasterKeyAndEntries_WhenSuccess()
    {
        // Arrange
        string key = "validKey";
        List<EntryModel> entries = new()
        { new() { Key = "k", UserName = "u" } };

        // Simular que el método LoadAsync devuelve una lista de entradas
        Mock<IFileManager> _fileManagerMock = new();
        _fileManagerMock.Setup(fm => fm.LoadAsync(key, It.IsAny<string>())).ReturnsAsync(entries);

        // Crear una instancia del EntriesManager con el mock
        EntriesManager _entriesManager = CreateManagerForLogin(_fileManagerMock);

        // Act
        await _entriesManager.LoginAsync(key);

        // Assert: verificar que el masterKey y las entradas se establecieron correctamente
        _fileManagerMock.Verify(fm => fm.LoadAsync(key, It.IsAny<string>()), Times.Once);
    }


    #endregion

    #region GetEntries Tests

    [Fact]
    public async Task GetEntries_ReturnsListOfEntries()
    {
        // Arrange
        List<EntryModel> expectedEntries = new()
        {
            new() { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note 1" },
            new() { Key = "key2", UserName = "user2", Password = "pass2", Notes = "note 1" }
        };

        // Hacemos el "seteo" manual de la lista interna usando LoginAsync con mock
        Mock<IFileManager> _fileManagerMock = new();
        _fileManagerMock
            .Setup(fm => fm.LoadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedEntries);

        Mock<IStringLocalizer> _localizerMock = new();

        _localizerMock
            .Setup(l => l["NoEntries"])
            .Returns(new LocalizedString("NoEntries", "No entries available."));

        EntriesManager _entriesManager = new(_localizerMock.Object, _fileManagerMock.Object);

        // Act
        await _entriesManager.LoginAsync("dummy-key");
        List<EntryModel> result = [.. _entriesManager.GetEntries()];

        // Assert
        Assert.Equal(expectedEntries.Count, result.Count);
        Assert.Equal("key1", result[0].Key);
        Assert.Equal("user2", result[1].UserName);
    }

    #endregion

    #region GetEntry tests

    [Fact]
    public void GetEntry_WithValidKey_ReturnsCorrectEntry()
    {
        // Arrange
        EntryModel expected = new() { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note 1" };
        List<EntryModel> entries = new()
        { expected };
        EntriesManager manager = CreateManagerWithEntries(entries);

        // Act
        EntryModel result = manager.GetEntry("key1");

        // Assert
        Assert.Equal(expected.Key, result.Key);
        Assert.Equal(expected.UserName, result.UserName);
    }

    [Fact]
    public void GetEntry_WhenEntriesListIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        EntriesManager manager = CreateManagerWithEntries([]);

        // Act & Assert
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => manager.GetEntry("any"));
        Assert.Equal("No entries available.", ex.Message); // Usar mensaje mockeado del localizer
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetEntry_WithNullOrEmptyKey_ThrowsArgumentException(string? key)
    {
        // Arrange
        List<EntryModel> entries = new()
        { new() { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note 1" } };
        EntriesManager manager = CreateManagerWithEntries(entries);

        // Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() => manager.GetEntry(key));
        Assert.Equal("Key is null or empty.", ex.Message); // Mensaje mock
    }

    [Fact]
    public void GetEntry_WithInvalidKey_ThrowsKeyNotFoundException()
    {
        // Arrange
        List<EntryModel> entries = new()
        { new() { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note 1" } };
        EntriesManager manager = CreateManagerWithEntries(entries);

        // Act & Assert
        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => manager.GetEntry("key999"));
        Assert.Equal("Entry not found.", ex.Message); // Mock
    }


    #endregion

    #region FilterEntries tests

    [Theory]
    [InlineData("key1")]
    [InlineData("user1")]
    [InlineData("pass1")]
    [InlineData("note 1")]
    public void FilterEntries_WithKeywordInAnyField_ReturnsMatchingEntry(string keyword)
    {
        // Arrange
        List<EntryModel> entries = new()
        {
            new () { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note 1" },
            new () { Key = "key2", UserName = "user2", Password = "pass2", Notes = "note 2" }
        };

        Mock<IStringLocalizer> mockLocalizer = new();
        Mock<IFileManager> mockFileManager = new();
        EntriesManager entriesManager = new(mockLocalizer.Object, mockFileManager.Object);
        typeof(EntriesManager)
            .GetField("_fileEntries", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(entriesManager, entries);

        // Act
        List<EntryModel> result =[.. entriesManager.FilterEntries(keyword)];

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        Assert.Equal("key1", result[0].Key); // El resto de los campos son de key1 también
        Assert.Equal("user1", result[0].UserName);
        Assert.Equal("pass1", result[0].Password);
        Assert.Equal("note 1", result[0].Notes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FilterEntries_WithEmptyKeyword_ReturnsAllEntries(string keyword)
    {
        // Arrange
        var entries = new List<EntryModel>
        {
            new() { Key = "Key1", UserName = "User1", Password = "Pass1", Notes = "Note1" },
            new() { Key = "Key2", UserName = "User2", Password = "Pass2", Notes = "Note2" }
        };

        // Mock del IFileManager
        var _fileManagerMock = GetEntriesManager(entries, out _);

        // Act
        var result = _fileManagerMock.FilterEntries(keyword);

        // Assert
        Assert.Equal(entries.Count, result.Count());
        Assert.Contains(result, e => e.Key == "Key1");
        Assert.Contains(result, e => e.Key == "Key2");
    }

    #endregion

    #region AddEntry tests

    [Fact]
    public async Task AddEntryAsync_WithValidEntry_AddsEntryAndSavesFile()
    {
        // Arrange
        EntryModel entry = new()
        {
            Key = "key1",
            UserName = "user1",
            Password = "pass1",
            Notes = "note 1"
        };

        List<EntryModel> entries = new();
        Mock<IStringLocalizer> mockLocalizer = new();
        Mock<IFileManager> mockFileManager = new();

        EntriesManager entriesManager = new(mockLocalizer.Object, mockFileManager.Object);

        // Setear _entries y _masterKey
        typeof(EntriesManager).GetField("_fileEntries", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(entriesManager, entries);
        typeof(EntriesManager).GetField("_masterKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(entriesManager, "master");

        // Act
        await entriesManager.AddEntryAsync(entry);

        // Assert
        Assert.Single(entries);
        Assert.Equal("key1", entries[0].Key);
        mockFileManager.Verify(fm => fm.SaveAsync(It.IsAny<List<EntryModel>>(), "master", It.IsAny<string>()), Times.Once);

        // Verificar que la entrada se agregó a la lista
        EntryModel? addedEntry = entries.FirstOrDefault(e => e.Key == entry.Key);
        Assert.NotNull(addedEntry);
        Assert.Equal(entry.UserName, addedEntry.UserName);
        Assert.Equal(entry.Password, addedEntry.Password);
        Assert.Equal(entry.Notes, addedEntry.Notes);
    }

    [Fact]
    public async Task AddEntryAsync_WithValidEntry_AddsEntryAndSaves()
    {
        // Arrange
        List<EntryModel> entries = new();
        EntryModel newEntry = new() { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note 1" };
        EntriesManager manager = GetEntriesManager(entries, out Mock<IFileManager>? mockFileManager);

        // Act
        await manager.AddEntryAsync(newEntry);

        // Assert
        Assert.Single(entries);
        Assert.Equal("key1", entries[0].Key);
        mockFileManager.Verify(f => f.SaveAsync(entries, "master", It.IsAny<string>()), Times.Once);

        // Verificar que la entrada se agregó a la lista
        EntryModel? addedEntry = entries.FirstOrDefault(e => e.Key == newEntry.Key);
        Assert.NotNull(addedEntry);
        Assert.Equal(newEntry.UserName, addedEntry.UserName);
    }

    [Fact]
    public async Task AddEntryAsync_WithNullEntry_ThrowsArgumentNullException()
    {
        EntriesManager manager = GetEntriesManager(new List<EntryModel>(), out _);
        await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AddEntryAsync(null!));
    }

    [Fact]
    public async Task AddEntryAsync_WithEmptyKey_ThrowsArgumentException()
    {
        EntriesManager manager = GetEntriesManager(new List<EntryModel>(), out _);
        EntryModel entry = new() { Key = "", UserName = "user", Password = "pass", Notes = "note" };

        await Assert.ThrowsAsync<ArgumentException>(() => manager.AddEntryAsync(entry));
    }

    [Fact]
    public async Task AddEntryAsync_WithDuplicateKey_ThrowsInvalidOperationException()
    {
        List<EntryModel> entries = new()
        { new() { Key = "key1" } };
        EntriesManager manager = GetEntriesManager(entries, out _);
        EntryModel entry = new() { Key = "KEY1", UserName = "x", Password = "x", Notes = "x" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.AddEntryAsync(entry));
    }

    [Fact]
    public async Task AddEntryAsync_WithClearCommand_SetsFieldsToEmpty()
    {
        EntryModel entry = new()
        {
            Key = "key1",
            UserName = AppConstants.ClearCommand,
            Password = AppConstants.ClearCommand,
            Notes = AppConstants.ClearCommand
        };

        EntriesManager manager = GetEntriesManager(new List<EntryModel>(), out _);

        // Act
        await manager.AddEntryAsync(entry);

        // Assert
        List<EntryModel> entries = (List<EntryModel>)typeof(EntriesManager)
            .GetField("_fileEntries", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(manager)!;

        EntryModel saved = entries[0];
        Assert.Equal("", saved.UserName);
        Assert.Equal("", saved.Password);
        Assert.Equal("", saved.Notes);
    }

    #endregion

    #region UpdateEntry

    [Fact]
    public async Task UpdateEntryAsync_WithValidEntry_UpdatesEntry()
    {
        List<EntryModel> entries = new()
        {
            new() { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note1" }
        };
        EntryModel updatedEntry = new()
        {
            Key = "key1",
            UserName = "newUser",
            Password = "newPass",
            Notes = "newNote"
        };

        EntriesManager manager = GetEntriesManager(entries, out Mock<IFileManager>? mockFileManager);

        // Act
        await manager.UpdateEntryAsync(updatedEntry);

        // Assert

        // Verificar que la entrada se actualizó en la lista
        EntryModel? updated = entries.FirstOrDefault(e => e.Key == updatedEntry.Key);
        Assert.NotNull(updated);
        Assert.Equal(updatedEntry.UserName, updated.UserName);
        Assert.Equal(updatedEntry.Password, updated.Password);
        Assert.Equal(updatedEntry.Notes, updated.Notes);

        // Verificar que la entrada se actualizó en el archivo
        mockFileManager.Verify(fm => fm.SaveAsync(entries, "master", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEntryAsync_WithNullEntry_ThrowsArgumentNullException()
    {
        EntriesManager manager = GetEntriesManager(new List<EntryModel>(), out _);
        await Assert.ThrowsAsync<ArgumentNullException>(() => manager.UpdateEntryAsync(null!));
    }

    [Fact]
    public async Task UpdateEntryAsync_WithNonExistentKey_ThrowsKeyNotFoundException()
    {
        List<EntryModel> entries = new()
        { new() { Key = "key1" } };
        EntryModel updatedEntry = new() { Key = "notfound" };
        EntriesManager manager = GetEntriesManager(entries, out _);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => manager.UpdateEntryAsync(updatedEntry));
    }

    [Fact]
    public async Task UpdateEntryAsync_WithClearCommand_ClearsOptionalFields()
    {
        List<EntryModel> entries = new()
        {
            new() { Key = "key1", UserName = "originalUser", Password = "originalPass", Notes = "originalNote" }
        };
        EntryModel updatedEntry = new()
        {
            Key = "key1",
            UserName = AppConstants.ClearCommand,
            Password = AppConstants.ClearCommand,
            Notes = AppConstants.ClearCommand
        };

        EntriesManager manager = GetEntriesManager(entries, out _);
        await manager.UpdateEntryAsync(updatedEntry);

        EntryModel entry = entries[0];
        Assert.Equal("", entry.UserName);
        Assert.Equal("", entry.Password);
        Assert.Equal("", entry.Notes);
    }

    #endregion

    #region DeleteEntry Tests

    [Fact]
    public async Task DeleteEntryAsync_WithValidKey_RemovesEntry()
    {
        List<EntryModel> entries = new()
        {
            new() { Key = "key1" },
            new() { Key = "key2" }
        };

        EntriesManager manager = GetEntriesManager(entries, out Mock<IFileManager>? mockFileManager);

        // Act
        await manager.DeleteEntryAsync("key1");

        // Assert
        Assert.Single(entries);
        Assert.DoesNotContain(entries, e => e.Key.Equals("key1", StringComparison.OrdinalIgnoreCase));
        mockFileManager.Verify(fm => fm.SaveAsync(entries, "master", It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteEntryAsync_WithInvalidKey_ThrowsArgumentException(string? invalidKey)
    {
        EntriesManager manager = GetEntriesManager(new List<EntryModel>(), out _);
        await Assert.ThrowsAsync<ArgumentException>(() => manager.DeleteEntryAsync(invalidKey!));
    }

    [Fact]
    public async Task DeleteEntryAsync_KeyNotFound_ThrowsKeyNotFoundException()
    {
        List<EntryModel> entries = new()
        { new() { Key = "key1" } };
        EntriesManager manager = GetEntriesManager(entries, out _);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => manager.DeleteEntryAsync("nonexistent"));
    }

    #endregion

    #region ChangeMasterKey Tests

    [Fact]
    public async Task ChangeMasterKeyAsync_WithValidKey_UpdatesMasterKeyAndSavesFile()
    {
        EntriesManager manager = GetEntriesManagerForChangeMasterKey(out Mock<IFileManager>? mockFileManager);

        await manager.ChangeMasterKeyAsync("newkey123");

        string? updatedMasterKey = typeof(EntriesManager)
            .GetField("_masterKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(manager) as string;

        Assert.Equal("newkey123", updatedMasterKey);
        mockFileManager.Verify(fm => fm.SaveAsync(It.IsAny<List<EntryModel>>(), "newkey123", It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public async Task ChangeMasterKeyAsync_WithInvalidKey_ThrowsArgumentException(string? invalidKey)
    {
        EntriesManager manager = GetEntriesManagerForChangeMasterKey(out _);

        await Assert.ThrowsAsync<ArgumentException>(() => manager.ChangeMasterKeyAsync(invalidKey!));
    }

    #endregion

    #region Helpers

    private static EntriesManager CreateManagerForLogin(Mock<IFileManager> fileManagerMock)
    {
        // Mock del localizer
        Mock<IStringLocalizer> localizerMock = new();
        localizerMock.Setup(l => l["MasterKeyNullError"]).Returns(new LocalizedString("MasterKeyNullError", "The master key cannot be null or empty."));
        localizerMock.Setup(l => l["InvalidMasterKeyError"]).Returns(new LocalizedString("InvalidMasterKeyError", "The master key is invalid."));

        // Creamos una instancia del manager
        return new EntriesManager(localizerMock.Object, fileManagerMock.Object);
    }

    private static EntriesManager CreateManagerWithEntries(List<EntryModel> entries)
    {
        // Mock del localizer
        Mock<IStringLocalizer> localizerMock = new();
        localizerMock.Setup(l => l["NoEntries"]).Returns(new LocalizedString("NoEntries", "No entries available."));
        localizerMock.Setup(l => l["KeyNullError"]).Returns(new LocalizedString("KeyNullError", "Key is null or empty."));
        localizerMock.Setup(l => l["EntryNotFound"]).Returns(new LocalizedString("EntryNotFound", "Entry not found."));

        // Mock del fileManager que no se usa en este método, pero igual lo pasamos
        Mock<IFileManager> fileManagerMock = new();

        // Creamos una instancia del manager y seteamos las entradas manualmente
        EntriesManager manager = new(localizerMock.Object, fileManagerMock.Object);

        // Usamos reflexión para setear _entries directamente (porque es privado)
        typeof(EntriesManager)
            .GetField("_fileEntries", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(manager, entries);

        return manager;
    }

    private static EntriesManager GetEntriesManager(List<EntryModel> initialEntries, out Mock<IFileManager> mockFileManager)
    {
        Mock<IStringLocalizer> mockLocalizer = new();
        mockFileManager = new Mock<IFileManager>();
        mockFileManager.Setup(fm => fm.SaveAsync(It.IsAny<List<EntryModel>>(), It.IsAny<string>(), ""))
                       .Returns(Task.CompletedTask);

        EntriesManager manager = new(mockLocalizer.Object, mockFileManager.Object);
        typeof(EntriesManager)
            .GetField("_fileEntries", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(manager, initialEntries);
        typeof(EntriesManager)
            .GetField("_masterKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(manager, "master");
        return manager;
    }

    private static EntriesManager GetEntriesManagerForChangeMasterKey(out Mock<IFileManager> mockFileManager)
    {
        Mock<IStringLocalizer> mockLocalizer = new();
        mockFileManager = new Mock<IFileManager>();
        mockFileManager.Setup(fm => fm.SaveAsync(It.IsAny<List<EntryModel>>(), It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(Task.CompletedTask);

        EntriesManager manager = new(mockLocalizer.Object, mockFileManager.Object);
        typeof(EntriesManager)
            .GetField("_fileEntries", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(manager, new List<EntryModel>());
        typeof(EntriesManager)
            .GetField("_masterKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(manager, "oldkey");

        return manager;
    }

    #endregion
}
