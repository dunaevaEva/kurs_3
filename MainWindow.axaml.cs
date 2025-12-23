using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvApp1.ViewModel;
using AvApp1.Views;
using AvApp1.Model;
using AvApp1.Services;
using System.Threading.Tasks;
using System;

namespace AvApp1;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    public MainWindow()
    {
        InitializeComponent();
        
        _vm = new MainViewModel(new ProjectService(), new CsvService());
        DataContext = _vm;
        
        _vm.SaveFileRequested += ShowSaveDialog;
        _vm.OpenFileRequested += ShowOpenDialog;
        _vm.ExportImageRequested += ShowExportImageDialog;
        
        _vm.CloseApplicationRequested += Close; 
        
        Closed += OnWindowClosed;     // отписка от событий
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _vm.SaveFileRequested -= ShowSaveDialog;
        _vm.OpenFileRequested -= ShowOpenDialog;
        _vm.ExportImageRequested -= ShowExportImageDialog;
        _vm.CloseApplicationRequested -= Close;
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        
        if (_vm.CurrentProject?.IsDirty == true)         // Если false, окно закроется
        {
            e.Cancel = true;

            var dialog = new ConfirmCloseDialog();
            var result = await dialog.ShowDialog<ConfirmCloseResult>(this);

            if (result == ConfirmCloseResult.DontSave)
            {
                _vm.CurrentProject.IsDirty = false;
                Close();
            }
            else if (result == ConfirmCloseResult.Save)
            {
                var path = await ShowSaveDialog();
                
                if (string.IsNullOrEmpty(path)) 
                {
                    return; 
                }
                
                _vm.SaveProjectDirectly(path); 

                _vm.CurrentProject.IsDirty = false;
                Close();
            }
        }
    }
    
    private async Task<string?> ShowSaveDialog()
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить проект",
            FileTypeChoices = new[] 
            { 
                new FilePickerFileType("Project files (*.json)") { Patterns = new[] { "*.json" } } 
            },
            DefaultExtension = "json"
        });

        return file?.Path.LocalPath;
    }

    private async Task<string?> ShowOpenDialog()
    {
        var result = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Открыть файл",
            AllowMultiple = false,
            FileTypeFilter = new[] 
            { 
                new FilePickerFileType("Все поддерживаемые") { Patterns = new[] { "*.json", "*.csv" } },
                new FilePickerFileType("JSON Проекты") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("CSV Данные") { Patterns = new[] { "*.csv" } }
            }
        });

        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    private async Task<string?> ShowExportImageDialog()
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить график как изображение",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } }
            },
            DefaultExtension = "png"
        });

        return file?.Path.LocalPath;
    }
}