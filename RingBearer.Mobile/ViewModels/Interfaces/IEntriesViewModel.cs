using RingBearer.Mobile.Models;
using System.Windows.Input;

namespace RingBearer.Mobile.ViewModels.Interfaces;

public interface IEntriesViewModel
{
    #region UI Messages

    string SearchPlaceholder { get; }

    string EntriesTitle { get; }

    string NoEntriesText { get; }

    string SelectAllText { get; }

    string DeleteSelectedButtonText { get; }

    #endregion

    #region Attributes

    string SearchTerm { get; set; }
    bool IsDeleteMode { get; set; }
    bool AreAllSelected { get; set; }
    bool IsRefreshing { get; set; }

    bool ShowList { get; }


    IEnumerable<EntryDTO> EntriesList { get; set; }

    #endregion
    Task LoadEntriesAsync();

    Task DeleteEntriesAsync();

    void ToggleDeleteMode();

    Task InvertEntriesListAsync();
}