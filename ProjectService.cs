using AvApp1.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AvApp1.Services
{
    public interface IProjectService
    {
        ProjectState CreateNew();
        ProjectState Load(string filePath);
        void Save(ProjectState project, string filePath);
    }

    public class ProjectService : IProjectService
    {
        public ProjectState CreateNew()
        {
            return new ProjectState
            {
                CsvFilePath = null,
                Dataset = new List<DataRow>(),
                ChartSettings = new ChartSettingsState(),
                Labels = new Dictionary<string, LabelInfo>(), 
                IsDirty = false
            };
        }

        public ProjectState Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл проекта не найден", filePath);

            try
            {
                var json = File.ReadAllText(filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var project = JsonSerializer.Deserialize<ProjectState>(json, options)
                              ?? throw new InvalidOperationException("Не удалось прочитать файл проекта");

                project.IsDirty = false;
                return project;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке проекта: {ex.Message}");
            }
        }

        public void Save(ProjectState project, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true 
                };

                var json = JsonSerializer.Serialize(project, options);
                File.WriteAllText(filePath, json);

                project.IsDirty = false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении проекта: {ex.Message}");
            }
        }
    }
}