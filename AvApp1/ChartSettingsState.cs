using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvApp1.Model
{
    public class ChartSettingsState : INotifyPropertyChanged
    {
        private string? _selectedKey;
        private string? _chartType = "Bar";
        private int _topN = 5;
        private int _width = 600; 
        private int _height = 400; 
        private string _chartTitle = "График данных"; 
        private string _titleAlignment = "Center";   

        public string? SelectedKey
        {
            get => _selectedKey;
            set { _selectedKey = value; OnPropertyChanged(); }
        }

        public string? ChartType
        {
            get => _chartType;
            set { _chartType = value; OnPropertyChanged(); }
        }

        public int TopN
        {
            get => _topN;
            set { _topN = value; OnPropertyChanged(); }
        }

        public int Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }

        public int Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }
        
        public string ChartTitle
        {
            get => _chartTitle;
            set { _chartTitle = value; OnPropertyChanged(); }
        }

        public string TitleAlignment
        {
            get => _titleAlignment;
            set { _titleAlignment = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
