using RingBearer.Core.Models;
using RingBearer.Mobile.Models;

namespace RingBearer.Mobile.Services;

public interface IDataAccess
{
    /// <summary>
    /// Check if the file of entries exists.
    /// </summary>
    /// <returns> True if the file exists, otherwise false.</returns>
    bool FileExist();

    /// <summary>
    /// Login to the application with the provided master key.
    /// </summary>
    /// <param name="masterKey"> The master key to login.</param>
    /// <returns> ResponseDTO.</returns>
    Task<ResponseDTO> LoginAsync(string masterKey);

    /// <summary>
    /// Get all entries.
    /// </summary>
    /// <returns> List of EntryDTO.</returns>
    IEnumerable<EntryDTO> GetEntries();

    /// <summary>
    /// Get an entry by its key.
    /// </summary>
    /// <param name="key"> The key of the entry to retrieve.</param>
    /// <returns> ResponseDTO which contains the entry.</returns>
    ResponseDTO GetEntry(string key);

    /// <summary>
    /// Update an existing entry in the list.
    /// </summary>
    /// <param name="keyword"> The keyword to filter the entries.</param>
    /// <returns> List of EntryDTO.</returns>
    IEnumerable<EntryDTO> FilterEntries(string keyword);

    /// <summary>
    /// Add a new entry to the list.
    /// </summary>
    /// <param name="entry"> The entry to add.</param>
    /// <returns> ResponseDTO whit the result of the operation.</returns>
    Task<ResponseDTO> AddEntryAsync(EntryDTO entry);

    /// <summary>
    /// Update an existing entry in the list.
    /// </summary>
    /// <param name="entry"> The entry to update.</param>
    /// <returns> ResponseDTO whit the result of the operation.</returns>
    Task<ResponseDTO> UpdateEntryAsync(EntryDTO entry);

    /// <summary>
    /// Delete a list of entries.
    /// </summary>
    /// <param name="entries"> The list of entries to delete.</param>
    /// <returns> ResponseDTO whit the result of the operation.</returns>
    Task<ResponseDTO> DeleteSelectedEntriesAsync(List<EntryDTO> entries);

    /// <summary>
    /// Change the master key of the application.
    /// </summary>
    /// <param name="masterKey"> The new master key to set.</param>
    /// <returns> ResponseDTO whit the result of the operation.</returns>
    Task<ResponseDTO> ChangeMasterKeyAsync(string masterKey);
}
