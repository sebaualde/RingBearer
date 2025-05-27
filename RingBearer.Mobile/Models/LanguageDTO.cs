using CommunityToolkit.Mvvm.ComponentModel;

namespace RingBearer.Mobile.Models;

public partial class LanguageDTO : ObservableObject
{
    private bool _isSelected;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsSelected 
    { 
        get => _isSelected ; 
        set => SetProperty(ref _isSelected, value); 
    }
}
