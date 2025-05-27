using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage(IAboutViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnLearnMoreClicked(object sender, EventArgs e)
    {
        Launcher.Default.OpenAsync(new Uri("https://ringbearer.sursoft.org/documentation"));
    }

    private void OnLearnPrivacyPolicyClicked(object sender, EventArgs e)
    {
        Launcher.Default.OpenAsync(new Uri("https://ringbearer.sursoft.org/privacy"));
    }

    private void OnTermsServiceClicked(object sender, EventArgs e)
    {
        Launcher.Default.OpenAsync(new Uri("https://ringbearer.sursoft.org/terms"));
    }

    private void OnCheckForUpdatesClicked(object sender, EventArgs e)
    {
        Launcher.Default.OpenAsync(new Uri("https://ringbearer.sursoft.org/download"));
    }
}