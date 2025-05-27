using RingBearer.Core.Models;

namespace RingBearer.Mobile.ViewModels.Interfaces;
public interface IImportExportViewModel
{
    #region UI Messages

    string ExportSubtitle { get; }
    string ExportPlaceHolder { get; }
    string ExportButtonText { get; }
    string ImportSubtitle { get; }
    string ImportPlaceHolder { get; }
    string ImportButtonText { get; }
    string WarningImportText { get; }

    #endregion

    string ExportPassword { get; set; }
    bool IsExportPasswordHidden { get; set; }

    string ImportPassword { get; set; }
    bool IsImportPasswordHidden { get; set; }

    Task ExportAsync();

    Task ImportAsync();
}
