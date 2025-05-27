using RingBearer.CLI.Helpers;
using RingBearer.CLI.Logic;
using RingBearer.Core.Constants;
using System.Reflection;

namespace RingBearer.Tests.ConsoleCLITests;
public class UILogicTests
{
    private readonly UILogic _logic = new();

    #region GetEntryModel Tests

    public static IEnumerable<object[]> InvalidArgsData =>
        [
        [Array.Empty<string>()],
        [ new[] { "-u", "user" } ]
    ];

    [Theory]
    [MemberData(nameof(InvalidArgsData))]
    public void GetEntryModel_InvalidArgs_ReturnsNull(string[] args)
    {
        Core.Models.EntryModel? result = _logic.GetEntryModel(args);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("myKey", AppConstants.ClearCommand, AppConstants.ClearCommand, AppConstants.ClearCommand)]
    [InlineData("key", "user", "pass", "some note")]
    public void GetEntryModel_ValidArgs_ReturnsExpectedModel(string key, string? user, string? pass, string? notes)
    {
        // Armamos el array de argumentos según los datos
        List<string> args = new() { key };

        if (user != null)
            args.AddRange(new[] { AppConstants.UserParamPrefix, user });

        if (pass != null)
            args.AddRange(new[] { AppConstants.PassParamPrefix, pass });

        if (notes != null)
            args.AddRange([AppConstants.NotesParamPrefix, .. notes.Split(" ")]);

        Core.Models.EntryModel? result = _logic.GetEntryModel(args.ToArray());

        Assert.NotNull(result);
        Assert.Equal(key, result!.Key);
        Assert.Equal(user ?? "", result.UserName);
        Assert.Equal(pass ?? "", result.Password);
        Assert.Equal(notes ?? "", result.Notes);
    }

    [Fact]
    public void GetEntryModel_MissingParamValues_UsesClear()
    {
        string[] args = new[] { "key", "-u", "-p", "-n" };
        Core.Models.EntryModel? result = _logic.GetEntryModel(args);

        Assert.NotNull(result);
        Assert.Equal(AppConstants.ClearCommand, result!.UserName);
        Assert.Equal(AppConstants.ClearCommand, result.Password);
        Assert.Equal(AppConstants.ClearCommand, result.Notes);
    }

    #endregion

    #region GetNotesString Tests

    private static MethodInfo GetNotesStringMethod => typeof(UILogic)
        .GetMethod("GetNotesString", BindingFlags.NonPublic | BindingFlags.Static)!;

    public static IEnumerable<object[]> NotesTestCases =>
        new List<object[]>
        {
            // Caso 1: notas simples
            new object[] { new[] { "-n", "esto", "es", "una", "nota" }, "esto es una nota" },

            // Caso 2: nota seguida de -u
            new object[] { new[] { "-n", "nota", "-u", "usuario" }, "nota" },

            // Caso 3: -n sin argumentos posteriores
            new object[] { new[] { "-n" }, AppConstants.ClearCommand },

            // Caso 4: -n seguido inmediatamente de -p
            new object[] { new[] { "-n", "-p" }, AppConstants.ClearCommand },

            // Caso 5: -n seguido inmediatamente de -u
            new object[] { new[] { "-n", "-u" }, AppConstants.ClearCommand },

            // Caso 6: sin parámetro -n
            new object[] { new[] { "algo", "-u", "usuario" }, "" }
        };

    [Theory]
    [MemberData(nameof(NotesTestCases))]
    public void GetNotesString_ParsesCorrectly(string[] args, string expected)
    {
        // Invocamos el método privado GetNotesString mediante reflection
        string actual = (string)GetNotesStringMethod.Invoke(null, [args])!;
        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetCommand Tests

    public static IEnumerable<object[]> CommandTestCases =>
        new List<object[]>
        {
        // Comando válido sin argumentos
        new object[] { "ls", AppCommand.List, Array.Empty<string>() },

        // Comando válido con un argumento
        new object[] { "get gmail", AppCommand.Get, new[] { "gmail" } },

        // Comando válido con múltiples argumentos
        new object[] { "add key1 -u user -p pass -n some notes", AppCommand.Add, new[] { "key1", "-u", "user", "-p", "pass", "-n", "some", "notes" } },

        new object[] { "del gmail", AppCommand.Delete, new[] { "gmail" } },
        new object[] { "upd gmail -p newpass", AppCommand.Update, new[] { "gmail", "-p", "newpass" } },
        new object[] { "ftr bank", AppCommand.Filter, new[] { "bank" } },
        new object[] { "cmk", AppCommand.ChangeMasterKey, Array.Empty<string>() },
        new object[] { "lang es", AppCommand.ChangeLang, new[] { "es" } },
        new object[] { "h", AppCommand.Help, Array.Empty<string>() },
        new object[] { "exit", AppCommand.Exit, Array.Empty<string>() },

        // Comando en mayúsculas
        new object[] { "GET github", AppCommand.Get, new[] { "github" } },

        // Comando vacío
        new object[] { "", AppCommand.Unknown, Array.Empty<string>() },

        // Comando desconocido
        new object[] { "fly moon", AppCommand.Unknown, new[] { "moon" } }
        };

    [Theory]
    [MemberData(nameof(CommandTestCases))]
    public void GetCommand_ParsesCorrectly(string input, AppCommand expectedCommand, string[] expectedArgs)
    {
        (AppCommand cmd, string[] args) = _logic.GetCommand(input);

        Assert.Equal(expectedCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }

    #endregion
}
