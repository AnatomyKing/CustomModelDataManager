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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;


namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ImportViewModel : ObservableObject
    {
        // === 1) Existing Import logic for .json / .yml / .yaml ===

        private string _importFolder = @"C:\Users\mrluu\Downloads\ymlfiles";

        private string _blockStatesImportFolder = @"C:\Users\mrluu\AppData\Roaming\.minecraft\resourcepacks\harambe\assets\minecraft\blockstates";
        /// <summary>
        /// The folder to scan for JSON and YAML files (custom model data).
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
            ImportBlockStatesCommand = new RelayCommand(ExecuteImportBlockStates);
        }

        /// <summary>
        /// Processes all .json, .yml, and .yaml files in the ImportFolder
        /// to import or update CustomModelData items and variations.
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

                // Collect all JSON/YAML files
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
                            string extension = Path.GetExtension(file).ToLowerInvariant();

                            if (extension == ".json")
                            {
                                // ----- Process JSON file for overrides -----
                                string jsonText = File.ReadAllText(file);
                                var importData = JsonConvert.DeserializeObject<ImportJsonModel>(jsonText);
                                if (importData == null)
                                    continue;
                                if (importData.overrides == null || importData.overrides.Count == 0)
                                    continue;

                                // Use filename (minus extension) as the ParentItem name (type=imported).
                                string parentName = Path.GetFileNameWithoutExtension(file);
                                var parentItem = FindOrCreateParentItem(context, parentName, "imported");

                                // Process each "override"
                                foreach (var ovrd in importData.overrides)
                                {
                                    if (ovrd?.predicate == null ||
                                        !ovrd.predicate.ContainsKey("custom_model_data") ||
                                        ovrd.predicate["custom_model_data"] == 0)
                                    {
                                        continue; // skip invalid
                                    }

                                    int customModelNumber = ovrd.predicate["custom_model_data"];
                                    string modelPath = ovrd.model;
                                    if (string.IsNullOrWhiteSpace(modelPath))
                                        continue;

                                    // The last segment in the model path is used for the CMD name
                                    string cmdName = modelPath.Split('/', '\\')
                                                             .LastOrDefault() ?? "Unnamed";

                                    // Find or create the CMD by its business key (customModelNumber)
                                    var existingCmd = context.CustomModelDataItems
                                        .Include(cmd => cmd.ParentItems) // so we can check if the parent is linked
                                        .FirstOrDefault(cmd => cmd.CustomModelNumber == customModelNumber);

                                    if (existingCmd != null)
                                    {
                                        // Update if needed
                                        if (!string.Equals(existingCmd.ModelPath, modelPath, StringComparison.OrdinalIgnoreCase))
                                            existingCmd.ModelPath = modelPath;
                                        if (!string.Equals(existingCmd.Name, cmdName, StringComparison.OrdinalIgnoreCase))
                                            existingCmd.Name = cmdName;

                                        // Link parent if not already
                                        if (!existingCmd.ParentItems.Any(p =>
                                            p.Name.Equals(parentItem.Name, StringComparison.OrdinalIgnoreCase)
                                            && p.Type.Equals(parentItem.Type, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            existingCmd.ParentItems.Add(parentItem);
                                        }
                                    }
                                    else
                                    {
                                        // Create new
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
                                    .WithNamingConvention(new NullNamingConvention()) // do not alter property names
                                    .IgnoreUnmatchedProperties() // ignore YAML keys we don't map
                                    .Build();

                                string yamlText = File.ReadAllText(file);
                                // Each top-level key is a separate "YamlImportModel"
                                var yamlData = deserializer.Deserialize<Dictionary<string, YamlImportModel>>(yamlText);
                                if (yamlData == null || yamlData.Count == 0)
                                    continue;

                                foreach (var kvp in yamlData)
                                {
                                    // The key is used as the CMD item name
                                    string cmdKey = kvp.Key;
                                    var yamlItem = kvp.Value;
                                    if (yamlItem == null)
                                        continue;

                                    int customModelNumber = yamlItem.Pack.custom_model_data;
                                    string modelPath = yamlItem.Pack.model;
                                    if (string.IsNullOrWhiteSpace(modelPath))
                                        continue;

                                    // The top-level key becomes the CMD's name
                                    string cmdName = cmdKey;
                                    // material -> parent name
                                    string parentName = yamlItem.material.ToLowerInvariant();
                                    var parentItem = FindOrCreateParentItem(context, parentName, "imported");

                                    // Find or create CustomModelData by customModelNumber
                                    var existingCmd = context.CustomModelDataItems
                                        // Eager-load the variations (and their BlockType) so we don't duplicate
                                        .Include(cmd => cmd.CustomVariations)
                                            .ThenInclude(cv => cv.BlockType)
                                        .Include(cmd => cmd.ParentItems)
                                        .FirstOrDefault(cmd => cmd.CustomModelNumber == customModelNumber);

                                    if (existingCmd != null)
                                    {
                                        // Update basic fields
                                        if (!string.Equals(existingCmd.ModelPath, modelPath, StringComparison.OrdinalIgnoreCase))
                                            existingCmd.ModelPath = modelPath;
                                        if (!string.Equals(existingCmd.Name, cmdName, StringComparison.OrdinalIgnoreCase))
                                            existingCmd.Name = cmdName;

                                        // Link parent if not already
                                        if (!existingCmd.ParentItems.Any(p =>
                                            p.Name.Equals(parentItem.Name, StringComparison.OrdinalIgnoreCase)
                                            && p.Type.Equals(parentItem.Type, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            existingCmd.ParentItems.Add(parentItem);
                                        }

                                        // If there's custom block info, handle it
                                        if (yamlItem.Mechanics?.custom_block != null)
                                        {
                                            var cb = yamlItem.Mechanics.custom_block;
                                            // Find or create a BlockType by name
                                            var blockType = FindOrCreateBlockType(context, cb.type);

                                            // Check if there's an existing Variation by (BlockType.Name, Variation)
                                            var existingVariation = existingCmd.CustomVariations
                                                .FirstOrDefault(v =>
                                                    string.Equals(v.BlockType!.Name, blockType.Name, StringComparison.OrdinalIgnoreCase)
                                                    && v.Variation == cb.custom_variation);

                                            if (existingVariation != null)
                                            {
                                                // Update existing
                                                existingVariation.BlockModelPath = cb.model;
                                                // Hard-coded blockdata before, but we could update it if needed
                                                existingVariation.BlockData = "imported";
                                            }
                                            else
                                            {
                                                // Create a new Variation
                                                var newVar = new CustomVariation
                                                {
                                                    Variation = cb.custom_variation,
                                                    BlockData = "imported",
                                                    BlockModelPath = cb.model,
                                                    BlockType = blockType,
                                                    BlockTypeID = blockType.BlockTypeID,
                                                    CustomModelDataID = existingCmd.CustomModelDataID
                                                };
                                                existingCmd.CustomVariations.Add(newVar);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Create new CMD
                                        var newCmd = new CustomModelData
                                        {
                                            Name = cmdName,
                                            CustomModelNumber = customModelNumber,
                                            ModelPath = modelPath,
                                            Status = true
                                        };
                                        newCmd.ParentItems.Add(parentItem);
                                        context.CustomModelDataItems.Add(newCmd);

                                        // If there's custom block info, handle it
                                        if (yamlItem.Mechanics?.custom_block != null)
                                        {
                                            var cb = yamlItem.Mechanics.custom_block;
                                            var blockType = FindOrCreateBlockType(context, cb.type);

                                            var newVar = new CustomVariation
                                            {
                                                Variation = cb.custom_variation,
                                                BlockData = "imported",
                                                BlockModelPath = cb.model,
                                                BlockType = blockType,
                                                BlockTypeID = blockType.BlockTypeID,
                                                CustomModelDataID = newCmd.CustomModelDataID
                                            };
                                            newCmd.CustomVariations.Add(newVar);
                                        }
                                    }
                                    importedCount++;
                                }
                            }
                        }
                        catch (Exception exFile)
                        {
                            // Log or debug the per-file exception
                            System.Diagnostics.Debug.WriteLine($"Error importing file '{file}': {exFile.Message}");
                        }
                    }

                    // After all files processed, try saving
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        string errorDetails = $"DB Update Error: {dbEx.Message}\nInner Exception: {dbEx.InnerException?.Message}";
                        MessageBox.Show("Error during SaveChanges: " + errorDetails);
                        System.Diagnostics.Debug.WriteLine(errorDetails);
                        return; // stop
                    }
                }

                // Done
                MessageBox.Show($"Imported/Updated {importedCount} custom model data item(s) successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during import: " + ex.Message);
            }
        }

        /// <summary>
        /// Finds or creates a ParentItem by name and type, ignoring case for the name. 
        /// </summary>
        private ParentItem FindOrCreateParentItem(AppDbContext context, string name, string type)
        {
            // Check local first to avoid re-querying DB for newly added parents
            var parent = context.ParentItems.Local
                .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                     && p.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (parent == null)
            {
                // Not in local cache—check DB
                parent = context.ParentItems
                    .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                         && p.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
            }

            if (parent == null)
            {
                parent = new ParentItem
                {
                    Name = name,
                    Type = type
                };
                context.ParentItems.Add(parent);
            }

            return parent;
        }

        /// <summary>
        /// Finds or creates a BlockType by name, ignoring case.
        /// </summary>
        private BlockType FindOrCreateBlockType(AppDbContext context, string blockTypeName)
        {
            // Check local first
            var bt = context.BlockTypes.Local
                .FirstOrDefault(b => b.Name.Equals(blockTypeName, StringComparison.OrdinalIgnoreCase));

            if (bt == null)
            {
                // Not in local, check DB
                bt = context.BlockTypes
                    .FirstOrDefault(b => b.Name.Equals(blockTypeName, StringComparison.OrdinalIgnoreCase));
            }

            if (bt == null)
            {
                bt = new BlockType { Name = blockTypeName };
                context.BlockTypes.Add(bt);
            }

            return bt;
        }

        // === 2) New Block States Import logic ===


        /// <summary>
        /// The folder where note_block.json, tripwire.json, etc. reside for updating blockdata.
        /// </summary>
        public string BlockStatesImportFolder
        {
            get => _blockStatesImportFolder;
            set
            {
                if (_blockStatesImportFolder != value)
                {
                    _blockStatesImportFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ImportBlockStatesCommand { get; }

        /// <summary>
        /// Reads block-state .json files (e.g., note_block.json) from BlockStatesImportFolder.
        /// The file name determines the block name (e.g. "note_block" => "minecraft:note_block").
        /// For each "variant" key, we match by "model" => CustomVariation.BlockModelPath,
        /// and update the Variation's BlockData to "minecraft:note_block[...]".
        /// </summary>
        private void ExecuteImportBlockStates(object? parameter)
        {
            int updatedCount = 0;
            try
            {
                if (!Directory.Exists(BlockStatesImportFolder))
                {
                    MessageBox.Show("BlockStates folder does not exist: " + BlockStatesImportFolder);
                    return;
                }

                // .json files only
                string[] files = Directory.GetFiles(BlockStatesImportFolder, "*.json", SearchOption.TopDirectoryOnly);

                using (var context = new AppDbContext())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            // The base file name (e.g. "note_block") => "minecraft:note_block"
                            string baseName = Path.GetFileNameWithoutExtension(file);
                            string blockPrefix = $"minecraft:{baseName}";

                            string jsonText = File.ReadAllText(file);
                            var rootObj = JObject.Parse(jsonText);

                            // We expect a "variants" property containing multiple states
                            var variantsObj = rootObj["variants"] as JObject;
                            if (variantsObj == null) continue;

                            // Each property in "variants" is something like "instrument=bass,note=21,powered=false"
                            foreach (var variantProperty in variantsObj.Properties())
                            {
                                string blockStateKey = variantProperty.Name; // e.g. "instrument=bass,note=21,powered=false"
                                var variantValue = variantProperty.Value as JObject;
                                if (variantValue == null) continue;

                                // The block model is the "model" property inside
                                string? blockModel = variantValue["model"]?.ToString();
                                if (string.IsNullOrWhiteSpace(blockModel))
                                    continue;

                                // Find an existing variation with that BlockModelPath
                                var existingVar = context.CustomVariations
                                    .FirstOrDefault(v => v.BlockModelPath == blockModel);

                                if (existingVar != null)
                                {
                                    // Example: "minecraft:note_block[instrument=bass,note=21,powered=false]"
                                    existingVar.BlockData = $"{blockPrefix}[{blockStateKey}]";
                                    updatedCount++;
                                }
                            }
                        }
                        catch (Exception exFile)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error importing blockstates file '{file}': {exFile.Message}");
                        }
                    }

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        string errorDetails = $"DB Update Error: {dbEx.Message}\nInner: {dbEx.InnerException?.Message}";
                        MessageBox.Show("Error during BlockStates SaveChanges: " + errorDetails);
                        return;
                    }
                }

                MessageBox.Show($"Updated block data for {updatedCount} variation(s).");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during blockstates import: " + ex.Message);
            }
        }

        #region === Helper Classes for JSON Import ===

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

        #endregion

        #region === Helper Classes for YAML Import ===

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

        #endregion
    }
}