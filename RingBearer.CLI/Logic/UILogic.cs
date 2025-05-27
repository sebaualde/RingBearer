using RingBearer.CLI.Helpers;
using RingBearer.Core.Constants;
using RingBearer.Core.Models;

namespace RingBearer.CLI.Logic;
public class UILogic : IUILogic
{

    public EntryModel? GetEntryModel(string[] commandArgs)
    {
        // Validar que haya al menos un argumento y que el primero no sea un flag
        if (commandArgs.Length < 1 || commandArgs[0].StartsWith("-"))
        {
            return null;
        }

        // Crear el modelo con la clave inicial
        EntryModel output = new() { Key = commandArgs[0] };

        // Diccionario para mapear los parámetros
        Dictionary<string, Action<string>> paramHandlers = new()
        {
            { AppConstants.UserParamPrefix, value => output.UserName = string.IsNullOrEmpty(value) ? AppConstants.ClearCommand : value },
            { AppConstants.PassParamPrefix, value => output.Password = string.IsNullOrEmpty(value) ? AppConstants.ClearCommand : value },
            { AppConstants.NotesParamPrefix, value => output.Notes = string.IsNullOrEmpty(value) ? AppConstants.ClearCommand : GetNotesString(commandArgs) }
        };

        // Procesar los argumentos restantes
        for (int i = 1; i < commandArgs.Length; i++)
        {
            if (paramHandlers.ContainsKey(commandArgs[i]))
            {
                // Obtener el valor del parámetro o un valor vacío si no hay más argumentos
                string value = (i + 1 < commandArgs.Length && !commandArgs[i + 1].StartsWith("-"))
                    ? commandArgs[i + 1]
                    : string.Empty;

                // Ejecutar el handler correspondiente
                paramHandlers[commandArgs[i]](value);

                // Saltar el siguiente argumento si fue procesado como valor
                if (!string.IsNullOrEmpty(value))
                {
                    i++;
                }
            }
        }

        return output;
    }

    private static string GetNotesString(string[] commandParams)
    {
        List<string> note = [];

        for (int i = 0; i < commandParams.Length; i++)
        {
            // si el argumento es -n, tomar todos los argumentos que siguen hasta el final o hasta que encuentre un -u o -p
            if (commandParams[i] == AppConstants.NotesParamPrefix)
            {
                // si lo siguiente a -n es -u o -p o no hay más argumentos, marcar como para limpiar el campo
                if (i + 1 >= commandParams.Length || commandParams[i + 1] == AppConstants.UserParamPrefix || commandParams[i + 1] == AppConstants.PassParamPrefix)
                {
                    return AppConstants.ClearCommand;
                }

                // validar que el siguiente argumento no sea -u o -p
                for (int j = i + 1; j < commandParams.Length; j++)
                {
                    // si el argumento es -u o -p, no tomarlo
                    if (commandParams[j] == AppConstants.UserParamPrefix || commandParams[j] == AppConstants.PassParamPrefix) break;

                    // si no es -u o -p, agregarlo a la lista de notas
                    note.Add(commandParams[j]);
                }
                break;
            }
        }

        return string.Join(" ", note);
    }

    public (AppCommand command, string[] commandArgs) GetCommand(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            return (AppCommand.Unknown, Array.Empty<string>());
        }
        string[] commandParts = command.Split(' ');
        AppCommand commandName = GetCommandType(commandParts[0].ToLower());
        string[] commandArgs = [.. commandParts.Skip(1)];
        return (commandName, commandArgs);
    }

    public AppCommand GetCommandType(string command) => command switch
    {
        Constants.Help => AppCommand.Help,
        Constants.Add => AppCommand.Add,
        Constants.Update => AppCommand.Update,
        Constants.Delete => AppCommand.Delete,
        Constants.List => AppCommand.List,
        Constants.Get => AppCommand.Get,
        Constants.Filter => AppCommand.Filter,
        Constants.ChangeMasterKey => AppCommand.ChangeMasterKey,
        Constants.ChangeLang => AppCommand.ChangeLang,
        Constants.Exit => AppCommand.Exit,
        _ => AppCommand.Unknown
    };

}
