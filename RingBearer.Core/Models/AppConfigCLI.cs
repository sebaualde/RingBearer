namespace RingBearer.Core.Models;

public class AppConfigCLI
{
    public string Language { get; set; } = "en";
    public int Theme { get; set; } = 1; // 0=Unspecified, 1=Light, 2=Dark
}
