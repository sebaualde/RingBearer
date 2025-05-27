using RingBearer.Core.Models;

namespace RingBearer.CLI.UI;
public interface IPrintMessages
{
    void PrintSeparatorLine(bool isWriteLine = true, ConsoleColor color = ConsoleColor.DarkGray);
    void PrintTable(IEnumerable<EntryModel> entries);
    void PrintTableHeader();
    void PrintTableRow(EntryModel entry);
    void PrintWriteLineMessage(string message = "", ConsoleColor color = ConsoleColor.White);
    void PrintWriteMessage(string message, ConsoleColor color = ConsoleColor.White, bool clearPadding = false);
    void PressAnyKeyToContinue();
    void PrintErrorMessage(string message);
    void ShowExitAppMessage();
    void ShowUnrecognizedCommandError();

    void WaitForUserInput();
    ConsoleKeyInfo ReadKeyInterceptor(bool intercept);
    void ClearConsole();
    void Write(string value = "");
    void WriteLine(string value = "");

}