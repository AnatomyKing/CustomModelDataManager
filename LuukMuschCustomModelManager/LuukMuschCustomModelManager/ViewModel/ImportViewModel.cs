using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ImportViewModel : ObservableObject
    {
        private string _importFolder = @"C:\Users\mrluu\Downloads\parentitems";
        /// <summary>
        /// The folder to scan for JSON files.
        /// </summary>
        public string ImportFolder
        {
            get => _importFolder;
            set
            {
                if (_importFolder != value)
                {
                    _importFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ImportCommand { get; }

        public ImportViewModel()
        {
            ImportCommand = new RelayCommand(ExecuteImport);
        }

        private void ExecuteImport(object? parameter)
        {
            int importedCount = 0;
            try
            {
                // Check if the folder exists.
                if (!Directory.Exists(ImportFolder))
                {
                    System.Windows.MessageBox.Show("Import folder does not exist: " + ImportFolder);
                    return;
                }

                // Get all .json files in the folder.
                string[] jsonFiles = Directory.GetFiles(ImportFolder, "*.json");

                using (var context = new AppDbContext())
                {
                    foreach (var file in jsonFiles)
                    {
                        try
                        {
                            string jsonText = File.ReadAllText(file);
                            var importData = JsonConvert.DeserializeObject<ImportJsonModel>(jsonText);
                            if (importData == null)
                                continue;

                            // If there are no overrides, skip this file.
                            if (importData.overrides == null || importData.overrides.Count == 0)
                                continue;

                            // Use the file name (without extension) as the ParentItem name.
                            string parentName = Path.GetFileNameWithoutExtension(file);

                            // Check if a ParentItem with the same name and type "imported" exists.
                            var parentItem = context.ParentItems.FirstOrDefault(p => p.Name == parentName && p.Type == "imported");
                            if (parentItem == null)
                            {
                                parentItem = new ParentItem
                                {
                                    Name = parentName,
                                    Type = "imported"
                                };
                                context.ParentItems.Add(parentItem);
                            }

                            // Process each override in the JSON file.
                            foreach (var ovrd in importData.overrides)
                            {
                                // Check that the predicate contains "custom_model_data" and that it's nonzero.
                                if (ovrd.predicate == null ||
                                    !ovrd.predicate.ContainsKey("custom_model_data") ||
                                    ovrd.predicate["custom_model_data"] == 0)
                                {
                                    continue;
                                }

                                int customModelNumber = ovrd.predicate["custom_model_data"];
                                string modelPath = ovrd.model;
                                if (string.IsNullOrWhiteSpace(modelPath))
                                    continue;

                                // Use the last segment of the model path as the CMD item name.
                                // This splits on both '/' and '\' in case of different path separators.
                                string[] segments = modelPath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                string cmdName = segments.Last();

                                // Check if a CMD item with the same custom model number already exists.
                                var existingCmd = context.CustomModelDataItems.FirstOrDefault(cmd => cmd.CustomModelNumber == customModelNumber);
                                if (existingCmd != null)
                                {
                                    // Update the model path and name if needed.
                                    if (existingCmd.ModelPath != modelPath)
                                        existingCmd.ModelPath = modelPath;
                                    if (existingCmd.Name != cmdName)
                                        existingCmd.Name = cmdName;

                                    // If the parent item is not already associated, add it.
                                    if (!existingCmd.ParentItems.Any(p => p.ParentItemID == parentItem.ParentItemID))
                                    {
                                        existingCmd.ParentItems.Add(parentItem);
                                    }
                                }
                                else
                                {
                                    // Create a new CustomModelData record.
                                    var newCmd = new CustomModelData
                                    {
                                        Name = cmdName,
                                        CustomModelNumber = customModelNumber,
                                        ModelPath = modelPath,
                                        Status = true
                                    };
                                    newCmd.ParentItems.Add(parentItem);
                                    context.CustomModelDataItems.Add(newCmd);
                                }
                                importedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log errors for this file and continue.
                            System.Diagnostics.Debug.WriteLine("Error importing file " + file + ": " + ex.Message);
                        }
                    }
                    context.SaveChanges();
                }
                System.Windows.MessageBox.Show($"Imported/Updated {importedCount} custom model data item(s) successfully.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error during import: " + ex.Message);
            }
        }

        // Helper classes to map the JSON structure.
        private class ImportJsonModel
        {
            public string parent { get; set; } = string.Empty;
            public Dictionary<string, string>? textures { get; set; }
            public List<ImportOverride>? overrides { get; set; }
        }

        private class ImportOverride
        {
            public Dictionary<string, int> predicate { get; set; } = new Dictionary<string, int>();
            public string model { get; set; } = string.Empty;
        }
    }
}