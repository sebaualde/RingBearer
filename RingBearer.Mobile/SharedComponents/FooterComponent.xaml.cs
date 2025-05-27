using Microsoft.Extensions.Localization;
using System.Windows.Input;

namespace RingBearer.Mobile.SharedComponents;

public partial class FooterComponent : ContentView
{
    public string DevelopedText { get; set; } = string.Empty;

    public FooterComponent()
    {
        InitializeComponent();
        SetDeveloperText();
 
        BindingContext = this;
    }

    private void SetDeveloperText()
    {
        var localizer = App.Services.GetService<IStringLocalizer>();
        if (localizer != null)
        {
            DevelopedText = localizer["DevelopedByTextMAUI"];
        }
    }

    private void OpenSursoftPage(object sender, TappedEventArgs e)
    {
        // Logic to open the Sursoft website
        Launcher.Default.OpenAsync(new Uri("https://sursoft.org"));
    }
}
