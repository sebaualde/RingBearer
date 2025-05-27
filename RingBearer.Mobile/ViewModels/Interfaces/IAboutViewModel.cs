namespace RingBearer.Mobile.ViewModels.Interfaces;

public interface IAboutViewModel
{
    public string VersionText { get; }
    public string DeveloperText { get; }
    public string PlatformsText { get; }
    public string LicenseText { get; }

    public string WhatIsItTitle { get; }
    public string WhatIsItText { get; }

    public string FeaturesTitle { get; }
    public string Functionality1Text { get; }
    public string Functionality2Text { get; }
    public string Functionality3Text { get; }
    public string Functionality4Text { get; }
    public string Functionality5Text { get; }

    public string ImportantTitle { get; }
    public string Important1Text { get; }
    public string Important2Text { get; }
    public string Important3Text { get; }

    public string AutorTitle { get; }
    public string AutorText { get; }

    public string LearnMoreText { get; }
    public string PrivacyPolicyText { get; }
    public string TermsServiceText { get; }
    public string CheckForUpdatesText { get; }
}
