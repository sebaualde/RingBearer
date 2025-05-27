using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.CLI.DataAccess;
using RingBearer.CLI.Helpers;
using RingBearer.CLI.Logic;
using RingBearer.CLI.UI;
using Xunit.Sdk;

namespace RingBearer.Tests.ConsoleCLITests;
public class UIManagerTests
{
    private readonly Mock<IUIDataAccess> _dataAccessMock;
    private readonly Mock<IPrintMessages> _printMessagesMock;
    private readonly Mock<IStringLocalizer> _localizerMock;
    private readonly Mock<IUILogic> _uILogicMock;
    private readonly UIManager _uiManager;

    public UIManagerTests()
    {
        _dataAccessMock = new Mock<IUIDataAccess>();
        _printMessagesMock = new Mock<IPrintMessages>();
        _localizerMock = new Mock<IStringLocalizer>();
        _uILogicMock = new Mock<IUILogic>();

        _uiManager = new UIManager(
            _dataAccessMock.Object,
            _printMessagesMock.Object,
            _localizerMock.Object,
            _uILogicMock.Object
        );
    }

    #region ShowLoginAsync Tests

    [Fact]
    public async Task ShowLoginAsync_ShouldReturnTrue_WhenEscapeIsPressed()
    {
        // Arrange
        SetupLoginMessage();

        // Simular que el usuario presiona Escape
        _printMessagesMock.SetupSequence(pm => pm.ReadKeyInterceptor(true))
            .Returns(new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false));

        // Act
        var result = await _uiManager.ShowLoginAsync();

        // Assert
        Assert.True(result);
        _printMessagesMock.Verify(pm => pm.ShowExitAppMessage(), Times.Once);
    }

    [Fact]
    public async Task ShowLoginAsync_ShouldReturnFalse_WhenLoginSucceeds()
    {
        // Arrange
        _dataAccessMock.Setup(da => da.LoginAsync("validPassword")).ReturnsAsync(true);

        var uiManagerMock = new Mock<UIManager>(
            _dataAccessMock.Object,
            _printMessagesMock.Object,
            _localizerMock.Object,
            _uILogicMock.Object
        )
        { CallBase = true };

        uiManagerMock.Setup(um => um.ReadPassword()).Returns(("validPassword", false));

        // Act
        var result = await uiManagerMock.Object.ShowLoginAsync();

        // Assert
        Assert.False(result);
        _dataAccessMock.Verify(da => da.LoginAsync("validPassword"), Times.Once);

    }

    [Fact]
    public async Task ShowLoginAsync_ShouldRetry_WhenLoginFails()
    {
        // Arrange
        SetupLoginMessage();
        _dataAccessMock.SetupSequence(da => da.LoginAsync(It.IsAny<string>()))
            .ReturnsAsync(false) // First attempt fails
            .ReturnsAsync(true); // Second attempt succeeds

        var uiManagerMock = new Mock<UIManager>(
            _dataAccessMock.Object,
            _printMessagesMock.Object,
            _localizerMock.Object,
            _uILogicMock.Object
        )
        { CallBase = true };

        uiManagerMock.SetupSequence(um => um.ReadPassword())
            .Returns(("invalidPassword", false)) // First attempt
            .Returns(("validPassword", false));  // Second attempt

        // Act
        var result = await uiManagerMock.Object.ShowLoginAsync();

        // Assert
        Assert.False(result);
        _dataAccessMock.Verify(da => da.LoginAsync("invalidPassword"), Times.Once);
        _dataAccessMock.Verify(da => da.LoginAsync("validPassword"), Times.Once);
    }

    private void SetupLoginMessage()
    {
        SetupLocalizer("ExitEscApp", "Exiting the application...");
        SetupLocalizer("MasterKeyText", "Enter your master key:");
        SetupLocalizer("HeaderSubtitle", "Welcome to Ring Bearer - Sursoft");

        _printMessagesMock.Setup(pm => pm.PrintWriteLineMessage(_localizerMock.Object["HeaderSubtitle"], Constants.SysTextColor));
        _printMessagesMock.Setup(pm => pm.WriteLine(""));
        _printMessagesMock.Setup(pm => pm.PrintWriteLineMessage(_localizerMock.Object["ExitEscApp"], Constants.SysTextColor));
        _printMessagesMock.Setup(pm => pm.WriteLine(""));
        _printMessagesMock.Setup(pm => pm.PrintWriteLineMessage(_localizerMock.Object["MasterKeyText"], Constants.DefaultTextColor));
        _printMessagesMock.Setup(pm => pm.ShowExitAppMessage());
    }

    #endregion

    [Fact]
    public void PrintErrorMessage_ShouldCallPrintWriteMessageAndPressAnyKeyToContinue()
    {
        // Arrange
        string errorMessage = "An error occurred.";
        ConsoleColor errorColor = Constants.ErrorTextColor;

        _printMessagesMock.Setup(pm => pm.PrintWriteMessage(errorMessage, errorColor, false));
        _printMessagesMock.Setup(pm => pm.PressAnyKeyToContinue());

        // Act
        _uiManager.PrintErrorMessage(errorMessage);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage(errorMessage, errorColor, false), Times.Once);
        _printMessagesMock.Verify(pm => pm.PressAnyKeyToContinue(), Times.Once);
    }

    #region Helpers

    private void SetupLocalizer(string key, string value)
    {
        _localizerMock
            .Setup(l => l[key])
            .Returns(new LocalizedString(key, value));
    }

    #endregion
}
