using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvApp1.Model
{
    public class ChartItem : INotifyPropertyChanged
    {
        private string _label;
        private int _value;
        private string _color;
        private string _originalKey; 
        public string OriginalKey 
        { 
            get => _originalKey; 
            set => _originalKey = value; 
        }

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(); }
        }

        public int Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        
    }
}