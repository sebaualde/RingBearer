using Microsoft.Extensions.Localization;
using RingBearer.CLI.Helpers;
using RingBearer.CLI.Logic;
using RingBearer.CLI.UI;
using RingBearer.Core.Constants;
using RingBearer.Core.Language;
using RingBearer.Core.Manager;
using RingBearer.Core.Models;

namespace RingBearer.CLI.DataAccess;

public class UIDataAccess(
    IUILogic uILogic,
    IPrintMessages printMessages,
    IEntriesManager entriesManager,
    ILanguageConfig languageConfig,
    IStringLocalizer localizer) : IUIDataAccess
{
    private readonly IUILogic _uILogic = uILogic;
    private readonly IPrintMessages _printMessages = printMessages;
    private readonly IEntriesManager _entriesManager = entriesManager;
    private readonly ILanguageConfig _languageConfig = languageConfig;
    private readonly IStringLocalizer _localizer = localizer;

    public async Task<bool> LoginAsync(string masterKey)
    {

        try
        {
            await _entriesManager.LoginAsync(masterKey);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            _printMessages.PrintWriteLineMessage(ex.Message, Constants.ErrorTextColor);
            _printMessages.PressAnyKeyToContinue();
            return false;
        }
    }

    public void ShowEntries()
    {
        try
        {
            IEnumerable<EntryModel> entries = _entriesManager.GetEntries();
            if (entries.Any())
            {
                string _headerApp = $@"
                                ╔─────────────────────────────────────────────────╗
                                {_localizer["EntriesListTitle"]}
                                ╚─────────────────────────────────────────────────╝";
                _printMessages.PrintWriteMessage(_headerApp, ConsoleColor.DarkYellow);

                Console.WriteLine();
                _printMessages.PrintTable(entries);
            }
            else
            {
                Console.WriteLine();
                _printMessages.PrintWriteMessage(_localizer["NoEntries"], Constants.ErrorTextColor);
            }

            Console.WriteLine();
            _printMessages.PressAnyKeyToContinue();
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

    public void GetEntry(string[] commandArgs)
    {
        try
        {
            if (commandArgs.Length < 1)
                throw new ArgumentException(_localizer["NoKeyOnGetError"]);

            if (string.IsNullOrEmpty(commandArgs[0]))
                throw new ArgumentException(_localizer["KeyNullError"]);


            string key = commandArgs[0];
            EntryModel entry = _entriesManager.GetEntry(key);
            if (entry == null)
            {
                _printMessages.WriteLine("");
                _printMessages.PrintWriteMessage(_localizer["EntryNotFound"], Constants.ErrorTextColor);
                _printMessages.PressAnyKeyToContinue();
            }
            else
            {
                _printMessages.PrintTable([entry]);
                _printMessages.PressAnyKeyToContinue();
            }
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

    public void FilterEntries(string[] commandArgs)
    {
        try
        {
            if (commandArgs.Length < 1)
                throw new ArgumentException(_localizer["NoKeyOnGetError"]);

            if (string.IsNullOrEmpty(commandArgs[0]))
                throw new ArgumentException(_localizer["KeyNullError"]);

            string key = commandArgs[0];
            IEnumerable<EntryModel> entries = _entriesManager.FilterEntries(key);
            if (entries.Any())
            {
                _printMessages.PrintTable(entries);
            }
            else
            {
                Console.WriteLine();
                _printMessages.PrintWriteMessage(_localizer["NoEntries"], Constants.ErrorTextColor);
            }
            _printMessages.PressAnyKeyToContinue();
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

    public async Task AddEntryAsync(string[] commandArgs)
    {
        try
        {
            EntryModel? newEntry = _uILogic.GetEntryModel(commandArgs);
            if (newEntry == null)
            {
                Console.WriteLine();
                _printMessages.PrintWriteMessage(_localizer["InvalidEntry"], Constants.ErrorTextColor);
                _printMessages.WaitForUserInput();
                return;
            }

            await _entriesManager.AddEntryAsync(newEntry);

            Console.WriteLine();
            _printMessages.PrintWriteLineMessage(_localizer["EntryAdded"], Constants.ResultTextColor);
            _printMessages.PressAnyKeyToContinue();
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

    public async Task UpdateEntryAsync(string[] commandArgs)
    {
        try
        {
            Core.Models.EntryModel? newEntry = _uILogic.GetEntryModel(commandArgs);
            if (newEntry == null)
            {
                Console.WriteLine();
                _printMessages.PrintWriteMessage(_localizer["InvalidEntry"], Constants.ErrorTextColor);
                _printMessages.WaitForUserInput();
                return;
            }
            await _entriesManager.UpdateEntryAsync(newEntry);
            Console.WriteLine();
            _printMessages.PrintWriteLineMessage(_localizer["EntryUpdated"], Constants.ResultTextColor);
            _printMessages.PressAnyKeyToContinue();
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

    public async Task DeleteEntryAsync(string[] commandArgs)
    {
        try
        {

            if (commandArgs.Length < 1 || string.IsNullOrEmpty(commandArgs[0]))
                throw new ArgumentException(_localizer["KeyNullError"]);

            string key = commandArgs[0];
            await _entriesManager.DeleteEntryAsync(key);
            Console.WriteLine();
            _printMessages.PrintWriteLineMessage(_localizer["EntryDeleted"], Constants.ResultTextColor);
            _printMessages.PressAnyKeyToContinue();
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

    public async Task ChangeMasterKeyAsync(string[] commandArgs)
    {
        try
        {
            if (commandArgs.Length < 1 || string.IsNullOrEmpty(commandArgs[0]))
                throw new ArgumentException(_localizer["KeyNullError"]);

            string key = commandArgs[0];
            await _entriesManager.ChangeMasterKeyAsync(key);
            Console.WriteLine();
            _printMessages.PrintWriteLineMessage(_localizer["MasterKeyChanged"], Constants.ResultTextColor);
            _printMessages.PressAnyKeyToContinue();
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

    public void ChangeLanguage(string[] commandArgs)
    {
        try
        {
            if (commandArgs.Length < 1 || string.IsNullOrEmpty(commandArgs[0]))
                throw new ArgumentException(_localizer["NoLanguageOnGetError"]);

            string language = commandArgs[0];
            _languageConfig.SetLanguage(language);

            Console.WriteLine();
            _printMessages.PrintWriteLineMessage(_localizer["LanguageChanged"], Constants.ResultTextColor);

            _printMessages.PressAnyKeyToContinue();
        }
        catch (Exception ex)
        {
            _printMessages.PrintErrorMessage(ex.Message);
        }
    }

}
