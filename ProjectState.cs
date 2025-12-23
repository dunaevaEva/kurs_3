using System.Collections.Generic;

namespace AvApp1.Model;

public class ProjectState
{
    public string? CsvFilePath { get; set; } 
    
    public bool IsDirty { get; set; }
    
    public List<DataRow> Dataset { get; set; } = new(); 
    
    public ChartSettingsState ChartSettings { get; set; } = new();
    
    public Dictionary<string, LabelInfo> Labels { get; set; } = new();
    
    public void Reset()
    {
        CsvFilePath = null;
        IsDirty = false;
        Dataset.Clear();
        Labels.Clear();
        ChartSettings = new ChartSettingsState();
    }
}