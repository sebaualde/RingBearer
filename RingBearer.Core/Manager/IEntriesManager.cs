using RingBearer.Core.Models;

namespace RingBearer.Core.Manager;

public interface IEntriesManager
{
    /// <summary>
    /// Check if the file of entries exists.
    /// </summary>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> True if the file exists, otherwise false.</returns>
    bool FileExist(string filePath);

    /// <summary>
    /// Login to the application with the provided master key.
    /// </summary>
    /// <param name="masterKey"> The master key to login.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> Thrown when the master key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the master key is invalid.</exception>
    Task LoginAsync(string masterKey, string filePath = "");

    /// <summary>
    /// Get all entries.
    /// </summary>
    /// <returns> List of EntryModel.</returns>
    IEnumerable<EntryModel> GetEntries();

    /// <summary>
    /// Get an entry by its key.
    /// </summary>
    /// <param name="key"> The key of the entry to retrieve.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"> Thrown when there are no entries.</exception>
    /// <exception cref="ArgumentException"> Thrown when the key is null or empty.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    EntryModel GetEntry(string key);

    /// <summary>
    /// Update an existing entry in the list.
    /// </summary>
    /// <param name="updatedEntry"> The entry to update.</param>
    /// <returns> Task.</returns>
    /// <exception cref="ArgumentNullException"> Thrown when the entry is null.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    IEnumerable<EntryModel> FilterEntries(string keyword);

    /// <summary>
    /// Add a new entry to the list.
    /// </summary>
    /// <param name="entry"> The entry to add.</param>
    /// <exception cref="ArgumentNullException"> Thrown when the entry is null.</exception>
    /// <exception cref="ArgumentException"> Thrown when the key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the entry already exists.</exception>
    Task AddEntryAsync(EntryModel entry);

    /// <summary>
    /// Update an existing entry in the list.
    /// </summary>
    /// <param name="updatedEntry"> The entry to update.</param>
    /// <returns> Task.</returns>
    /// <exception cref="ArgumentNullException"> Thrown when the entry is null.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    Task UpdateEntryAsync(EntryModel entry);

    /// <summary>
    /// Delete an entry from the list.
    /// </summary>
    /// <param name="key"> The key of the entry to delete.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> Thrown when the key is null or empty.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    Task DeleteEntryAsync(string key);

    /// <summary>
    /// Delete the selected entries from the list.
    /// </summary>
    /// <param name="selectedEntries"> The list of selected entries to delete.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> Thrown when the selected entries are null or empty.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    Task DeleteSelectedEntriesAsync(List<EntryModel> selectedEntries);

    /// <summary>
    /// Change the master key for the application.
    /// </summary>
    /// <param name="newMasterKey"> The new master key to set.</param>
    /// <returns> Task.</returns>
    /// <exception cref="ArgumentException"> Thrown when the new master key is null or empty.</exception>
    Task ChangeMasterKeyAsync(string newMasterKey);

    /// <summary>
    /// Import entries from a list of EntryModel.
    /// </summary>
    /// <param name="importedEntries"> The list of entries to import.</param>
    /// <exception cref="ArgumentException"> Thrown when the imported entries are null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when there are duplicate entries.</exception>
    Task ImportEntriesAsync(List<EntryModel> importedEntries);
}
