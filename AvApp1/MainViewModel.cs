using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AvApp1.Inf;
using AvApp1.Model;
using AvApp1.Services;
using AvApp1.Views;

namespace AvApp1.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // --- Сервисы ===================================
        private readonly IProjectService _projectService;
        private readonly ICsvService _csvService;
        private readonly IExportService _exportService;
        
        private readonly ChartService _chartService = new();
        private readonly StatisticsService _statsService = new(); 
        private readonly Random _random = new Random();

        // --- Поля данных ==============================
        private ProjectState _currentProject;
        private string _outputMessage = "Готово";
        private ChartItem? _selectedChartItem; 
        private IEnumerable<string> _availableKeys = new List<string>();

        // --- События ===================================
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Func<Task<string?>>? SaveFileRequested;
        public event Func<Task<string?>>? OpenFileRequested;
        public event Func<Task<string?>>? ExportImageRequested;
        public event Action? CloseApplicationRequested;

        // --- Свойства =================================

        public ProjectState CurrentProject
        {
            get => _currentProject;
            private set
            {
                _currentProject = value;
                OnPropertyChanged(nameof(CurrentProject));
            }
        }

        public string OutputMessage
        {
            get => _outputMessage;
            set
            {
                _outputMessage = value;
                OnPropertyChanged(nameof(OutputMessage));
            }
        }

        // Выбор колонки ============================
        public string? SelectedChartKey
        {
            get => CurrentProject.ChartSettings.SelectedKey;
            set
            {
                if (CurrentProject.ChartSettings.SelectedKey != value)
                {
                    CurrentProject.ChartSettings.SelectedKey = value;
                    OnPropertyChanged(nameof(SelectedChartKey));
                    CurrentProject.IsDirty = true;
                    
                    UpdateChart();
                }
            }
        }

        // Список доступных ключей CSV =================
        public IEnumerable<string> AvailableKeys
        {
            get => _availableKeys;
            set
            {
                _availableKeys = value;
                OnPropertyChanged(nameof(AvailableKeys));
            }
        }

        // Коллекция данных ==========================================
        public ObservableCollection<ChartItem> ChartData { get; } = new();

        public ChartItem? SelectedChartItem
        {
            get => _selectedChartItem;
            set
            {
                _selectedChartItem = value;
                OnPropertyChanged(nameof(SelectedChartItem));
                (RemoveLabelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Команды ==================================================
        public ICommand NewProjectCommand { get; }
        public ICommand LoadProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand LoadCsvCommand { get; }
        public ICommand SetUpChartCommand { get; }
        public ICommand ExitCommand  { get; }
        public ICommand ResetCommand { get; }
        public ICommand RemoveLabelCommand { get; }
        public ICommand ExportCommand { get; }

        // --- Конструктор ========================================================
        public MainViewModel(IProjectService projectService, ICsvService csvService)
        {
            _projectService = projectService;
            _csvService = csvService;
            _exportService = new ExportService();
            
            _currentProject = _projectService.CreateNew();
            
            NewProjectCommand = new RelayCommand(_ => CreateNewProject());
            LoadProjectCommand = new RelayCommand(_ => LoadProject());
            SaveProjectCommand = new RelayCommand(_ => SaveProject());
            SaveAsCommand = new RelayCommand(_ => SaveProject()); 
            
            LoadCsvCommand = new RelayCommand(_ => RequestCsvLoad());
            SetUpChartCommand = new RelayCommand(_ => OpenChartSettings());
            
            ExitCommand = new RelayCommand(_ => CloseApplicationRequested?.Invoke());
            
            ExportCommand = new RelayCommand(param => ExportChart(param));

            ResetCommand = new RelayCommand(_ =>
            {
                CurrentProject.Labels.Clear();
                CurrentProject.IsDirty = true;
                UpdateChart();
                OutputMessage = "Настройки отображения сброшены";
            });

            RemoveLabelCommand = new RelayCommand(_ => RemoveSelectedRow(), _ => SelectedChartItem != null);
        }

        // --- Методы логики ========================

        private void CreateNewProject()
        {
            CurrentProject = _projectService.CreateNew();
            ChartData.Clear();
            AvailableKeys = new List<string>();
            OnPropertyChanged(nameof(AvailableKeys));
            OnPropertyChanged(nameof(SelectedChartKey)); 
            OutputMessage = "Создан новый проект";
        }

        private async void RequestCsvLoad()
        {
            if (OpenFileRequested == null) return;
            var path = await OpenFileRequested.Invoke();
            if (path != null) LoadCsv(path);
        }

        public void LoadCsv(string path)
        {
            try
            {
                var data = _csvService.Load(path);
                if (data != null && data.Any())
                {
                    CurrentProject.Dataset = data;
                    CurrentProject.CsvFilePath = path;
                    CurrentProject.IsDirty = true;

                    AvailableKeys = data.First().Values.Keys.ToList();
                    
                    var firstKey = AvailableKeys.FirstOrDefault();
                    if (string.IsNullOrEmpty(CurrentProject.ChartSettings.SelectedKey) ||
                        !AvailableKeys.Contains(CurrentProject.ChartSettings.SelectedKey))
                    {
                        SelectedChartKey = firstKey; 
                    }
                    else
                    {
                        UpdateChart();
                    }

                    OutputMessage = $"Загружен CSV: {data.Count} строк.";
                }
            }
            catch (Exception ex)
            {
                OutputMessage = $"Ошибка CSV: {ex.Message}";
            }
        }

        public void UpdateChart()
        {
            var settings = CurrentProject.ChartSettings;
            
            if (string.IsNullOrEmpty(settings.SelectedKey) || 
                CurrentProject.Dataset == null || 
                !CurrentProject.Dataset.Any()) 
                return;

            var counts = _chartService.CountByKey(CurrentProject.Dataset, settings.SelectedKey);
            var topN = _chartService.ApplyTopN(counts, settings.TopN);

            ChartData.Clear();
            var valuesForStats = new List<double>();

            foreach (var kv in topN)
            {
                string originalKey = kv.Key;
                int count = kv.Value;
                valuesForStats.Add(count);
                
                if (!CurrentProject.Labels.TryGetValue(originalKey, out var labelInfo))
                {
                    labelInfo = new LabelInfo 
                    { 
                        Name = originalKey, 
                        DisplayName = originalKey, 
                        Color = GetRandomColor()
                    };
                    CurrentProject.Labels[originalKey] = labelInfo;
                }

                var item = new ChartItem
                {
                    OriginalKey = originalKey,
                    Label = labelInfo.DisplayName, 
                    Value = count,
                    Color = labelInfo.Color 
                };
                
                item.PropertyChanged += ChartItem_PropertyChanged;
                
                ChartData.Add(item);
            }

            CalculateStats(valuesForStats);
            OnPropertyChanged(nameof(CurrentProject));
        }

        private void ChartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ChartItem item)
            {
                if (!CurrentProject.Labels.ContainsKey(item.OriginalKey))
                {
                    CurrentProject.Labels[item.OriginalKey] = new LabelInfo { Name = item.OriginalKey };
                }

                var info = CurrentProject.Labels[item.OriginalKey];
                info.DisplayName = item.Label;
                info.Color = item.Color;

                CurrentProject.IsDirty = true;
                
                OnPropertyChanged(nameof(CurrentProject));
            }
        }

        private void RemoveSelectedRow()
        {
            if (SelectedChartItem == null) return;
            
            ChartData.Remove(SelectedChartItem);
            CurrentProject.IsDirty = true;
        }

        private void CalculateStats(List<double> values)
        {
            if (values.Count == 0) return;
            double mean = _statsService.Mean(values);
            double median = _statsService.Median(values);
            OutputMessage = $"Статистика: Среднее={mean:F1}, Медиана={median:F1}";
        }

        // --- Загрузка и Сохранение =============================================

        private async void LoadProject()
        {
            if (OpenFileRequested == null) return;
            var path = await OpenFileRequested.Invoke();
            if (path != null)
            {
                try
                {
                    CurrentProject = _projectService.Load(path);
                    if (CurrentProject.Dataset != null && CurrentProject.Dataset.Any())
                    {
                        AvailableKeys = CurrentProject.Dataset.First().Values.Keys.ToList();
                    }
                    
                    UpdateChart();
                    OutputMessage = "Проект загружен";
                }
                catch (Exception ex)
                {
                    OutputMessage = $"Ошибка загрузки: {ex.Message}";
                }
            }
        }

        private async void SaveProject()
        {
            if (SaveFileRequested == null) return;
            var path = await SaveFileRequested.Invoke();
            if (path != null)
            {
                SaveProjectDirectly(path);
            }
        }
        
        public void SaveProjectDirectly(string path)
        {
            try
            {
                _projectService.Save(CurrentProject, path);
                CurrentProject.IsDirty = false; 
                OutputMessage = "Проект сохранен";
            }
            catch (Exception ex)
            {
                OutputMessage = $"Ошибка сохранения: {ex.Message}";

            }
        }

        private void OpenChartSettings()
        {
            var vm = new ChartSettingsViewModel(CurrentProject.ChartSettings, AvailableKeys);
            vm.CloseRequested += result =>
            {
                if (result)
                {
                    CurrentProject.IsDirty = true;
                    UpdateChart();
                }
            };
            var window = new ChartSettingsWindow { DataContext = vm };
            window.Show();
        }

        private async void ExportChart(object? param)
        {
            if (param is not Avalonia.Controls.Control chartControl)
            {
                OutputMessage = "Ошибка: График не найден";
                return;
            }

            if (ExportImageRequested == null) return;

            var path = await ExportImageRequested.Invoke();
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    _exportService.RenderAndSave(chartControl, path);
                    OutputMessage = $"Экспорт завершен: {path}";
                }
                catch (Exception ex)
                {
                    OutputMessage = $"Ошибка экспорта: {ex.Message}";
                }
            }
        }

        private string GetRandomColor()
        {
            var r = _random.Next(50, 200);
            var g = _random.Next(50, 200);
            var b = _random.Next(50, 200);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
