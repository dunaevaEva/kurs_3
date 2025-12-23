using System;
using System.Collections.Generic;
using System.ComponentModel; 
using System.Runtime.CompilerServices; 
using System.Windows.Input;
using AvApp1.Inf; 
using AvApp1.Model;

namespace AvApp1.ViewModel
{
    public class ChartSettingsViewModel : INotifyPropertyChanged
    {
        private readonly ChartSettingsState _originalSettings;
        public ChartSettingsState TempSettings { get; }

        public IEnumerable<string> AvailableKeys { get; }
        
        public event Action<bool>? CloseRequested;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        
        public string ChartTitle
        {
            get => TempSettings.ChartTitle;
            set 
            { 
                TempSettings.ChartTitle = value; 
                OnPropertyChanged(); 
            }
        }

        public string TitleAlignment
        {
            get => TempSettings.TitleAlignment;
            set 
            { 
                TempSettings.TitleAlignment = value; 
                OnPropertyChanged(); 
            }
        }
        
        public List<string> AlignmentOptions { get; } = new() { "Left", "Center", "Right" };

        public ChartSettingsViewModel(ChartSettingsState settings, IEnumerable<string> availableKeys)
        {
            _originalSettings = settings;
            AvailableKeys = availableKeys;
            
            TempSettings = new ChartSettingsState
            {
                SelectedKey = settings.SelectedKey,
                ChartType = settings.ChartType,
                TopN = settings.TopN,
                Width = settings.Width,
                Height = settings.Height,
      
                ChartTitle = settings.ChartTitle,
                TitleAlignment = settings.TitleAlignment
            };

            ApplyCommand = new RelayCommand(_ => Apply());
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(false));
        }

        private void Apply()
        {
            _originalSettings.SelectedKey = TempSettings.SelectedKey;
            _originalSettings.ChartType = TempSettings.ChartType;
            _originalSettings.TopN = TempSettings.TopN;
            _originalSettings.Width = TempSettings.Width;
            _originalSettings.Height = TempSettings.Height;
            
            _originalSettings.ChartTitle = TempSettings.ChartTitle;
            _originalSettings.TitleAlignment = TempSettings.TitleAlignment;

            CloseRequested?.Invoke(true);
        }
        
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
