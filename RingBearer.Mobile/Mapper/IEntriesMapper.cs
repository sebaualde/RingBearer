using RingBearer.Core.Models;
using RingBearer.Mobile.Models;

namespace RingBearer.Mobile.Mapper;
public interface IEntriesMapper
{
    /// <summary>
    /// Maps a list of EntryDTO to EntryModel.
    /// </summary>
    /// <param name="entries"> The list of EntryDTO to map.</param>
    /// <returns> List of EntryModel.</returns>
    IEnumerable<EntryModel> MapEntries(IEnumerable<EntryDTO> entries);

    /// <summary>
    /// Maps a list of EntryModel to EntryDTO.
    /// </summary>
    /// <param name="entries"> The list of EntryModel to map.</param>
    /// <returns> List of EntryDTO.</returns>
    IEnumerable<EntryDTO> MapEntries(IEnumerable<EntryModel> entries);

    /// <summary>
    /// Maps a single EntryDTO to EntryModel.
    /// </summary>
    /// <param name="entry"> The EntryDTO to map.</param>
    /// <returns> EntryModel.</returns>
    EntryModel MapEntry(EntryDTO entry);

    /// <summary>
    /// Maps a single EntryModel to EntryDTO.
    /// </summary>
    /// <param name="entry"> The EntryModel to map.</param>
    /// <returns> EntryDTO.</returns>
    EntryDTO MapEntry(EntryModel entry);
}