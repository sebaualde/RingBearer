
using RingBearer.Core.Constants;
using RingBearer.Mobile.Constants;
using RingBearer.Mobile.Views;

namespace RingBearer.Mobile;

public partial class App : Application
{
    private CancellationTokenSource? _sleepCancellationTokenSource;
    private bool _goTologinPage = false;

    public static IServiceProvider Services { get; private set; } = null!;

    public static Window MainWindow { get; set; } = null!;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        Services = serviceProvider;
        Preferences.Set(PreferencesNames.IsAuthenticated, false);
    }

    // Crea la ventana principal de la aplicación y en windows especifica el tamaño inicial de la ventana
    protected override Window CreateWindow(IActivationState? activationState)
    {
        MainWindow = new Window()
        {
            Page = new AppShell(),
            MinimumWidth = 400,
            MaximumWidth =400,
            MinimumHeight = 700
        };
        return MainWindow;
    }

    // Inicializa el temporizador de cierre de sesión por inactividad
    protected override void OnSleep()
    {
        base.OnSleep();

        _sleepCancellationTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(AppConstants.LogoutSleepTime, _sleepCancellationTokenSource.Token);

                if (_sleepCancellationTokenSource != null && MainWindow.Page is not LoginPage)
                {
                    _goTologinPage = true;
                }
            }
            catch (TaskCanceledException)
            {
            }
        });
    }

    // Cancela el temporizador de cierre de sesión por inactividad o navega a la página de inicio de sesión 
    protected async override void OnResume()
    {
        base.OnResume();

        // Cancelamos el temporizador si volvemos antes del tiempo de espera
        _sleepCancellationTokenSource?.Cancel();
        _sleepCancellationTokenSource = null;

        if (_goTologinPage && App.Current!.MainPage is AppShell shell)
        {
            Preferences.Set(PreferencesNames.IsAuthenticated, false);
            await shell.NavigateToLoginPageAsync();
            _goTologinPage = false;
        }
    }
}
