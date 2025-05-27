namespace RingBearer.Core.Models;

public interface IEntryModel
{
    string Key { get; set; }
    string Notes { get; set; }
    string Password { get; set; }
    string UserName { get; set; }
}