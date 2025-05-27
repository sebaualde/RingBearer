using RingBearer.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RingBearer.Mobile.ViewModels.Interfaces;

public interface IEntryViewModel
{
    #region UI Messages

    string EntryPageTitle { get; }

    string KeyPlaceholder { get; }
    string UserNamePlaceholder { get; }
    string PasswordPlaceholder { get; }
    string NotesPlaceholder { get; }
    string SaveButtonText { get; }

    string ErrorMessage { get; }
    bool IsErrorMessageVisible { get; }

    string SuccessMessage { get; }
    bool IsSuccessMessageVisible { get; }

    bool IsSaveButtonEnabled { get; }

    #endregion

    EntryDTO Model { get; set; }

    bool IsAddNewMode { get; }

    bool IsRefreshing { get; }

    /// <summary>
    /// Loads the entry from the file using the specified key.
    /// </summary>
    /// <param name="key"> The key of the entry to load.</param>
    void LoadEntry(string key);

    /// <summary>
    /// Saves the entry to the file.
    /// </summary>
    /// <returns> A task representing the asynchronous operation.</returns>
    Task SaveEntryAsync();

    /// <summary>
    /// Copies the specified field text to the clipboard.
    /// </summary>
    /// <param name="fieldText"> The text to copy to the clipboard.</param>
    /// <returns> A task representing the asynchronous operation.</returns>
    Task CopyToClipboardAsync(string fieldText);
}