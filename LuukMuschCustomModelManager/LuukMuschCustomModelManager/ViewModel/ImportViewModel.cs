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
using System.Windows;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ImportViewModel : ObservableObject
    {
        private string _importFolder = @"C:\Users\mrluu\Downloads\ymlfiles";
        /// <summary>
        /// The folder to scan for JSON and YAML files.
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

        /// <summary>
        /// Processes all .json, .yml and .yaml files in the import folder.
        /// For JSON files the original logic is used.
        /// For YAML files, the file is deserialized into helper objects that map
        /// the YAML structure. In YAML:
        /// - The top–level key (e.g. "rubber_room") is used as the CustomModelData name.
        /// - The Pack section provides the item’s ModelPath and CustomModelNumber.
        /// - The material field supplies the parent item (its Name).
        /// - The Mechanics.custom_block section supplies:
        ///     • type (block type; e.g. NOTEBLOCK or STRINGBLOCK),
        ///     • custom_variation (variation number),
        ///     • model (the BlockModelPath).
        /// The BlockData is hard–coded as "imported" for every imported item.
        /// Existing items are updated instead of being duplicated.
        /// </summary>
        private void ExecuteImport(object? parameter)
        {
            int importedCount = 0;
            try
            {
                if (!Directory.Exists(ImportFolder))
                {
                    MessageBox.Show("Import folder does not exist: " + ImportFolder);
                    return;
                }

                // Get all files with .json, .yml, or .yaml extension.
                string[] files = Directory.GetFiles(ImportFolder, "*.*")
                    .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                using (var context = new AppDbContext())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            string extension = Path.GetExtension(file).ToLower();
                            if (extension == ".json")
                            {
                                // ----- Process JSON file -----
                                string jsonText = File.ReadAllText(file);
                                var importData = JsonConvert.DeserializeObject<ImportJsonModel>(jsonText);
                                if (importData == null)
                                    continue;

                                // Skip file if there are no overrides.
                                if (importData.overrides == null || importData.overrides.Count == 0)
                                    continue;

                                // Use the file name (without extension) as the ParentItem name.
                                string parentName = Path.GetFileNameWithoutExtension(file);
                                var parentItem = context.ParentItems.FirstOrDefault(p =>
                                    p.Name.Equals(parentName, StringComparison.OrdinalIgnoreCase) &&
                                    p.Type == "imported");
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
                                    string[] segments = modelPath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                    string cmdName = segments.Last();

                                    var existingCmd = context.CustomModelDataItems.FirstOrDefault(cmd => cmd.CustomModelNumber == customModelNumber);
                                    if (existingCmd != null)
                                    {
                                        if (existingCmd.ModelPath != modelPath)
                                            existingCmd.ModelPath = modelPath;
                                        if (existingCmd.Name != cmdName)
                                            existingCmd.Name = cmdName;

                                        if (!existingCmd.ParentItems.Any(p => p.ParentItemID == parentItem.ParentItemID))
                                        {
                                            existingCmd.ParentItems.Add(parentItem);
                                        }
                                    }
                                    else
                                    {
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
                            else if (extension == ".yml" || extension == ".yaml")
                            {
                                // ----- Process YAML file -----
                                var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(new NullNamingConvention()) // Use property names as-is
                                    .IgnoreUnmatchedProperties() // Ignore extra YAML keys that aren’t mapped
                                    .Build();
                                string yamlText = File.ReadAllText(file);
                                // Deserialize the YAML into a dictionary where each top-level key is the CMD item name.
                                var yamlData = deserializer.Deserialize<Dictionary<string, YamlImportModel>>(yamlText);
                                if (yamlData == null || yamlData.Count == 0)
                                    continue;

                                foreach (var kvp in yamlData)
                                {
                                    // The top-level key (e.g. "rubber_room") becomes the CMD name.
                                    string cmdKey = kvp.Key;
                                    var yamlItem = kvp.Value;

                                    // Get the custom model number and ModelPath from the Pack section.
                                    int customModelNumber = yamlItem.Pack.custom_model_data;
                                    string modelPath = yamlItem.Pack.model;
                                    if (string.IsNullOrWhiteSpace(modelPath))
                                        continue;

                                    // Use the top-level key as the CMD item's name.
                                    string cmdName = cmdKey;

                                    // Use the 'material' field as the ParentItem name.
                                    // Convert the material to lower-case for consistency.
                                    string lowerParentName = yamlItem.material.ToLowerInvariant();

                                    // First check the local cache, then the database.
                                    var parentItem = context.ParentItems.Local
                                        .FirstOrDefault(p => p.Name.Equals(lowerParentName, StringComparison.OrdinalIgnoreCase) &&
                                                              p.Type == "imported");
                                    if (parentItem == null)
                                    {
                                        parentItem = context.ParentItems
                                            .FirstOrDefault(p => p.Name.Equals(lowerParentName, StringComparison.OrdinalIgnoreCase) &&
                                                                 p.Type == "imported");
                                    }
                                    if (parentItem == null)
                                    {
                                        parentItem = new ParentItem
                                        {
                                            Name = lowerParentName,
                                            Type = "imported"
                                        };
                                        context.ParentItems.Add(parentItem);
                                    }

                                    var existingCmd = context.CustomModelDataItems.FirstOrDefault(cmd => cmd.CustomModelNumber == customModelNumber);
                                    if (existingCmd != null)
                                    {
                                        if (existingCmd.ModelPath != modelPath)
                                            existingCmd.ModelPath = modelPath;
                                        if (existingCmd.Name != cmdName)
                                            existingCmd.Name = cmdName;

                                        if (!existingCmd.ParentItems.Any(p => p.ParentItemID == parentItem.ParentItemID))
                                        {
                                            existingCmd.ParentItems.Add(parentItem);
                                        }

                                        // Process custom block info, if present.
                                        if (yamlItem.Mechanics != null && yamlItem.Mechanics.custom_block != null)
                                        {
                                            var customBlock = yamlItem.Mechanics.custom_block;
                                            // Get or create the block type.
                                            var blockType = context.BlockTypes.FirstOrDefault(b =>
                                                b.Name.Equals(customBlock.type, StringComparison.OrdinalIgnoreCase));
                                            if (blockType == null)
                                            {
                                                blockType = new BlockType { Name = customBlock.type };
                                                context.BlockTypes.Add(blockType);
                                            }

                                            // Update or add a custom variation for this block type.
                                            var existingVariation = existingCmd.CustomVariations.FirstOrDefault(v => v.BlockTypeID == blockType.BlockTypeID);
                                            if (existingVariation != null)
                                            {
                                                existingVariation.Variation = customBlock.custom_variation;
                                                existingVariation.BlockModelPath = customBlock.model;
                                                existingVariation.BlockData = "imported"; // Hard-coded block data.
                                            }
                                            else
                                            {
                                                var newVariation = new CustomVariation
                                                {
                                                    Variation = customBlock.custom_variation,
                                                    BlockData = "imported",
                                                    BlockModelPath = customBlock.model,
                                                    BlockType = blockType,
                                                    BlockTypeID = blockType.BlockTypeID,
                                                    CustomModelDataID = existingCmd.CustomModelDataID
                                                };
                                                existingCmd.CustomVariations.Add(newVariation);
                                            }
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

                                        // Process custom block info, if present.
                                        if (yamlItem.Mechanics != null && yamlItem.Mechanics.custom_block != null)
                                        {
                                            var customBlock = yamlItem.Mechanics.custom_block;
                                            var blockType = context.BlockTypes.FirstOrDefault(b =>
                                                b.Name.Equals(customBlock.type, StringComparison.OrdinalIgnoreCase));
                                            if (blockType == null)
                                            {
                                                blockType = new BlockType { Name = customBlock.type };
                                                context.BlockTypes.Add(blockType);
                                            }

                                            var newVariation = new CustomVariation
                                            {
                                                Variation = customBlock.custom_variation,
                                                BlockData = "imported",
                                                BlockModelPath = customBlock.model,
                                                BlockType = blockType,
                                                BlockTypeID = blockType.BlockTypeID,
                                                CustomModelDataID = newCmd.CustomModelDataID
                                            };
                                            newCmd.CustomVariations.Add(newVariation);
                                        }
                                    }
                                    importedCount++;
                                }
                            }
                        }
                        catch (Exception exFile)
                        {
                            System.Diagnostics.Debug.WriteLine("Error importing file " + file + ": " + exFile.Message);
                        }
                    }
                    context.SaveChanges();
                }
                MessageBox.Show($"Imported/Updated {importedCount} custom model data item(s) successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during import: " + ex.Message);
            }
        }

        // ===== Helper Classes for JSON Import =====
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

        // ===== Helper Classes for YAML Import =====
        private class YamlImportModel
        {
            public string itemname { get; set; } = string.Empty;
            public string material { get; set; } = string.Empty;
            public YamlPack Pack { get; set; } = new YamlPack();
            public YamlMechanics Mechanics { get; set; } = new YamlMechanics();
        }

        private class YamlPack
        {
            public bool generate_model { get; set; }
            public string model { get; set; } = string.Empty;
            public int custom_model_data { get; set; }
        }

        private class YamlMechanics
        {
            public YamlCustomBlock custom_block { get; set; } = new YamlCustomBlock();
        }

        private class YamlCustomBlock
        {
            public string type { get; set; } = string.Empty;
            public int custom_variation { get; set; }
            public string model { get; set; } = string.Empty;
        }
    }
}