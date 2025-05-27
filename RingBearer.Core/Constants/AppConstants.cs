namespace RingBearer.Core.Constants;
public static class AppConstants
{
    public const string AppConfigsFilePath = "bearer.settings.json";
    public const string BaseName = "Messages";
    public const string Location = "RingBearer.Core";
    public const int SaltSize = 16; // 128-bit
    public const int KeySize = 32;  // 256-bit
    public const int Iterations = 100_000;
    public const int LogoutSleepTime = 300_000; // in miliseconds 5 min = 300_000 

    public const string ClearCommand = "clear";
    public const string PassParamPrefix = "-p";
    public const string UserParamPrefix = "-u";
    public const string NotesParamPrefix = "-n";

    /*
        -----------------------------------------------------------------
        🔎 Desglose de ghâsh-bûrz-krimp.ring:
        -----------------------------------------------------------------
        ghâsh = fuego
        bûrz = oscuro / maligno / del poder oscuro
        krimp = atrapado / encadenado
        .ring = referencia directa al Anillo, perfecto para RingBearer 💍
     */
    public const string FileName = "ghâsh-bûrz-krimp.ring";
}
