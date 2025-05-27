using RingBearer.CLI.Helpers;
using RingBearer.Core.Models;

namespace RingBearer.CLI.Logic;
public interface IUILogic
{
    (AppCommand command, string[] commandArgs) GetCommand(string command);

    AppCommand GetCommandType(string command);

    EntryModel? GetEntryModel(string[] commandArgs);

    //EntryModel? GetEntryModelForUpdate(string[] commandArgs);
}
