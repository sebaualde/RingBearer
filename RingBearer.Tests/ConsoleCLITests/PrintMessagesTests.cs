using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.CLI.UI;
using RingBearer.Core.Models;

namespace RingBearer.Tests.ConsoleCLITests;
public class PrintMessagesTests
{
    private readonly Mock<IStringLocalizer> _localizerMock;
    private readonly Mock<PrintMessages> _printMessagesMock;
    private readonly PrintMessages _printMessages;

    public PrintMessagesTests()
    {
        _localizerMock = new Mock<IStringLocalizer>();
        _printMessagesMock = new Mock<PrintMessages>(_localizerMock.Object) { CallBase = true };
        _printMessages = new PrintMessages(_localizerMock.Object);
    }

    [Fact]
    public void PressAnyKeyToContinue_ShouldPrintMessageAndWaitForKey()
    {
        // Arrange
        SetupLocalizer("PressKeyToContinue", "Press any key to continue...");

        _printMessagesMock.Setup(pm => pm.WriteLine(""));
        _printMessagesMock.Setup(pm => pm.PrintWriteMessage("Press any key to continue...", ConsoleColor.Cyan, false));
        _printMessagesMock.Setup(pm => pm.WaitForUserInput());

        // Act
        _printMessagesMock.Object.PressAnyKeyToContinue();

        // Assert
        _localizerMock.Verify(l => l["PressKeyToContinue"], Times.Once);
        _printMessagesMock.Verify(pm => pm.WriteLine(""), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage("Press any key to continue...", ConsoleColor.Cyan, false), Times.Once);
        _printMessagesMock.Verify(pm => pm.WaitForUserInput(), Times.Once);
    }

