namespace RingBearer.CLI.UI;
internal interface IUIManager
{
    Task InitAppMenuAsync();

    Task<bool> ShowLoginAsync();

    void PrintErrorMessage(string message);
}
