using RingBearer.Mobile.Models;
using System.Collections.ObjectModel;

namespace RingBearer.Mobile.ViewModels.Interfaces;
public interface ILanguageViewModel
{
    string PageTitle { get; }
    string ApplyButtonText { get; }
    bool IsApplyButtonVisible { get; }

    ObservableCollection<LanguageDTO> Languages { get; set; }

    void ShowSelectedLanguage(string selectedCulture);

    void SetDefaultLanguage();
}
