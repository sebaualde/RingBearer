using RingBearer.Core.Models;

namespace RingBearer.Core.Services;

public interface IFileExchangeService
{
    Task ExportAsync(IEnumerable<EntryModel> entries, ExportOptions options);

    Task<List<EntryModel>> ImportAsync(ExportOptions options);
}
