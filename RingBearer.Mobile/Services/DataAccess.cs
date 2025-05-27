using RingBearer.Core.Constants;
using RingBearer.Core.Manager;
using RingBearer.Core.Models;
using RingBearer.Mobile.Mapper;
using RingBearer.Mobile.Models;

namespace RingBearer.Mobile.Services;

public class DataAccess(
        IEntriesManager entriesManager,
        IEntriesMapper mapper) : IDataAccess
{
    private readonly IEntriesManager _entriesManager = entriesManager;
    private readonly IEntriesMapper _mapper = mapper;

    public bool FileExist()
    {
        return _entriesManager.FileExist(Path.Combine(FileSystem.AppDataDirectory, AppConstants.FileName));
    }

    public async Task<ResponseDTO> LoginAsync(string masterKey)
    {
        try
        {
            string path = FileSystem.AppDataDirectory;

            // Asegurarse de que el directorio exista
            Directory.CreateDirectory(path);

            await _entriesManager.LoginAsync(masterKey, Path.Combine(FileSystem.AppDataDirectory, AppConstants.FileName));
            return new ResponseDTO { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public IEnumerable<EntryDTO> GetEntries()
    {
        try
        {
            IEnumerable<EntryModel> entries = _entriesManager.GetEntries();
            return _mapper.MapEntries(entries);
        }
        catch
        {
            return [];
        }
    }

    public ResponseDTO GetEntry(string key)
    {
        try
        {
            EntryModel entry = _entriesManager.GetEntry(key);
            return new ResponseDTO
            {
                IsSuccess = true,
                Data = _mapper.MapEntry(entry)
            };
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public IEnumerable<EntryDTO> FilterEntries(string keyword)
    {
        try
        {
            IEnumerable<EntryModel> entries = _entriesManager.FilterEntries(keyword);
            return _mapper.MapEntries(entries);
        }
        catch
        {
            return [];
        }

    }

    public async Task<ResponseDTO> AddEntryAsync(EntryDTO entry)
    {
        try
        {
            EntryModel entryModel = _mapper.MapEntry(entry);
            await _entriesManager.AddEntryAsync(entryModel);
            return new ResponseDTO { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public async Task<ResponseDTO> UpdateEntryAsync(EntryDTO entry)
    {
        try
        {
            EntryModel entryModel = _mapper.MapEntry(entry);
            await _entriesManager.UpdateEntryAsync(entryModel);
            return new ResponseDTO { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public async Task<ResponseDTO> DeleteEntryAsync(string key)
    {
        try
        {
            await _entriesManager.DeleteEntryAsync(key);
            return new ResponseDTO { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public async Task<ResponseDTO> DeleteSelectedEntriesAsync(List<EntryDTO> entries)
    {
        try
        {
            IEnumerable<EntryModel> entryModels = _mapper.MapEntries(entries);
            await _entriesManager.DeleteSelectedEntriesAsync([.. entryModels]);
            return new ResponseDTO { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public async Task<ResponseDTO> ChangeMasterKeyAsync(string masterKey)
    {
        try
        {
            await _entriesManager.ChangeMasterKeyAsync(masterKey);
            return new ResponseDTO { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

}

