using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RingBearer.CLI.DataAccess;
using RingBearer.CLI.Logic;
using RingBearer.CLI.UI;
using RingBearer.Core.Constants;
using RingBearer.Core.Crypto;
using RingBearer.Core.Language;
using RingBearer.Core.Manager;
using RingBearer.Core.Storage;
using System.Globalization;

namespace RingBearer.CLI.Configurations;

public static class ServiceConfigurator
{
    public static ServiceProvider BuildProvider()
    {
        // Armás los servicios
        ServiceCollection services = new();

        // Logging (necesario para IStringLocalizerFactory)
        services.AddLogging();

        // Localización
        services.AddSingleton<ILanguageConfig, LanguageConfig>();

        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<IStringLocalizer>(sp =>
        {   
            // Configurás la localización para los mensajes
            IStringLocalizerFactory factory = sp.GetRequiredService<IStringLocalizerFactory>();
            return factory.Create(AppConstants.BaseName, AppConstants.Location);
        });

        // Servicios de Core
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<IFileManager, FileManager>();
        services.AddSingleton<IEntriesManager, EntriesManager>();

        // Servicios de UI
        services.AddSingleton<IUIManager, UIManager>();
        services.AddSingleton<IUILogic, UILogic>();
        services.AddSingleton<IUIDataAccess, UIDataAccess>();
        services.AddSingleton<IPrintMessages, PrintMessages>();

        var provider = services.BuildServiceProvider();

        // Configurás la cultura por defecto
        var langConfig = provider.GetRequiredService<ILanguageConfig>();
        var culture = new CultureInfo(langConfig.Language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Creás el provider
        return provider;
    }
}