    [Fact]
    public void ShowExitAppMessage_ShouldPrintMessageAndWaitForKey()
    {
        // Arrange
        SetupLocalizer("ExitApp", "Exiting the application...");

        _printMessagesMock.Setup(pm => pm.WriteLine(""));
        _printMessagesMock.Setup(pm => pm.PrintWriteMessage("Exiting the application...", ConsoleColor.Cyan, false));
        _printMessagesMock.Setup(pm => pm.WaitForUserInput());

        // Act
        _printMessagesMock.Object.ShowExitAppMessage();

        // Assert
        _localizerMock.Verify(l => l["ExitApp"], Times.Once);
        _printMessagesMock.Verify(pm => pm.WriteLine(""), Times.Exactly(2)); // Two empty lines
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage("Exiting the application...", ConsoleColor.Cyan, false), Times.Once);
        _printMessagesMock.Verify(pm => pm.WaitForUserInput(), Times.Once);
    }

    [Fact]
    public void ShowUnrecognizedCommandError_ShouldPrintErrorMessageAndWaitForKey()
    {
        // Arrange
        SetupLocalizer("UnrecognizedCommand", "Unrecognized command.");

        _printMessagesMock.Setup(pm => pm.WriteLine(""));
        _printMessagesMock.Setup(pm => pm.PrintWriteLineMessage("Unrecognized command.", ConsoleColor.Red));
        _printMessagesMock.Setup(pm => pm.PressAnyKeyToContinue());

        // Act
        _printMessagesMock.Object.ShowUnrecognizedCommandError();

        // Assert
        _localizerMock.Verify(l => l["UnrecognizedCommand"], Times.Once);
        _printMessagesMock.Verify(pm => pm.WriteLine(""), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteLineMessage("Unrecognized command.", ConsoleColor.Red), Times.Once);
        _printMessagesMock.Verify(pm => pm.PressAnyKeyToContinue(), Times.Once);
    }


    [Theory]
    [InlineData("Test message", ConsoleColor.Green)]
    [InlineData("", ConsoleColor.White)]
    public void PrintWriteLineMessage_ShouldSetColorWriteMessageAndResetColor(string message, ConsoleColor color)
    {
        // Arrange
        _printMessagesMock.Setup(pm => pm.SetColor(color));
        _printMessagesMock.Setup(pm => pm.WriteLine($"\t{message}"));
        _printMessagesMock.Setup(pm => pm.ResetColor());

        // Act
        _printMessagesMock.Object.PrintWriteLineMessage(message, color);

        // Assert
        _printMessagesMock.Verify(pm => pm.SetColor(color), Times.Once);
        _printMessagesMock.Verify(pm => pm.WriteLine($"\t{message}"), Times.Once);
        _printMessagesMock.Verify(pm => pm.ResetColor(), Times.Once);
    }

    [Theory]
    [InlineData("Test message", ConsoleColor.Green, false, "\tTest message")]
    [InlineData("Another message", ConsoleColor.Yellow, true, "\tAnother message")]
    [InlineData("", ConsoleColor.White, false, "\t")]
    [InlineData("Message with padding", ConsoleColor.Cyan, false, "\tMessage with padding")]
    [InlineData("Message without padding", ConsoleColor.Red, true, "\tMessage without padding")]
    public void PrintWriteMessage_ShouldSetColorWriteMessageAndResetColor(string message, ConsoleColor color, bool clearPadding, string expectedOutput)
    {
        // Arrange
        _printMessagesMock.Setup(pm => pm.SetColor(color));
        _printMessagesMock.Setup(pm => pm.Write(expectedOutput));
        _printMessagesMock.Setup(pm => pm.ResetColor());

        // Act
        _printMessagesMock.Object.PrintWriteMessage(message, color, clearPadding);

        // Assert
        _printMessagesMock.Verify(pm => pm.SetColor(color), Times.Once);
        _printMessagesMock.Verify(pm => pm.Write(expectedOutput), Times.Once);
        _printMessagesMock.Verify(pm => pm.ResetColor(), Times.Once);
    }

    [Theory]
    [InlineData(true, ConsoleColor.Green)]
    [InlineData(false, ConsoleColor.Red)]
    public void PrintSeparatorLine_ShouldPrintWithCorrectFormat(bool isWriteLine, ConsoleColor color)
    {
        // Arrange
        _printMessagesMock.Setup(pm => pm.SetColor(color));

        // Act
        _printMessagesMock.Object.PrintSeparatorLine(isWriteLine, ConsoleColor.Green);

        // Assert
        _printMessagesMock.Verify(c => c.SetColor(ConsoleColor.Green), Times.Once);

        string lineString = @"
    ─────────────────────────────────────────────────────────────────────────────────────────────────────────────";

        if (isWriteLine)
            _printMessagesMock.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains(lineString))), Times.Once);
        else
            _printMessagesMock.Verify(c => c.Write(It.Is<string>(s => s.Contains(lineString))), Times.Once);

        _printMessagesMock.Verify(c => c.ResetColor(), Times.Once);
    }

    [Fact]
    public void PrintTableRow_ShouldPrintRowWithCorrectFormatting()
    {
        // Arrange
        var entry = new EntryModel
        {
            Key = "TestKey",
            UserName = "TestUser",
            Password = "TestPassword",
            Notes = "TestNotes"
        };

        // establish the expected behavior of the mock
        _printMessagesMock.Setup(pm => pm.PrintSeparatorLine(true, ConsoleColor.DarkGray));
        _printMessagesMock.Setup(pm => pm.PrintWriteMessage(entry.Key.PadRight(30), ConsoleColor.Yellow, false));
        _printMessagesMock.Setup(pm => pm.PrintWriteMessage(entry.UserName.PadRight(30), ConsoleColor.DarkCyan, true));
        _printMessagesMock.Setup(pm => pm.PrintWriteMessage(entry.Password.PadRight(30), ConsoleColor.DarkCyan, true));
        _printMessagesMock.Setup(pm => pm.PrintWriteMessage(entry.Notes, ConsoleColor.White, true));

        // Act
        _printMessagesMock.Object.PrintTableRow(entry);

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintSeparatorLine(true, ConsoleColor.DarkGray), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage(entry.Key.PadRight(30), ConsoleColor.Yellow, false), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage(entry.UserName.PadRight(30), ConsoleColor.DarkCyan, true), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage(entry.Password.PadRight(30), ConsoleColor.DarkCyan, true), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage(entry.Notes, ConsoleColor.White, true), Times.Once);
    }

    [Fact]
    public void PrintTableHeader_ShouldPrintHeaderWithCorrectFormatting()
    {
        // Arrange
        SetupLocalizer("HeaderKeyTable", "Key");
        SetupLocalizer("HeaderUserTable", "User");
        SetupLocalizer("HeaderPassTable", "Password");
        SetupLocalizer("HeaderNotesTable", "Notes");

        string expectedHeader = "Key".PadRight(30) + "User".PadRight(30) + "Password".PadRight(30) + "Notes";

        _printMessagesMock.Setup(pm => pm.PrintSeparatorLine(true, ConsoleColor.DarkGray));
        _printMessagesMock.Setup(pm => pm.PrintWriteMessage(expectedHeader, ConsoleColor.DarkGray, false));

        // Act
        _printMessagesMock.Object.PrintTableHeader();

        // Assert
        _printMessagesMock.Verify(pm => pm.PrintSeparatorLine(true, ConsoleColor.DarkGray), Times.Once);
        _printMessagesMock.Verify(pm => pm.PrintWriteMessage(expectedHeader, ConsoleColor.DarkGray, false), Times.Once);
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
