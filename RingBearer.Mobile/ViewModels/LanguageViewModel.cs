using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using RingBearer.Core.Language;
using RingBearer.Mobile.AppConfigurations;
using RingBearer.Mobile.Models;
using RingBearer.Mobile.ViewModels.Interfaces;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Resources;

namespace RingBearer.Mobile.ViewModels;

public partial class LanguageViewModel(IStringLocalizer localizer, IUserConfigurations userConfigurations) : ObservableObject, ILanguageViewModel
{
    //private readonly IStringLocalizer _localizer;

    #region Attributes

    private ObservableCollection<LanguageDTO> _languages =
    [
        new() { Id = (int)LanguagesEnum.En, Name = "English", Code = "en", IsSelected = false },
        new() { Id = (int)LanguagesEnum.Es, Name = "Español", Code = "es", IsSelected = false },
        new() { Id = (int)LanguagesEnum.Pt, Name = "Português", Code = "pt", IsSelected = false },
        new() { Id = (int)LanguagesEnum.It, Name = "Italiano", Code = "it", IsSelected = false }
    ];

    private string _pageTitle = string.Empty;
    private string _applyLangButtonText = string.Empty;
    private bool _isApplyButtonVisible = false;
    private readonly string _currentCulture = userConfigurations.Config.Language;

    #endregion

    #region Properties

    public ObservableCollection<LanguageDTO> Languages
    {
        get => _languages;
        set => SetProperty(ref _languages, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    public string ApplyButtonText
    {
        get => _applyLangButtonText;
        set => SetProperty(ref _applyLangButtonText, value);
    }

    public bool IsApplyButtonVisible
    {
        get => _isApplyButtonVisible;
        set => SetProperty(ref _isApplyButtonVisible, value);
    }

    #endregion

    //public LanguageViewModel(IStringLocalizer localizer, IUserConfigurations userConfigurations)
    //{
    //    _localizer = localizer;
    //    _currentCulture = userConfigurations.Config.Language;
    //    SetDefaultLanguage();
    //}

    public void SetDefaultLanguage()
    {
        IsApplyButtonVisible = false;
        ShowSelectedLanguage(_currentCulture);
    }

    public void ShowSelectedLanguage(string cultureSelected)
    {
        foreach (LanguageDTO language in Languages)
        {
            language.IsSelected = language.Code == cultureSelected;
            if (language.IsSelected) cultureSelected = language.Code;
        }

        IsApplyButtonVisible = !cultureSelected.Equals(_currentCulture);

        ResourceManager resourceManager = RingBearer.Core.Resources.Messages.ResourceManager;
        CultureInfo culture = new(cultureSelected);

        PageTitle = resourceManager.GetString("LanguageChangeTitleMAUI", culture)
            ?? localizer["LanguageChangeTitleMAUI"];
        ApplyButtonText = resourceManager.GetString("ApplyLangButtonTextMAUI", culture)
            ?? localizer["ApplyLangButtonTextMAUI"];
    }


}
