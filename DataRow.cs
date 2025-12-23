using System;
using System.Collections.Generic;

namespace AvApp1.Model;

public class DataRow
{
    public Dictionary<string, string> Values { get; set; } = new();
    
    public string this[string key]
    {
        get => Values.ContainsKey(key) ? Values[key] : string.Empty;
        set => Values[key] = value;
    }
}