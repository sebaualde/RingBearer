
using Microsoft.Extensions.DependencyInjection;
using RingBearer.CLI.Configurations;
using RingBearer.CLI.UI;
using System.Globalization;

try
{

    #region AppConfigurationStart

    Console.Title = "Ring Bearer - Sursoft";

    ServiceProvider serviceProvider = ServiceConfigurator.BuildProvider();
    IUIManager uiManager = serviceProvider.GetRequiredService<IUIManager>();

    #endregion

    bool isExitPressed = await uiManager.ShowLoginAsync();

    if (isExitPressed is false) await uiManager.InitAppMenuAsync();
}
catch (Exception ex)
{
    //uiManager.PrintErrorMessage(ex.Message);
    Console.WriteLine(ex.Message);
}
