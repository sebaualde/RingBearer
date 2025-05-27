using Microsoft.Extensions.Localization;
using RingBearer.Core.Constants;
using RingBearer.Core.Models;
using RingBearer.Core.Storage;

namespace RingBearer.Core.Manager;

public class EntriesManager(
    IStringLocalizer localizer,
    IFileManager fileManager) : IEntriesManager
{
    private readonly IStringLocalizer _localizer = localizer;
    private readonly IFileManager _fileManager = fileManager;

    private List<EntryModel> _fileEntries = [];
    private string _masterKey = string.Empty;
    private string _filePath = string.Empty;

    /// <summary>
    /// Check if the file of entries exists.
    /// </summary>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> True if the file exists, otherwise false.</returns>
    public bool FileExist(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// Login to the application with the provided master key.
    /// </summary>
    /// <param name="masterKey"> The master key to login.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> Thrown when the master key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the master key is invalid.</exception>
    public async Task LoginAsync(string masterKey, string filePath = "")
    {
        if (string.IsNullOrWhiteSpace(masterKey))
            throw new ArgumentException(_localizer["MasterKeyNullError"]);

        _masterKey = masterKey;
        _filePath = string.IsNullOrEmpty(filePath) ? AppConstants.FileName : filePath;
        _fileEntries = await _fileManager.LoadAsync(masterKey, _filePath);

        if (_fileEntries == null)
            throw new InvalidOperationException(_localizer["InvalidMasterKeyError"]);
    }

    /// <summary>
    /// Get all entries.
    /// </summary>
    /// <returns> List of EntryModel.</returns>
    public IEnumerable<EntryModel> GetEntries()
    {
        return [.. _fileEntries];
    }

    /// <summary>
    /// Get an entry by its key.
    /// </summary>
    /// <param name="key"> The key of the entry to retrieve.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"> Thrown when there are no entries.</exception>
    /// <exception cref="ArgumentException"> Thrown when the key is null or empty.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    public EntryModel GetEntry(string key)
    {
        if (_fileEntries.Count == 0)
            throw new InvalidOperationException(_localizer["NoEntries"]);

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(_localizer["KeyNullError"]);

        EntryModel? entry = _fileEntries.FirstOrDefault(e => e.Key.Contains(key, StringComparison.OrdinalIgnoreCase));

        return entry ?? throw new KeyNotFoundException(_localizer["EntryNotFound"]);
    }

    /// <summary>
    /// Filter entries based on a keyword.
    /// </summary>
    /// <param name="keyword"> The keyword to filter entries.</param>
    /// <returns> List of filtered EntryModel.</returns>
    /// <exception cref="ArgumentException"> Thrown when the keyword is null or empty.</exception>
    public IEnumerable<EntryModel> FilterEntries(string keyword)
    {
        return string.IsNullOrWhiteSpace(keyword)
            ? [.. _fileEntries]
            : [.. _fileEntries.Where(e =>
            (e.Key?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
            (e.Password?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
            (e.UserName?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
            (e.Notes?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ?? false))];
    }

    /// <summary>
    /// Add a new entry to the list.
    /// </summary>
    /// <param name="entry"> The entry to add.</param>
    /// <exception cref="ArgumentNullException"> Thrown when the entry is null.</exception>
    /// <exception cref="ArgumentException"> Thrown when the key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the entry already exists.</exception>
    public async Task AddEntryAsync(EntryModel entry)
    {
        // Validar la entrada
        ValidateAddEntry(entry);

        // Actualizar lista local
        _fileEntries.Add(entry);

        // Guardar la lista actualizada en el archivo
        await _fileManager.SaveAsync(_fileEntries, _masterKey, _filePath);

    }

    /// <summary>
    /// Update an existing entry in the list.
    /// </summary>
    /// <param name="updatedEntry"> The entry to update.</param>
    /// <returns> Task.</returns>
    /// <exception cref="ArgumentNullException"> Thrown when the entry is null.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    public async Task UpdateEntryAsync(EntryModel updatedEntry)
    {
        if (updatedEntry == null)
            throw new ArgumentNullException(_localizer["EntryNullError"]);

        EntryModel? existingEntry = _fileEntries.FirstOrDefault(e => e.Key.Equals(updatedEntry.Key, StringComparison.CurrentCultureIgnoreCase))
            ?? throw new KeyNotFoundException(_localizer["EntryNotFound"]);

        // Actualizar lista local
        UpdateEntryFields(existingEntry, updatedEntry);

        // Guardar la lista actualizada en el archivo
        await _fileManager.SaveAsync(_fileEntries, _masterKey, _filePath);
    }

    /// <summary>
    /// Delete an entry from the list.
    /// </summary>
    /// <param name="key"> The key of the entry to delete.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> Thrown when the key is null or empty.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    public async Task DeleteEntryAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(_localizer["KeyNullError"], nameof(key));

        // Buscar la entrada a eliminar
        EntryModel? entryToDelete = _fileEntries.FirstOrDefault(e => e.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            ?? throw new KeyNotFoundException(_localizer["EntryNotFound"]);

        // Eliminar la entrada de la lista
        _fileEntries.Remove(entryToDelete);

        // Guardar la lista actualizada en el archivo
        await _fileManager.SaveAsync(_fileEntries, _masterKey, _filePath);
    }

    /// <summary>
    /// Delete the selected entries from the list.
    /// </summary>
    /// <param name="selectedEntries"> The list of selected entries to delete.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> Thrown when the selected entries are null or empty.</exception>
    /// <exception cref="KeyNotFoundException"> Thrown when the entry is not found.</exception>
    public async Task DeleteSelectedEntriesAsync(List<EntryModel> selectedEntries)
    {
        if (selectedEntries == null || selectedEntries.Count == 0)
            throw new ArgumentException(_localizer["SelectedEntriesNullError"]);

        // Eliminar las entradas seleccionadas de la lista
        foreach (EntryModel entry in selectedEntries)
        {
            EntryModel? entryToDelete = _fileEntries.FirstOrDefault(e => e.Key.Equals(entry.Key, StringComparison.CurrentCultureIgnoreCase))
                ?? throw new KeyNotFoundException(_localizer["EntryNotFound"]);
            _fileEntries.Remove(entryToDelete);
        }
        // Guardar la lista actualizada en el archivo
        await _fileManager.SaveAsync(_fileEntries, _masterKey, _filePath);
    }

    /// <summary>
    /// Change the master key for the application.
    /// </summary>
    /// <param name="newMasterKey"> The new master key to set.</param>
    /// <returns> Task.</returns>
    /// <exception cref="ArgumentException"> Thrown when the new master key is null or empty.</exception>
    public async Task ChangeMasterKeyAsync(string newMasterKey)
    {
        if (string.IsNullOrWhiteSpace(newMasterKey))
            throw new ArgumentException(_localizer["MasterKeyNullError"], nameof(newMasterKey));

        // Guardar la lista actualizada en el archivo
        _masterKey = newMasterKey;
        await _fileManager.SaveAsync(_fileEntries, newMasterKey, _filePath);
    }

    /// <summary>
    /// Import entries from a list of EntryModel.
    /// </summary>
    /// <param name="importedEntries"> The list of entries to import.</param>
    /// <exception cref="ArgumentException"> Thrown when the imported entries are null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when there are duplicate entries.</exception>
    public async Task ImportEntriesAsync(List<EntryModel> importedEntries)
    {
        // 1- validar entradas 
        if (importedEntries == null || importedEntries.Count == 0)
            throw new ArgumentException(_localizer["ImportEntriesNullError"]);

        //2- importamos las entradas que no existen en la lista de entradas
        List<EntryModel> entriesToImport = [];
        foreach (EntryModel entry in importedEntries)
        {
            if (EntryAlreadyExists(entry))
            {
                //establecer los valores de la entrada en la entrada existente
                EntryModel? existingEntry = _fileEntries.FirstOrDefault(e => e.Key.ToLower().Equals(entry.Key.ToLower(), StringComparison.OrdinalIgnoreCase));
                if (existingEntry != null)
                {
                    UpdateEntryFields(existingEntry, entry);
                }
            }
            else
            {
                entriesToImport.Add(entry);
            }
        }

        // 3- crear una nueva lista que contenga las entradas del archivo y las importadas
        List<EntryModel> updatedList = [.. _fileEntries, .. entriesToImport];

        // 4- Guardar la lista actualizada en el archivo
        await _fileManager.SaveAsync(updatedList, _masterKey, _filePath);

        // 5- Actualizar la lista local
        _fileEntries = updatedList;
    }

    /// <summary>
    /// Update the fields of an existing entry with the values from the updated entry.
    /// </summary>
    /// <param name="existingEntry"> The existing entry to update.</param>
    /// <param name="updatedEntry"> The updated entry with new values.</param>
    private static void UpdateEntryFields(EntryModel existingEntry, EntryModel updatedEntry)
    {
        if (string.IsNullOrWhiteSpace(updatedEntry.UserName) is false && existingEntry.UserName != updatedEntry.UserName)
        {
            if (updatedEntry.UserName == AppConstants.ClearCommand)
                existingEntry.UserName = string.Empty;
            else
                existingEntry.UserName = updatedEntry.UserName;
        }

        if (string.IsNullOrWhiteSpace(updatedEntry.Password) is false && existingEntry.Password != updatedEntry.Password)
        {
            if (updatedEntry.Password == AppConstants.ClearCommand)
                existingEntry.Password = string.Empty;
            else
                existingEntry.Password = updatedEntry.Password;
        }

        if (string.IsNullOrWhiteSpace(updatedEntry.Notes) is false && existingEntry.Notes != updatedEntry.Notes)
        {
            if (updatedEntry.Notes == AppConstants.ClearCommand)
                existingEntry.Notes = string.Empty;
            else
                existingEntry.Notes = updatedEntry.Notes;
        }
    }

    /// <summary>
    /// Validate the entry before adding it to the list.
    /// </summary>
    /// <param name="entry"> The entry to validate.</param>
    /// <exception cref="ArgumentNullException"> Thrown when the entry is null.</exception>
    /// <exception cref="ArgumentException"> Thrown when the key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the entry already exists.</exception>
    private void ValidateAddEntry(EntryModel entry)
    {
        // Validar la entrada
        if (entry == null)
            throw new ArgumentNullException(_localizer["EntryNullError"]);

        if (string.IsNullOrWhiteSpace(entry.Key))
            throw new ArgumentException(_localizer["KeyNullError"]);

        // Verificar si la entrada ya existe
        if (_fileEntries.Any(e => e.Key.ToLower().Equals(entry.Key.ToLower(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(_localizer["EntryAlreadyExists", entry.Key]);

        if (entry.UserName == AppConstants.ClearCommand)
            entry.UserName = string.Empty;

        if (entry.Password == AppConstants.ClearCommand)
            entry.Password = string.Empty;

        if (entry.Notes == AppConstants.ClearCommand)
            entry.Notes = string.Empty;
    }

    private bool EntryAlreadyExists(EntryModel entry)
    {
        return _fileEntries.Any(e => e.Key.ToLower().Equals(entry.Key.ToLower(), StringComparison.OrdinalIgnoreCase));
    }
}