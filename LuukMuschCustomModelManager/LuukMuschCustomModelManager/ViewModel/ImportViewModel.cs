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

                            // Must have an "overrides" array.
                            if (importData.overrides == null || importData.overrides.Count == 0)
                                continue;

                            // Find the first override whose predicate contains a nonzero "custom_model_data" value.
                            var validOverride = importData.overrides.FirstOrDefault(o =>
                                o.predicate != null &&
                                o.predicate.ContainsKey("custom_model_data") &&
                                o.predicate["custom_model_data"] != 0);

                            if (validOverride == null)
                                continue;

                            int customModelNumber = validOverride.predicate["custom_model_data"];
                            string modelPath = validOverride.model;

                            // Use the file name (without extension) as the ParentItem name.
                            string parentName = Path.GetFileNameWithoutExtension(file);

                            // Create or reuse a ParentItem with the given name and a hardcoded type "imported".
                            var existingParent = context.ParentItems.FirstOrDefault(p => p.Name == parentName && p.Type == "imported");
                            ParentItem parentItem;
                            if (existingParent != null)
                            {
                                parentItem = existingParent;
                            }
                            else
                            {
                                parentItem = new ParentItem
                                {
                                    Name = parentName,
                                    Type = "imported"
                                };
                                context.ParentItems.Add(parentItem);
                            }

                            // Create a new CustomModelData record with the imported values.
                            var cmd = new CustomModelData
                            {
                                Name = parentName,
                                CustomModelNumber = customModelNumber,
                                ModelPath = modelPath,
                                Status = true
                            };
                            // Associate the ParentItem with the CustomModelData.
                            cmd.ParentItems.Add(parentItem);

                            context.CustomModelDataItems.Add(cmd);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            // Log errors for this file and continue.
                            System.Diagnostics.Debug.WriteLine("Error importing file " + file + ": " + ex.Message);
                        }
                    }
                    context.SaveChanges();
                }
                System.Windows.MessageBox.Show($"Imported {importedCount} item(s) successfully.");
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