using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvApp1.ViewModel
{
    public class StatsViewModel : INotifyPropertyChanged
    {
        private double _mean;
        private double _median;
        private double _stdDev;

        public double Mean
        {
            get => _mean;
            set 
            { 
                _mean = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(MeanDisplay)); 
            }
        }

        public double Median
        {
            get => _median;
            set 
            { 
                _median = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(MedianDisplay)); 
            }
        }

        public double StdDev
        {
            get => _stdDev;
            set 
            { 
                _stdDev = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(StdDevDisplay)); 
            }
        }
        
        public string MeanDisplay => $"Среднее: {Mean:F2}";
        public string MedianDisplay => $"Медиана: {Median:F2}";
        public string StdDevDisplay => $"Ст. откл.: {StdDev:F2}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
