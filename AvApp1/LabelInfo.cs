using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvApp1.Model;

public class LabelInfo : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _color = "#808080";
    private string _displayName = string.Empty;

    public string Name 
    { 
        get => _name; 
        set => SetProperty(ref _name, value); 
    }

    public string Color 
    { 
        get => _color; 
        set => SetProperty(ref _color, value); 
    }

    public string DisplayName 
    { 
        get => _displayName; 
        set => SetProperty(ref _displayName, value); 
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
