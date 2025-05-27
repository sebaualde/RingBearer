
namespace RingBearer.CLI.DataAccess;

public interface IUIDataAccess
{
    Task AddEntryAsync(string[] commandArgs);

    Task ChangeMasterKeyAsync(string[] commandArgs);

    Task DeleteEntryAsync(string[] commandArgs);

    void FilterEntries(string[] commandArgs);

    void GetEntry(string[] commandArgs);

    Task<bool> LoginAsync(string masterKey);

    void ShowEntries();

    Task UpdateEntryAsync(string[] commandArgs);

    void ChangeLanguage(string[] commandArgs);
}