using System;
using System.IO;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using AvApp1.Model;

namespace AvApp1.Services
{
    public interface ICsvService
    {
        List<DataRow> Load(string path);
    }

    public class CsvService : ICsvService
    {
        public List<DataRow> Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден", filePath);

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                IgnoreBlankLines = true,
                PrepareHeaderForMatch = args => args.Header.ToLower() 
            });

            try
            {
                if (!csv.Read() || !csv.ReadHeader())
                    return new List<DataRow>();

                var headers = csv.HeaderRecord;
                if (headers == null) return new List<DataRow>();

                var result = new List<DataRow>();

                while (csv.Read())
                {
                    var row = new DataRow();

                    foreach (var header in headers)
                    {
                        var value = csv.GetField(header);
                        
                        row.Values[header] = Normalize(value);
                    }

                    result.Add(row);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при разборе CSV: {ex.Message}", ex);
            }
        }

        private string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";
            
            string valLower = value.Trim().ToLower();
            if (valLower == "null" || valLower == "none" || valLower == "undefined")
                return "unknown";

            return value.Trim();
        }
    }
}
