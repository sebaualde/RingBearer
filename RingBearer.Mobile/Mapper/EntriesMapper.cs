using RingBearer.Core.Models;
using RingBearer.Mobile.Models;

namespace RingBearer.Mobile.Mapper;

public class EntriesMapper : IEntriesMapper
{
    public IEnumerable<EntryDTO> MapEntries(IEnumerable<EntryModel> entries)
    {
        return [.. entries.Select(entry => new EntryDTO
        {
            Key = entry.Key,
            UserName = entry.UserName,
            Password = entry.Password,
            Notes = entry.Notes,
        })];
    }

    public IEnumerable<EntryModel> MapEntries(IEnumerable<EntryDTO> entries)
    {
        return [.. entries.Select(entry => new EntryModel
        {
            Key = entry.Key,
            UserName = entry.UserName,
            Password = entry.Password,
            Notes = entry.Notes,
        })];
    }

    public EntryDTO MapEntry(EntryModel entry)
    {
        return new EntryDTO
        {
            Key = entry.Key,
            UserName = entry.UserName,
            Password = entry.Password,
            Notes = entry.Notes,
        };
    }

    public EntryModel MapEntry(EntryDTO entry)
    {
        return new EntryModel
        {
            Key = entry.Key,
            UserName = entry.UserName,
            Password = entry.Password,
            Notes = entry.Notes,
        };
    }
}

