
using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.CLI.DataAccess;
using RingBearer.CLI.Helpers;
using RingBearer.CLI.Logic;
using RingBearer.CLI.UI;
using RingBearer.Core.Language;
using RingBearer.Core.Manager;
using RingBearer.Core.Models;

namespace RingBearer.Tests.ConsoleCLITests;

public class UIDataAccessTests
{
    private readonly Mock<IUILogic> _logicMock = new();
    private readonly Mock<IPrintMessages> _printMessagesMock = new();
    private readonly Mock<IEntriesManager> _entriesManagerMock = new();
    private readonly Mock<ILanguageConfig> _languageConfigMock = new();
    private readonly Mock<IStringLocalizer> _localizerMock = new();

    private readonly UIDataAccess _uiDataAccess;

    public UIDataAccessTests()
    {
        _uiDataAccess = new UIDataAccess(
            _logicMock.Object,
            _printMessagesMock.Object,
            _entriesManagerMock.Object,
            _languageConfigMock.Object,
            _localizerMock.Object
        );
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_Success_ReturnsTrue()
    {
        // Arrange
        // Fix for CS0854: Replace the optional argument usage with explicit argument passing
        string masterKey = "testKey";
        _entriesManagerMock
            .Setup(m => m.LoginAsync(masterKey, ""))
            .Returns(Task.CompletedTask);
        //_entriesManagerMock
        //    .Setup(m => m.LoginAsync(masterKey))
        //    .Returns(Task.CompletedTask);

        // Act
        bool result = await _uiDataAccess.LoginAsync(masterKey);

        // Assert
        Assert.True(result);

        // Verify that the login method was called with the correct master key
        _printMessagesMock.Verify(m => m.PrintWriteLineMessage(It.IsAny<string>(), It.IsAny<ConsoleColor>()), Times.Never);
        // Verify that the PressAnyKeyToContinue method was not called
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Failure_ReturnsFalseAndPrintsError()
    {
        // Arrange
        string masterKey = "wrongKey";
        string errorMessage = "Login failed";

        _entriesManagerMock
            .Setup(m => m.LoginAsync(masterKey, ""))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        bool result = await _uiDataAccess.LoginAsync(masterKey);

        // Assert
        Assert.False(result);
        // Verify that the error message was printed
        _printMessagesMock.Verify(m => m.PrintWriteLineMessage(errorMessage, Constants.ErrorTextColor), Times.Once);
        // Verify that the PressAnyKeyToContinue method was called
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    #endregion

    #region ShowEntries Tests

    [Fact]
    public void ShowEntries_WithEntries_ShowsEntriesAndHeader()
    {
        // Arrange
        List<EntryModel> entries =
        [
            new () { Key = "key1", UserName = "user1", Password = "pass1", Notes = "note1" }
        ];

        _entriesManagerMock.Setup(m => m.GetEntries()).Returns(entries);
        _localizerMock
            .Setup(l => l["EntriesListTitle"])
            .Returns(new LocalizedString("EntriesListTitle", "Entries List"));

        // Act
        _uiDataAccess.ShowEntries();

        // Assert
        // Verify that the header message was printed
        _printMessagesMock.Verify(m => m.PrintWriteMessage(It.IsAny<string>(), It.IsAny<ConsoleColor>(), It.IsAny<bool>()), Times.Once);
        // Verify that the entries were printed
        _printMessagesMock.Verify(m => m.PrintTable(It.IsAny<List<Core.Models.EntryModel>>()), Times.Once);
        // Verify that the separator line was printed
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public void ShowEntries_NoEntries_ShowsNoEntriesMessage()
    {
        // Arrange
        List<EntryModel> entries = new();
        _entriesManagerMock.Setup(m => m.GetEntries()).Returns(entries);
        _localizerMock
            .Setup(l => l["NoEntries"])
            .Returns(new LocalizedString("NoEntries", "No entries available"));

        // Act
        _uiDataAccess.ShowEntries();

        // Assert
        // Verify that the header message was printed
        _printMessagesMock.Verify(
            m => m.PrintWriteMessage(
                It.Is<string>(msg => msg == "No entries available"),
                It.Is<ConsoleColor>(color => color == Constants.ErrorTextColor),
                It.IsAny<bool>()),
            Times.Once);
        // Verify that the entries were not printed
        _printMessagesMock.Verify(m => m.PrintTable(It.IsAny<List<EntryModel>>()), Times.Never);
        // Verify that the separator line was printed
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public void ShowEntries_ThrowsException_PrintsErrorMessage()
    {
        // Arrange
        string errorMessage = "Error fetching entries";
        _entriesManagerMock.Setup(m => m.GetEntries()).Throws(new Exception(errorMessage));

        // Act
        _uiDataAccess.ShowEntries();

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage(errorMessage), Times.Once);
    }

    #endregion

    #region GetEntry Test

    [Fact]
    public void GetEntry_ValidKey_ShowsEntry()
    {
        // Arrange
        string key = "myKey";
        EntryModel entry = new() { Key = key, UserName = "user", Password = "pass", Notes = "notes" };

        _entriesManagerMock.Setup(m => m.GetEntry(key)).Returns(entry);

        // Act
        _uiDataAccess.GetEntry([key]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintTable(It.Is<IEnumerable<EntryModel>>(l => l.Contains(entry))), Times.Once);
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public void GetEntry_NoArgs_PrintsError()
    {
        // Arrange
        SetupLocalizer("NoKeyOnGetError", "Key is required.");

        // Act
        _uiDataAccess.GetEntry([]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("Key is required."), Times.Once);
    }

    [Fact]
    public void GetEntry_EmptyKey_PrintsError()
    {
        // Arrange
        SetupLocalizer("KeyNullError", "Key cannot be null.");

        // Act
        _uiDataAccess.GetEntry([""]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("Key cannot be null."), Times.Once);
    }

    [Fact]
    public void GetEntry_EntryNotFound_PrintsNotFoundMessage()
    {
        string key = "missingKey";


        SetupLocalizer("EntryNotFound", "Entry not found.");

        // Act
        _uiDataAccess.GetEntry([key]);

        // Assert
        // Verify that the error message was printed
        _printMessagesMock.Verify(
           m => m.PrintWriteMessage(
               It.Is<string>(msg => msg == "Entry not found."),
               It.Is<ConsoleColor>(color => color == Constants.ErrorTextColor),
               It.IsAny<bool>()), Times.Once);
        // Verify that the PressAnyKeyToContinue method was called
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }


    #endregion

    #region FilterEntries Tests

    [Fact]
    public void FilterEntries_WithResults_PrintsTable()
    {
        // Arrange
        List<EntryModel> entries = new()
        { new() { Key = "email" } };
        _entriesManagerMock.Setup(m => m.FilterEntries("email")).Returns(entries);

        // Act
        _uiDataAccess.FilterEntries(["email"]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintTable(entries), Times.Once);
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public void FilterEntries_NoArgs_PrintsLocalizedError()
    {
        // Arrange
        SetupLocalizer("NoKeyOnGetError", "You must provide a key.");

        // Act
        _uiDataAccess.FilterEntries([]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("You must provide a key."), Times.Once);
    }

    [Fact]
    public void FilterEntries_EmptyKey_PrintsLocalizedError()
    {
        // Arrange
        SetupLocalizer("KeyNullError", "Key cannot be null.");

        // Act
        _uiDataAccess.FilterEntries([""]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("Key cannot be null."), Times.Once);
    }

    [Fact]
    public void FilterEntries_NoResults_PrintsNoEntriesMessage()
    {
        // Arrange
        _entriesManagerMock.Setup(m => m.FilterEntries("test")).Returns([]);
        SetupLocalizer("NoEntries", "No entries found.");

        // Act
        _uiDataAccess.FilterEntries(["test"]);

        // Assert
        // Verify that the error message was printed
        _printMessagesMock.Verify(m => m.PrintWriteMessage(
             It.Is<string>(msg => msg == "No entries found."),
             It.Is<ConsoleColor>(color => color == Constants.ErrorTextColor),
             It.IsAny<bool>()), Times.Once);

        // Verify that the PressAnyKeyToContinue method was called
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    #endregion

    #region AddEntryAsync Tests

    [Fact]
    public async Task AddEntryAsync_ValidEntry_AddsEntryAndPrintsSuccess()
    {
        // Arrange
        EntryModel newEntry = new() { Key = "email" };
        _logicMock.Setup(u => u.GetEntryModel(It.IsAny<string[]>())).Returns(newEntry);

        SetupLocalizer("EntryAdded", "Entry added successfully.");


        // Act
        await _uiDataAccess.AddEntryAsync(["-k", "email"]);

        // Assert
        // Verify that the entry was added
        _entriesManagerMock.Verify(e => e.AddEntryAsync(newEntry), Times.Once);
        // Verify that the success message was printed
        _printMessagesMock.Verify(m => m.PrintWriteLineMessage("Entry added successfully.", Constants.ResultTextColor), Times.Once);
        // Verify that the PressAnyKeyToContinue method was called
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public async Task AddEntryAsync_InvalidEntry_PrintsInvalidEntryMessage()
    {
        // Arrange
        _printMessagesMock.Setup(m => m.WaitForUserInput());
        SetupLocalizer("InvalidEntry", "Entry is invalid.");

        // Act
        await _uiDataAccess.AddEntryAsync(["-k"]);

        // Assert
        // Verify that the invalid entry message was printed
        _printMessagesMock.Verify(m => m.PrintWriteMessage(
           It.Is<string>(msg => msg == "Entry is invalid."),
           It.Is<ConsoleColor>(color => color == Constants.ErrorTextColor),
           It.IsAny<bool>()), Times.Once);

        // Verify that the WaitForUserInput method was called
        _entriesManagerMock.Verify(e => e.AddEntryAsync(It.IsAny<EntryModel>()), Times.Never);
    }

    [Fact]
    public async Task AddEntryAsync_ExceptionThrown_PrintsErrorMessage()
    {
        // Arrange
        EntryModel newEntry = new() { Key = "fail" };
        _logicMock.Setup(u => u.GetEntryModel(new[] { "-k", "fail" })).Returns(newEntry);

        _entriesManagerMock.Setup(e => e.AddEntryAsync(newEntry)).ThrowsAsync(new Exception("boom"));

        // Act
        await _uiDataAccess.AddEntryAsync(new[] { "-k", "fail" });

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("boom"), Times.Once);
    }


    #endregion

    #region UpdateEntryAsync Tests

    [Fact]
    public async Task UpdateEntryAsync_ValidEntry_CallsUpdateAndPrintsSuccess()
    {
        // Arrange
        EntryModel newEntry = new() { Key = "update-key" };
        _logicMock
            .Setup(u => u.GetEntryModel(It.IsAny<string[]>()))
            .Returns(newEntry);
        SetupLocalizer("EntryUpdated", "Entry updated!");

        // Act
        await _uiDataAccess.UpdateEntryAsync(["-k", "update-key"]);

        // Assert
        // Verify that the entry was updated
        _entriesManagerMock.Verify(e => e.UpdateEntryAsync(newEntry), Times.Once);
        // Verify that the success message was printed
        _printMessagesMock.Verify(m => m.PrintWriteLineMessage("Entry updated!", Constants.ResultTextColor), Times.Once);
        // Verify that the PressAnyKeyToContinue method was called
        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public async Task UpdateEntryAsync_InvalidEntry_PrintsInvalidEntryMessage()
    {
        // Arrange
        _logicMock.Setup(u => u.GetEntryModel(It.IsAny<string[]>())).Returns<EntryModel?>(null);
        SetupLocalizer("InvalidEntry", "Invalid entry.");
        _printMessagesMock.Setup(m => m.WaitForUserInput());

        // Act
        await _uiDataAccess.UpdateEntryAsync(["-k"]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintWriteMessage(
            It.Is<string>(msg => msg == "Invalid entry."),
            It.Is<ConsoleColor>(color => color == Constants.ErrorTextColor),
            It.IsAny<bool>()), Times.Once);

        _printMessagesMock.Verify(m => m.WaitForUserInput(), Times.Once);
        _entriesManagerMock.Verify(m => m.UpdateEntryAsync(It.IsAny<EntryModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEntryAsync_ExceptionThrown_PrintsErrorMessage()
    {
        // Arrange
        EntryModel entry = new() { Key = "error-key" };
        _logicMock.Setup(u => u.GetEntryModel(It.IsAny<string[]>())).Returns(entry);
        _entriesManagerMock.Setup(e => e.UpdateEntryAsync(entry)).ThrowsAsync(new Exception("boom"));

        // Act
        await _uiDataAccess.UpdateEntryAsync(["-k", "error-key"]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("boom"), Times.Once);
    }

    [Fact]
    public async Task UpdateEntryAsync_InvalidNullEntry_PrintsInvalidEntryMessage()
    {
        // Arrange
        _logicMock.Setup(u => u.GetEntryModel(It.IsAny<string[]>())).Returns((EntryModel?)null);
        SetupLocalizer("InvalidEntry", "Entry is invalid.");

        // Act
        await _uiDataAccess.UpdateEntryAsync(["-k"]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintWriteMessage(
            It.Is<string>(msg => msg == "Entry is invalid."),
            It.Is<ConsoleColor>(color => color == Constants.ErrorTextColor),
            It.IsAny<bool>()), Times.Once);

        _entriesManagerMock.Verify(m => m.UpdateEntryAsync(It.IsAny<EntryModel>()), Times.Never);
    }

    #endregion

    #region DeleteEntryAsync Tests

    [Fact]
    public async Task DeleteEntryAsync_ValidKey_PrintsSuccessMessage()
    {
        // Arrange
        string key = "my-key";
        SetupLocalizer("EntryDeleted", "Entry deleted successfully.");

        _entriesManagerMock
            .Setup(m => m.DeleteEntryAsync(key))
            .Returns(Task.CompletedTask);

        // Act
        await _uiDataAccess.DeleteEntryAsync([key]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintWriteLineMessage(
            "Entry deleted successfully.",
            Constants.ResultTextColor), Times.Once);

        _printMessagesMock.Verify(m => m.PressAnyKeyToContinue(), Times.Once);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteEntryAsync_InvalidKey_PrintsKeyNullError(string? keyArg)
    {
        // Arrange
        SetupLocalizer("KeyNullError", "Key cannot be null.");

        string[] commandArgs = keyArg is null ? Array.Empty<string>() : [keyArg];

        // Act
        await _uiDataAccess.DeleteEntryAsync(commandArgs);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("Key cannot be null."), Times.Once);
        _entriesManagerMock.Verify(m => m.DeleteEntryAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEntryAsync_ExceptionThrown_PrintsErrorMessage()
    {
        // Arrange
        string key = "fail-key";
        string exceptionMessage = "boom";

        _entriesManagerMock
            .Setup(m => m.DeleteEntryAsync(key))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        await _uiDataAccess.DeleteEntryAsync([key]);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage(exceptionMessage), Times.Once);
    }

    public static IEnumerable<object[]> InvalidKeyArgs =>
      [
          [Array.Empty<string>()],        // Sin argumentos
            [new[] { "" }]          // Argumento vacío
      ];

    [Theory]
    [MemberData(nameof(InvalidKeyArgs))]        // Argumento vacío
    public async Task DeleteEntryAsync_KeyMissing_PrintsKeyNullError(string[] commandArgs)
    {
        // Arrange
        SetupLocalizer("KeyNullError", "Key is required.");

        // Act
        await _uiDataAccess.DeleteEntryAsync(commandArgs);

        // Assert
        _printMessagesMock.Verify(m => m.PrintErrorMessage("Key is required."), Times.Once);
    }

    #endregion

    #region ChangeMasterKeyAsync Tests

    [Fact]
    public async Task ChangeMasterKeyAsync_ThrowsArgumentException_WhenCommandArgsIsEmpty()
    {
        // Arrange
        string[] commandArgs = Array.Empty<string>();
        SetupLocalizer("KeyNullError", "Key cannot be null or empty.");

        // Act
        await _uiDataAccess.ChangeMasterKeyAsync(commandArgs);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintErrorMessage("Key cannot be null or empty."), Times.Once);
    }

    [Fact]
    public async Task ChangeMasterKeyAsync_ThrowsArgumentException_WhenKeyIsNullOrEmpty()
    {
        // Arrange
        string[] commandArgs = new[] { "" };
        SetupLocalizer("KeyNullError", "Key cannot be null or empty.");

        // Act
        await _uiDataAccess.ChangeMasterKeyAsync(commandArgs);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintErrorMessage("Key cannot be null or empty."), Times.Once);
    }

    [Fact]
    public async Task ChangeMasterKeyAsync_CallsChangeMasterKeyAsync_WhenKeyIsValid()
    {
        // Arrange
        string[] commandArgs = new[] { "newMasterKey" };
        SetupLocalizer("MasterKeyChanged", "Master key changed successfully.");

        // Act
        await _uiDataAccess.ChangeMasterKeyAsync(commandArgs);

        // Assert
        _entriesManagerMock.Verify(em => em.ChangeMasterKeyAsync("newMasterKey"), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteLineMessage("Master key changed successfully.", Constants.ResultTextColor), Times.Once);
        _printMessagesMock.Verify(pm => pm.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public async Task ChangeMasterKeyAsync_HandlesException_WhenChangeMasterKeyFails()
    {
        // Arrange
        string[] commandArgs = new[] { "newMasterKey" };
        string exceptionMessage = "An error occurred.";
        _entriesManagerMock
            .Setup(em => em.ChangeMasterKeyAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        await _uiDataAccess.ChangeMasterKeyAsync(commandArgs);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintErrorMessage(exceptionMessage), Times.Once);
    }

    #endregion

    #region ChangeLanguage Tests

    [Fact]
    public void ChangeLanguage_ThrowsArgumentException_WhenCommandArgsIsEmpty()
    {
        // Arrange
        string[] commandArgs = Array.Empty<string>();
        SetupLocalizer("NoLanguageOnGetError", "Language code is required.");

        // Act
        _uiDataAccess.ChangeLanguage(commandArgs);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintErrorMessage("Language code is required."), Times.Once);
    }

    [Fact]
    public void ChangeLanguage_ThrowsArgumentException_WhenLanguageCodeIsNullOrEmpty()
    {
        // Arrange
        string[] commandArgs = new[] { "" };
        SetupLocalizer("NoLanguageOnGetError", "Language code is required.");

        // Act
        _uiDataAccess.ChangeLanguage(commandArgs);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintErrorMessage("Language code is required."), Times.Once);
    }

    [Fact]
    public void ChangeLanguage_CallsSetLanguage_WhenLanguageCodeIsValid()
    {
        // Arrange
        string[] commandArgs = new[] { "es" };
        SetupLocalizer("LanguageChanged", "Language changed successfully.");

        // Act
        _uiDataAccess.ChangeLanguage(commandArgs);

        // Assert
        _languageConfigMock.Verify(lc => lc.SetLanguage("es"), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteLineMessage("Language changed successfully.", Constants.ResultTextColor), Times.Once);
        _printMessagesMock.Verify(pm => pm.PressAnyKeyToContinue(), Times.Once);
    }

    [Fact]
    public void ChangeLanguage_HandlesException_WhenSetLanguageFails()
    {
        // Arrange
        string[] commandArgs = new[] { "es" };
        string exceptionMessage = "An error occurred.";
        _languageConfigMock
            .Setup(lc => lc.SetLanguage(It.IsAny<string>()))
            .Throws(new Exception(exceptionMessage));

        // Act
        _uiDataAccess.ChangeLanguage(commandArgs);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintErrorMessage(exceptionMessage), Times.Once);
    }

    #endregion

    #region Helpers

    private void SetupLocalizer(string key, string value)
    {
        _localizerMock
            .Setup(l => l[key])
            .Returns(new LocalizedString(key, value));
    }

    #endregion
}
