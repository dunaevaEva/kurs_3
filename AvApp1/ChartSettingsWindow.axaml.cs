using Avalonia.Controls;
using AvApp1.ViewModel;
using System;

namespace AvApp1.Views
{
    public partial class ChartSettingsWindow : Window
    {
        public ChartSettingsWindow()
        {
            InitializeComponent();
            
            this.DataContextChanged += ChartSettingsWindow_DataContextChanged;
        }

        private void ChartSettingsWindow_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is ChartSettingsViewModel vm)
            {
                vm.CloseRequested += result =>
                {
                    this.Close(result);
                };
            }
        }
    }
}
