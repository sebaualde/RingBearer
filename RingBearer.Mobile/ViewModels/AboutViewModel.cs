using Microsoft.Extensions.Localization;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.ViewModels;
public class AboutViewModel(IStringLocalizer localizer) : IAboutViewModel
{
    public string VersionText => localizer["AboutVersionTextMAUI"];
    public string DeveloperText => localizer["AboutDeveloperTextMAUI"];
    public string PlatformsText => localizer["AboutPlatformsTextMAUI"];
    public string LicenseText => localizer["AboutLicenseTextMAUI"];

    public string WhatIsItTitle => localizer["AboutWhatIsItTitleMAUI"];
    public string WhatIsItText => localizer["AboutWhatIsItTextMAUI"];

    public string FeaturesTitle => localizer["AboutFeaturesTitleMAUI"];

    public string Functionality1Text => localizer["AboutFunctionality1TextMAUI"];
    public string Functionality2Text => localizer["AboutFunctionality2TextMAUI"];
    public string Functionality3Text => localizer["AboutFunctionality3TextMAUI"];
    public string Functionality4Text => localizer["AboutFunctionality4TextMAUI"];
    public string Functionality5Text => localizer["AboutFunctionality5TextMAUI"];

    public string ImportantTitle => localizer["AboutImportantTitleMAUI"];
    public string Important1Text => localizer["AboutImportant1TextMAUI"];
    public string Important2Text => localizer["AboutImportant2TextMAUI"];
    public string Important3Text => localizer["AboutImportant3TextMAUI"];

    public string AutorTitle => localizer["AboutAuthorTitleMAUI"];
    public string AutorText => localizer["AboutAuthorTextMAUI"];

    public string LearnMoreText => localizer["AboutLearnMoreTextMAUI"];
    public string PrivacyPolicyText => localizer["AboutPrivacyPolicyTextMAUI"];
    public string TermsServiceText => localizer["AboutTermsServiceTextMAUI"];
    public string CheckForUpdatesText => localizer["AboutCheckForUpdatesTextMAUI"];
}
