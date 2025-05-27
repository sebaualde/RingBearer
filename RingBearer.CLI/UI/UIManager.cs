using Microsoft.Extensions.Localization;
using RingBearer.CLI.DataAccess;
using RingBearer.CLI.Helpers;
using RingBearer.CLI.Logic;
using RingBearer.Core.Language;
using System.Text;

namespace RingBearer.CLI.UI;
public class UIManager(
    IUIDataAccess dataAccess,
    IPrintMessages printMessages,
    IStringLocalizer localizer,
    IUILogic uILogic) : IUIManager
{
    private readonly IUIDataAccess _dataAccess = dataAccess;
    private readonly IPrintMessages _printMessages = printMessages;
    private readonly IStringLocalizer _localizer = localizer;
    private readonly IUILogic _uILogic = uILogic;

    private readonly string _headerApp = $@"
    ╔═══════════════════════════════════════════════════════════════════════════════════════════════════════════╗
    ║                                     Ring Bearer - Sursoft                                                 ║  
    {localizer["HeaderSubtitle"]}
    ╚═══════════════════════════════════════════════════════════════════════════════════════════════════════════╝";
    private readonly List<string> _commandsMemory = [];

    #region Login (2)

    public async Task<bool> ShowLoginAsync()
    {
        string masterKey;
        do
        {
            _printMessages.ClearConsole();

            _printMessages.PrintWriteLineMessage(_headerApp, Constants.SysTextColor);
            _printMessages.WriteLine();
            _printMessages.PrintWriteLineMessage(_localizer["ExitEscApp"], Constants.SysTextColor);
            _printMessages.WriteLine();
            _printMessages.PrintWriteMessage(_localizer["MasterKeyText"], Constants.DefaultTextColor);

            (masterKey, bool isEscPressed) = ReadPassword();

            if (isEscPressed) return true; // Salir del programa

            // hacemos el login
            bool loginSuccess = await _dataAccess.LoginAsync(masterKey);
            if (loginSuccess is false)
            {
                masterKey = String.Empty;
            }
        }
        while (string.IsNullOrEmpty(masterKey));

        return false;
    }

    public virtual (string password, bool isEscPressed) ReadPassword()
    {
        StringBuilder password = new();
        ConsoleKeyInfo keyInfo;

        // Si el usuario presiona Enter, se sale del bucle y se devuelve la contraseña
        while ((keyInfo = _printMessages.ReadKeyInterceptor(true)).Key != ConsoleKey.Enter)
        {
            // Si el usuario presiona Escape, se sale del bucle y se devuelve una cadena vacía
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                _printMessages.ShowExitAppMessage();
                return (string.Empty, true);
            }

            // Si el usuario presiona Backspace, se elimina el último carácter de la contraseña
            if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                _printMessages.Write("\b \b");
            }
            // Si el usuario presiona cualquier otra tecla, se añade el carácter a la contraseña
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                password.Append(keyInfo.KeyChar); // Añade el carácter a la contraseña
                _printMessages.Write("*"); // Muestra un asterisco en lugar del carácter real
            }
        }

        _printMessages.WriteLine();
        return (password.ToString(), false);
    }

    #endregion

    #region Menu (3)

    public async Task InitAppMenuAsync()
    {
        AppCommand command;

        do
        {
            _printMessages.ClearConsole();

            ShowMenuTitle();

            _printMessages.Write("\tcmd:\\>");
            (command, string[] commandArgs) = GetCommandAndArgs();

            switch (command)
            {
                case AppCommand.Get:
                    _dataAccess.GetEntry(commandArgs);
                    break;
                case AppCommand.List:
                    _dataAccess.ShowEntries();
                    break;
                case AppCommand.Filter:
                    _dataAccess.FilterEntries(commandArgs);
                    break;
                case AppCommand.Add:
                    await _dataAccess.AddEntryAsync(commandArgs);
                    break;
                case AppCommand.Update:
                    await _dataAccess.UpdateEntryAsync(commandArgs);
                    break;
                case AppCommand.Delete:
                    await _dataAccess.DeleteEntryAsync(commandArgs);
                    break;
                case AppCommand.ChangeMasterKey:
                    await _dataAccess.ChangeMasterKeyAsync(commandArgs);
                    break;
                case AppCommand.ChangeLang:
                    _dataAccess.ChangeLanguage(commandArgs);
                    break;
                case AppCommand.Exit:
                    _printMessages.ShowExitAppMessage();
                    break;
                case AppCommand.Help:
                    ShowHelpMenu();
                    break;
                default:
                    _printMessages.ShowUnrecognizedCommandError();
                    break;
            }

        } while (command != AppCommand.Exit);
    }

    private void ShowMenuTitle()
    {
        _printMessages.ClearConsole();

        _printMessages.PrintWriteLineMessage(_headerApp, Constants.SysTextColor);
        _printMessages.WriteLine();
        _printMessages.PrintWriteLineMessage(_localizer["ExitEscApp"], Constants.SysTextColor);
        _printMessages.PrintWriteLineMessage(_localizer["HelpTitle"], Constants.SysTextColor);
        _printMessages.WriteLine();
    }

    private void ShowHelpMenu()
    {
        _printMessages.ClearConsole();

        _printMessages.PrintWriteLineMessage(_headerApp, Constants.SysTextColor);

        _printMessages.PrintWriteLineMessage($@"
        {_localizer["HelpSubtitle"]}:",
        ConsoleColor.Cyan);

        int paddinRight = 40;

        ConsoleColor cmdColor = ConsoleColor.Yellow;

        //help
        _printMessages.PrintSeparatorLine();
        _printMessages.PrintWriteMessage(Constants.Help.PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteMessage($"{_localizer["HelpHelpExplanation "]}");
        //list
        _printMessages.PrintSeparatorLine();
        _printMessages.PrintWriteMessage(Constants.List.PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteMessage($"{_localizer["HelpLsExplanation"]}");
        //get
        _printMessages.PrintSeparatorLine();
        _printMessages.PrintWriteMessage($"{Constants.Get} <key>".PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteMessage($"{_localizer["HelpGetExplanation"]}");
        //filter
        _printMessages.PrintSeparatorLine();
        _printMessages.PrintWriteMessage($"{Constants.Filter} <word>".PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteLineMessage($"{_localizer["HelpFilterExplanation1"]}");
        _printMessages.PrintWriteMessage(" ".PadRight(paddinRight));
        _printMessages.PrintWriteMessage($"{_localizer["HelpFilterExplanation2"]}");
        //add / update
        _printMessages.PrintSeparatorLine();
        _printMessages.PrintWriteMessage($"{Constants.Add} / {Constants.Update} <key> [-u user] [-p pass] [-n notes]".PadRight(40), cmdColor);
        _printMessages.PrintWriteLineMessage(_localizer["AddOrUpdHelpText1"]);
        _printMessages.PrintWriteMessage(" ".PadRight(paddinRight));
        _printMessages.PrintWriteLineMessage(_localizer["AddOrUpdHelpText2"]);
        _printMessages.PrintWriteMessage(" ".PadRight(paddinRight));
        _printMessages.PrintWriteMessage(_localizer["AddOrUpdHelpText3"], ConsoleColor.DarkGray);
        //delete
        _printMessages.PrintSeparatorLine();
        _printMessages.PrintWriteMessage($"{Constants.Delete} <key>".PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteMessage($"{_localizer["HelpDelExplanation"]}");
        _printMessages.PrintSeparatorLine();
        //change mster key
        _printMessages.PrintWriteMessage($"{Constants.ChangeMasterKey} <newMasterKey>".PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteMessage($"{_localizer["HelpCmkExplanation"]}");
        _printMessages.PrintSeparatorLine();
        //Change Language
        _printMessages.PrintWriteMessage($"{Constants.ChangeLang} <language>".PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteMessage($"{_localizer["HelpLangExplanation"]} {AvailableLang.GetAvailableLanguages()}");
        _printMessages.PrintSeparatorLine();
        //exit
        _printMessages.PrintWriteMessage(Constants.Exit.PadRight(paddinRight), cmdColor);
        _printMessages.PrintWriteMessage($"{_localizer["HelpExitExplanation"]}");
        _printMessages.PrintSeparatorLine();

        _printMessages.PressAnyKeyToContinue();
    }

    #endregion

    #region Prompt section (4)

    private (AppCommand command, string[] commandArgs) GetCommandAndArgs()
    {
        string commandString = GetPromptValue() ?? string.Empty;

        return _uILogic.GetCommand(commandString);
    }

    private string GetPromptValue()
    {
        StringBuilder sbPrompt = new();

        ConsoleKeyInfo keyInfo;
        int memorySelect = _commandsMemory.Count - 1;
        int cursorPosition = 0;

        while ((keyInfo = _printMessages.ReadKeyInterceptor(true)).Key != ConsoleKey.Enter)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Escape:
                    return "exit";

                case ConsoleKey.Backspace:
                    if (cursorPosition > 0)
                    {
                        sbPrompt.Remove(sbPrompt.Length - 1, 1);
                        cursorPosition--;
                        _printMessages.Write("\b \b");
                    }
                    break;

                case ConsoleKey.UpArrow:
                    NavigateCommandHistory(ref sbPrompt, ref memorySelect, ref cursorPosition, -1);
                    break;

                case ConsoleKey.DownArrow:
                    NavigateCommandHistory(ref sbPrompt, ref memorySelect, ref cursorPosition, 1);
                    break;

                case ConsoleKey.LeftArrow:
                case ConsoleKey.RightArrow:
                    // No action for arrow keys
                    break;

                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        sbPrompt.Append(keyInfo.KeyChar);
                        cursorPosition++;
                        Console.Write(keyInfo.KeyChar);
                    }
                    break;
            }
        }

        _printMessages.WriteLine();
        string command = sbPrompt.ToString().Trim();
        if (!string.IsNullOrEmpty(command))
        {
            // Agregar el comando a la memoria
            if (!_commandsMemory.Contains(command))
            {
                _commandsMemory.Add(command.Trim());
            }
        }
        return command;
    }

    private void NavigateCommandHistory(ref StringBuilder sbPrompt, ref int memorySelect, ref int cursorPosition, int direction)
    {
        if (_commandsMemory.Count == 0) return;

        memorySelect = Math.Clamp(memorySelect + direction, 0, _commandsMemory.Count - 1);

        sbPrompt.Clear();
        sbPrompt.Append(_commandsMemory[memorySelect]);

        ClearCurrentLine();
        Console.Write("\tcmd:\\>" + _commandsMemory[memorySelect]);
        cursorPosition = sbPrompt.Length;
        Console.SetCursorPosition(14 + cursorPosition, Console.CursorTop);
    }

    private static void ClearCurrentLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, currentLineCursor);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    #endregion

    #region Messages (1)

    public void PrintErrorMessage(string message)
    {
        _printMessages.WriteLine();
        _printMessages.PrintWriteMessage(message, Constants.ErrorTextColor);
        _printMessages.WriteLine();
        _printMessages.PressAnyKeyToContinue();
    }

    #endregion
}
