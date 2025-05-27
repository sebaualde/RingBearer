namespace RingBearer.Core.Models;

public class EntryModel : IEntryModel
{
    public string Key { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
