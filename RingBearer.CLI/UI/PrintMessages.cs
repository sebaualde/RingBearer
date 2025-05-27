using Microsoft.Extensions.Localization;
using RingBearer.Core.Models;

namespace RingBearer.CLI.UI;

public class PrintMessages(IStringLocalizer localizer) : IPrintMessages
{
    private readonly IStringLocalizer _localizer = localizer;

    public virtual void PressAnyKeyToContinue()
    {
        WriteLine();
        PrintWriteMessage(_localizer["PressKeyToContinue"], ConsoleColor.Cyan);
        WaitForUserInput();
    }

    public void ShowExitAppMessage()
    {
        WriteLine();
        WriteLine();
        PrintWriteMessage(_localizer["ExitApp"], ConsoleColor.Cyan);
        WaitForUserInput();
    }

    public void ShowUnrecognizedCommandError()
    {
        WriteLine();
        PrintWriteLineMessage(_localizer["UnrecognizedCommand"], ConsoleColor.Red);
        PressAnyKeyToContinue();
    }

    public virtual void PrintWriteLineMessage(string message = "", ConsoleColor color = ConsoleColor.White)
    {
        SetColor(color);
        WriteLine(($"\t{message}"));
        ResetColor();
    }

    public virtual void PrintWriteMessage(string message, ConsoleColor color = ConsoleColor.White, bool clearPadding = false)
    {
        SetColor(color);

        if (clearPadding)
        {
            Write(($"\t{message}"));
        }
        else
        {
            Write(($"\t{message}"));
        }

        ResetColor();
    }

    public virtual void PrintSeparatorLine(bool isWriteLine = true, ConsoleColor color = ConsoleColor.DarkGray)
    {
        SetColor(color);
        if (isWriteLine)
        {
            WriteLine(@"
    ─────────────────────────────────────────────────────────────────────────────────────────────────────────────");
        }
        else
        {
            Write(@"
    ─────────────────────────────────────────────────────────────────────────────────────────────────────────────");
        }
        ResetColor();
    }

    public void PrintTable(IEnumerable<EntryModel> entries)
    {
        PrintTableHeader();
        foreach (EntryModel entry in entries)
        {
            PrintTableRow(entry);
        }
        PrintSeparatorLine();
    }

    public void PrintTableHeader()
    {
        PrintSeparatorLine();
        PrintWriteMessage($"{_localizer["HeaderKeyTable"].Value.PadRight(30)}{_localizer["HeaderUserTable"].Value.PadRight(30)}{_localizer["HeaderPassTable"].Value.PadRight(30)}{_localizer["HeaderNotesTable"].Value}", ConsoleColor.DarkGray);
    }

    public void PrintTableRow(EntryModel entry)
    {
        PrintSeparatorLine();

        PrintWriteMessage($"{entry.Key}".PadRight(30), ConsoleColor.Yellow);
        PrintWriteMessage($"{entry.UserName}".PadRight(30), ConsoleColor.DarkCyan, true);
        PrintWriteMessage($"{entry.Password}".PadRight(30), ConsoleColor.DarkCyan, true);
        PrintWriteMessage($"{entry.Notes}", ConsoleColor.White, true);
    }

    public void PrintErrorMessage(string message)
    {
        WriteLine();
        PrintWriteMessage(message, ConsoleColor.Red);
        WriteLine();
        PressAnyKeyToContinue();
    }

    #region Console Helpers


    public virtual void WaitForUserInput() => Console.ReadKey();

    public virtual ConsoleKeyInfo ReadKeyInterceptor(bool intercept) => Console.ReadKey(intercept);

    public virtual void Write(string value = "") => Console.Write(value);

    public virtual void WriteLine(string value = "") => Console.WriteLine(value);

    public virtual void ClearConsole() => Console.Clear();

    public virtual void ResetColor() => Console.ResetColor();

    public virtual void SetColor(ConsoleColor color) => Console.ForegroundColor = color;

    #endregion

}
