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
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ImportViewModel : ObservableObject
    {
        private static readonly Dictionary<string, string> PredefinedMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { "wooden_axe", "axe" },
            { "stone_axe", "axe" },
            { "iron_axe", "axe" },
            { "golden_axe", "axe" },
            { "diamond_axe", "axe" },
            { "netherite_axe", "axe" },

            { "wooden_pickaxe", "pickaxe" },
            { "stone_pickaxe", "pickaxe" },
            { "iron_pickaxe", "pickaxe" },
            { "golden_pickaxe", "pickaxe" },
            { "diamond_pickaxe", "pickaxe" },
            { "netherite_pickaxe", "pickaxe" },

            { "wooden_sword", "sword" },
            { "stone_sword", "sword" },
            { "iron_sword", "sword" },
            { "golden_sword", "sword" },
            { "diamond_sword", "sword" },
            { "netherite_sword", "sword" },

            { "wooden_hoe", "hoe" },
            { "stone_hoe", "hoe" },
            { "iron_hoe", "hoe" },
            { "golden_hoe", "hoe" },
            { "diamond_hoe", "hoe" },
            { "netherite_hoe", "hoe" },

            { "wooden_shovel", "shovel" },
            { "stone_shovel", "shovel" },
            { "iron_shovel", "shovel" },
            { "golden_shovel", "shovel" },
            { "diamond_shovel", "shovel" },
            { "netherite_shovel", "shovel" },

            { "paper",       "customblocks" },
            { "nautilus_shell",       "cosmetics" },
            { "feather",       "furniture" },
            { "apple",       "food" },
            { "melon_slice",       "food" },
            { "golden_carrot",       "food" },
            { "leather_chestplate",       "armor" },
            { "leather_leggings",       "armor" },
            { "leather_boots",       "armor" },
            { "rabbit_foot",       "items" },

            // Add more initial mappings as desired
            };

        // Predefined "skipped" files
        // If "IsSkipped = true", the import code won't import these.
        // The user can see them in the DataGrid, add/remove rows, etc.
        private static readonly string[] PredefinedSkippedFiles = new[]
        {
            "leather_horse_armor",
            "player_head"
            // Add more as needed
        };

        private readonly ObservableCollection<ParentItemMapping> _parentItemMappings
            = new ObservableCollection<ParentItemMapping>();

        private readonly ObservableCollection<SkippedParentFile> _skippedParentFiles
            = new ObservableCollection<SkippedParentFile>();

        private string _importFolder = @"C:\Users\mrluu\Downloads\ymlfiles";
        private string _blockStatesImportFolder = @"C:\Users\mrluu\AppData\Roaming\.minecraft\resourcepacks\harambe\assets\minecraft\blockstates";

        public ImportViewModel()
        {
            // Fill ParentItemMappings from the predefined dictionary
            foreach (var kvp in PredefinedMappings)
            {
                _parentItemMappings.Add(new ParentItemMapping
                {
                    ParentName = kvp.Key,
                    ParentType = kvp.Value
                });
            }

            // Fill SkippedParentFiles from the predefined strings
            // By default, "IsSkipped" is true for each.
            foreach (var skipFile in PredefinedSkippedFiles)
            {
                _skippedParentFiles.Add(new SkippedParentFile
                {
                    FileName = skipFile,
                    IsSkipped = true
                });
            }

            ImportCommand = new RelayCommand(ExecuteImport);
            ImportBlockStatesCommand = new RelayCommand(ExecuteImportBlockStates);
        }

        public ObservableCollection<ParentItemMapping> ParentItemMappings => _parentItemMappings;

        // The user can see which files are set to skip, and can toggle IsSkipped or add new.
        public ObservableCollection<SkippedParentFile> SkippedParentFiles => _skippedParentFiles;

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

        public ICommand ImportCommand { get; }
        public ICommand ImportBlockStatesCommand { get; }

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

                // Collect all relevant files
                string[] files = Directory.GetFiles(ImportFolder, "*.*")
                    .Where(f => f.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".yml", System.StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".yaml", System.StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                using (var context = new AppDbContext())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            // If user has flagged this file for skipping, skip it.
                            string baseName = Path.GetFileNameWithoutExtension(file);
                            if (ShouldSkipFile(baseName))
                            {
                                System.Diagnostics.Debug.WriteLine($"Skipping file '{baseName}' per user setting.");
                                continue;
                            }

                            string extension = Path.GetExtension(file).ToLowerInvariant();

                            if (extension == ".json")
                            {
                                string jsonText = File.ReadAllText(file);
                                var importData = JsonConvert.DeserializeObject<ImportJsonModel>(jsonText);
                                if (importData?.overrides == null || importData.overrides.Count == 0)
                                    continue;

                                string parentName = baseName;
                                string parentType = GetParentTypeForItem(parentName);

                                var parentItem = FindOrCreateParentItem(context, parentName, parentType);

                                foreach (var ovrd in importData.overrides)
                                {
                                    if (ovrd?.predicate == null)
                                        continue;

                                    if (!ovrd.predicate.TryGetValue("custom_model_data", out var cmdVal))
                                        continue;

                                    int customModelNumber;
                                    try
                                    {
                                        customModelNumber = Convert.ToInt32(cmdVal);
                                    }
                                    catch
                                    {
                                        continue;
                                    }

                                    if (customModelNumber == 0)
                                        continue;

                                    string modelPath = ovrd.model;
                                    if (string.IsNullOrWhiteSpace(modelPath))
                                        continue;

                                    string cmdName = modelPath.Split('/', '\\').LastOrDefault() ?? "Unnamed";

                                    var existingCmd = context.CustomModelDataItems
                                        .Include(cmd => cmd.ParentItems)
                                        .FirstOrDefault(cmd => cmd.CustomModelNumber == customModelNumber);

                                    if (existingCmd != null)
                                    {
                                        if (!string.Equals(existingCmd.ModelPath, modelPath, System.StringComparison.OrdinalIgnoreCase))
                                            existingCmd.ModelPath = modelPath;
                                        if (!string.Equals(existingCmd.Name, cmdName, System.StringComparison.OrdinalIgnoreCase))
                                            existingCmd.Name = cmdName;

                                        if (!existingCmd.ParentItems.Any(p =>
                                            p.Name.Equals(parentItem.Name, System.StringComparison.OrdinalIgnoreCase)
                                            && p.Type.Equals(parentItem.Type, System.StringComparison.OrdinalIgnoreCase)))
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
                                string fileNameOnly = Path.GetFileName(file);

                                if (fileNameOnly.Equals("CustomModelDataHarambe.yml", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    var deserializer = new DeserializerBuilder()
                                        .WithNamingConvention(new CamelCaseNamingConvention())
                                        .IgnoreUnmatchedProperties()
                                        .Build();

                                    string yamlText = File.ReadAllText(file);
                                    var topDict = deserializer.Deserialize<Dictionary<string, object>>(yamlText);
                                    if (topDict == null || topDict.Count == 0)
                                        continue;

                                    foreach (var outerKvp in topDict)
                                    {
                                        string parentType = outerKvp.Key;

                                        if (outerKvp.Value is not Dictionary<object, object> parentNameDict)
                                            continue;

                                        foreach (var innerKvp in parentNameDict)
                                        {
                                            string parentName = innerKvp.Key.ToString() ?? "(No Parent)";

                                            if (innerKvp.Value is not Dictionary<object, object> itemDict)
                                                continue;

                                            foreach (var itemKvp in itemDict)
                                            {
                                                string cmdItemName = itemKvp.Key.ToString() ?? "UnnamedItem";

                                                if (itemKvp.Value is not Dictionary<object, object> fields)
                                                    continue;

                                                if (!fields.TryGetValue("custom_model_data", out var cmdValObj))
                                                    continue;

                                                int cmdNumber;
                                                try
                                                {
                                                    cmdNumber = Convert.ToInt32(cmdValObj);
                                                }
                                                catch
                                                {
                                                    continue;
                                                }

                                                if (cmdNumber == 0)
                                                    continue;

                                                string itemModelPath = fields.ContainsKey("item_model_path")
                                                    ? fields["item_model_path"]?.ToString() ?? string.Empty
                                                    : string.Empty;
                                                if (string.IsNullOrWhiteSpace(itemModelPath))
                                                    continue;

                                                var parentItem = FindOrCreateParentItem(context, parentName, parentType);

                                                var existingCmd = context.CustomModelDataItems
                                                    .Include(cmd => cmd.ParentItems)
                                                    .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                                                    .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                                                    .FirstOrDefault(cmd => cmd.CustomModelNumber == cmdNumber);

                                                if (existingCmd != null)
                                                {
                                                    if (!string.Equals(existingCmd.ModelPath, itemModelPath, System.StringComparison.OrdinalIgnoreCase))
                                                        existingCmd.ModelPath = itemModelPath;
                                                    if (!string.Equals(existingCmd.Name, cmdItemName, System.StringComparison.OrdinalIgnoreCase))
                                                        existingCmd.Name = cmdItemName;

                                                    if (!existingCmd.ParentItems.Any(p =>
                                                        p.Name.Equals(parentItem.Name, System.StringComparison.OrdinalIgnoreCase)
                                                        && p.Type.Equals(parentItem.Type, System.StringComparison.OrdinalIgnoreCase)))
                                                    {
                                                        existingCmd.ParentItems.Add(parentItem);
                                                    }
                                                }
                                                else
                                                {
                                                    existingCmd = new CustomModelData
                                                    {
                                                        Name = cmdItemName,
                                                        CustomModelNumber = cmdNumber,
                                                        ModelPath = itemModelPath,
                                                        Status = true
                                                    };
                                                    existingCmd.ParentItems.Add(parentItem);
                                                    context.CustomModelDataItems.Add(existingCmd);
                                                }

                                                // block_info handling
                                                if (fields.TryGetValue("block_info", out var blockInfoObj) &&
                                                    blockInfoObj is Dictionary<object, object> blockInfoDict)
                                                {
                                                    string blockTypeName = blockInfoDict.ContainsKey("type")
                                                        ? blockInfoDict["type"]?.ToString() ?? string.Empty
                                                        : string.Empty;
                                                    string blockData = blockInfoDict.ContainsKey("blockdata")
                                                        ? blockInfoDict["blockdata"]?.ToString() ?? string.Empty
                                                        : string.Empty;
                                                    int blockVariation = 0;
                                                    if (blockInfoDict.ContainsKey("variation"))
                                                    {
                                                        int.TryParse(blockInfoDict["variation"]?.ToString(), out blockVariation);
                                                    }

                                                    string blockModelPath = string.Empty;
                                                    if (fields.TryGetValue("linked_block_model_path", out var linkedPathObj) &&
                                                        linkedPathObj != null)
                                                    {
                                                        blockModelPath = linkedPathObj.ToString() ?? string.Empty;
                                                    }

                                                    if (!string.IsNullOrWhiteSpace(blockTypeName))
                                                    {
                                                        var blockType = FindOrCreateBlockType(context, blockTypeName);

                                                        var existingVariation = existingCmd.CustomVariations
                                                            .FirstOrDefault(v =>
                                                                string.Equals(v.BlockType!.Name, blockType.Name, System.StringComparison.OrdinalIgnoreCase)
                                                                && v.Variation == blockVariation);

                                                        if (existingVariation != null)
                                                        {
                                                            if (!string.IsNullOrWhiteSpace(blockModelPath))
                                                                existingVariation.BlockModelPath = blockModelPath;
                                                            if (!string.IsNullOrWhiteSpace(blockData))
                                                                existingVariation.BlockData = blockData;
                                                        }
                                                        else
                                                        {
                                                            var newVar = new CustomVariation
                                                            {
                                                                Variation = blockVariation,
                                                                BlockData = string.IsNullOrWhiteSpace(blockData) ? "imported" : blockData,
                                                                BlockModelPath = string.IsNullOrWhiteSpace(blockModelPath) ? "imported" : blockModelPath,
                                                                BlockType = blockType,
                                                                BlockTypeID = blockType.BlockTypeID,
                                                                CustomModelDataID = existingCmd.CustomModelDataID
                                                            };
                                                            existingCmd.CustomVariations.Add(newVar);
                                                        }
                                                    }
                                                }

                                                // shader_info
                                                if (fields.TryGetValue("shader_info", out var shaderListObj) &&
                                                    shaderListObj is IEnumerable<object> shaderList)
                                                {
                                                    foreach (var shaderObj in shaderList)
                                                    {
                                                        if (shaderObj is not Dictionary<object, object> shaderDict)
                                                            continue;

                                                        string shaderName = shaderDict.ContainsKey("name")
                                                            ? shaderDict["name"]?.ToString() ?? string.Empty
                                                            : string.Empty;
                                                        string hex = shaderDict.ContainsKey("hex")
                                                            ? shaderDict["hex"]?.ToString() ?? string.Empty
                                                            : string.Empty;
                                                        string rgb = shaderDict.ContainsKey("rgb")
                                                            ? shaderDict["rgb"]?.ToString() ?? string.Empty
                                                            : string.Empty;
                                                        int colorInt = 0;
                                                        if (shaderDict.ContainsKey("color"))
                                                        {
                                                            int.TryParse(shaderDict["color"]?.ToString(), out colorInt);
                                                        }

                                                        if (string.IsNullOrWhiteSpace(shaderName) ||
                                                            string.IsNullOrWhiteSpace(hex) ||
                                                            string.IsNullOrWhiteSpace(rgb))
                                                        {
                                                            continue;
                                                        }

                                                        var existingShader = context.ShaderArmorColorInfos.Local
                                                            .FirstOrDefault(s =>
                                                                s.Name.Equals(shaderName, System.StringComparison.OrdinalIgnoreCase)
                                                                && s.HEX.Equals(hex, System.StringComparison.OrdinalIgnoreCase));

                                                        if (existingShader == null)
                                                        {
                                                            existingShader = context.ShaderArmorColorInfos
                                                                .FirstOrDefault(s =>
                                                                    s.Name.Equals(shaderName, System.StringComparison.OrdinalIgnoreCase)
                                                                    && s.HEX.Equals(hex, System.StringComparison.OrdinalIgnoreCase));

                                                            if (existingShader == null)
                                                            {
                                                                existingShader = new ShaderArmorColorInfo
                                                                {
                                                                    Name = shaderName,
                                                                    HEX = hex,
                                                                    RGB = rgb,
                                                                    Color = colorInt
                                                                };
                                                                context.ShaderArmorColorInfos.Add(existingShader);
                                                            }
                                                        }

                                                        bool alreadyLinked = existingCmd.ShaderArmors
                                                            .Any(sa => sa.ShaderArmorColorInfoID == existingShader.ShaderArmorColorInfoID);

                                                        if (!alreadyLinked)
                                                        {
                                                            var newLink = new CustomModel_ShaderArmor
                                                            {
                                                                CustomModelData = existingCmd,
                                                                CustomModelDataID = existingCmd.CustomModelDataID,
                                                                ShaderArmorColorInfo = existingShader,
                                                                ShaderArmorColorInfoID = existingShader.ShaderArmorColorInfoID
                                                            };
                                                            existingCmd.ShaderArmors.Add(newLink);
                                                        }
                                                    }
                                                }

                                                importedCount++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var deserializer = new DeserializerBuilder()
                                        .WithNamingConvention(new NullNamingConvention())
                                        .IgnoreUnmatchedProperties()
                                        .Build();

                                    string yamlText = File.ReadAllText(file);
                                    var yamlData = deserializer.Deserialize<Dictionary<string, YamlImportModel>>(yamlText);
                                    if (yamlData == null || yamlData.Count == 0)
                                        continue;

                                    foreach (var kvp in yamlData)
                                    {
                                        string cmdKey = kvp.Key;
                                        var yamlItem = kvp.Value;
                                        if (yamlItem == null)
                                            continue;

                                        int customModelNumber = yamlItem.Pack.custom_model_data;
                                        string modelPath = yamlItem.Pack.model;
                                        if (string.IsNullOrWhiteSpace(modelPath))
                                            continue;

                                        string cmdName = cmdKey;
                                        string parentName = yamlItem.material.ToLowerInvariant();
                                        string parentType = GetParentTypeForItem(parentName);

                                        var parentItem = FindOrCreateParentItem(context, parentName, parentType);

                                        var existingCmd = context.CustomModelDataItems
                                            .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                                            .Include(cmd => cmd.ParentItems)
                                            .FirstOrDefault(cmd => cmd.CustomModelNumber == customModelNumber);

                                        if (existingCmd != null)
                                        {
                                            if (!string.Equals(existingCmd.ModelPath, modelPath, System.StringComparison.OrdinalIgnoreCase))
                                                existingCmd.ModelPath = modelPath;
                                            if (!string.Equals(existingCmd.Name, cmdName, System.StringComparison.OrdinalIgnoreCase))
                                                existingCmd.Name = cmdName;

                                            if (!existingCmd.ParentItems.Any(p =>
                                                p.Name.Equals(parentItem.Name, System.StringComparison.OrdinalIgnoreCase)
                                                && p.Type.Equals(parentItem.Type, System.StringComparison.OrdinalIgnoreCase)))
                                            {
                                                existingCmd.ParentItems.Add(parentItem);
                                            }

                                            if (yamlItem.Mechanics?.custom_block != null)
                                            {
                                                var cb = yamlItem.Mechanics.custom_block;
                                                var blockType = FindOrCreateBlockType(context, cb.type);

                                                var existingVariation = existingCmd.CustomVariations
                                                    .FirstOrDefault(v =>
                                                        string.Equals(v.BlockType!.Name, blockType.Name, System.StringComparison.OrdinalIgnoreCase)
                                                        && v.Variation == cb.custom_variation);

                                                if (existingVariation != null)
                                                {
                                                    existingVariation.BlockModelPath = cb.model;
                                                    existingVariation.BlockData = "imported";
                                                }
                                                else
                                                {
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
                                            var newCmd = new CustomModelData
                                            {
                                                Name = cmdName,
                                                CustomModelNumber = customModelNumber,
                                                ModelPath = modelPath,
                                                Status = true
                                            };
                                            newCmd.ParentItems.Add(parentItem);
                                            context.CustomModelDataItems.Add(newCmd);

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
                        }
                        catch (Exception exFile)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error importing file '{file}': {exFile.Message}");
                        }
                    }

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        string errorDetails = $"DB Update Error: {dbEx.Message}\nInner Exception: {dbEx.InnerException?.Message}";
                        MessageBox.Show("Error during SaveChanges: " + errorDetails);
                        System.Diagnostics.Debug.WriteLine(errorDetails);
                        return;
                    }
                }

                MessageBox.Show($"Imported/Updated {importedCount} custom model data item(s) successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during import: " + ex.Message);
            }
        }

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

                string[] files = Directory.GetFiles(BlockStatesImportFolder, "*.json", SearchOption.TopDirectoryOnly);

                using (var context = new AppDbContext())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            string baseName = Path.GetFileNameWithoutExtension(file);
                            if (ShouldSkipFile(baseName))
                            {
                                System.Diagnostics.Debug.WriteLine($"Skipping blockstates file '{baseName}' per user setting.");
                                continue;
                            }

                            string blockPrefix = $"minecraft:{baseName}";

                            string jsonText = File.ReadAllText(file);
                            var rootObj = JObject.Parse(jsonText);

                            var variantsObj = rootObj["variants"] as JObject;
                            if (variantsObj == null) continue;

                            foreach (var variantProperty in variantsObj.Properties())
                            {
                                string blockStateKey = variantProperty.Name;
                                var variantValue = variantProperty.Value as JObject;
                                if (variantValue == null) continue;

                                string? blockModel = variantValue["model"]?.ToString();
                                if (string.IsNullOrWhiteSpace(blockModel))
                                    continue;

                                var existingVar = context.CustomVariations
                                    .FirstOrDefault(v => v.BlockModelPath == blockModel);
                                if (existingVar != null)
                                {
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

        /// <summary>
        /// If this file is listed as "IsSkipped = true" in SkippedParentFiles, we skip it.
        /// </summary>
        private bool ShouldSkipFile(string parentName)
        {
            var match = _skippedParentFiles
                .FirstOrDefault(x => x.FileName.Equals(parentName, System.StringComparison.OrdinalIgnoreCase));
            return (match != null && match.IsSkipped);
        }

        private string GetParentTypeForItem(string parentName)
        {
            var found = _parentItemMappings
                .FirstOrDefault(m => m.ParentName.Equals(parentName, System.StringComparison.OrdinalIgnoreCase));
            if (found != null)
                return found.ParentType;

            return "imported";
        }

        private ParentItem FindOrCreateParentItem(AppDbContext context, string name, string type)
        {
            var parent = context.ParentItems.Local
                .FirstOrDefault(p => p.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)
                                     && p.Type.Equals(type, System.StringComparison.OrdinalIgnoreCase));

            if (parent == null)
            {
                parent = context.ParentItems
                    .FirstOrDefault(p => p.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)
                                         && p.Type.Equals(type, System.StringComparison.OrdinalIgnoreCase));
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

        private BlockType FindOrCreateBlockType(AppDbContext context, string blockTypeName)
        {
            var bt = context.BlockTypes.Local
                .FirstOrDefault(b => b.Name.Equals(blockTypeName, System.StringComparison.OrdinalIgnoreCase));

            if (bt == null)
            {
                bt = context.BlockTypes
                    .FirstOrDefault(b => b.Name.Equals(blockTypeName, System.StringComparison.OrdinalIgnoreCase));
            }

            if (bt == null)
            {
                bt = new BlockType { Name = blockTypeName };
                context.BlockTypes.Add(bt);
            }

            return bt;
        }

        // Helper classes
        private class ImportJsonModel
        {
            public string parent { get; set; } = string.Empty;
            public Dictionary<string, string>? textures { get; set; }
            public List<ImportOverride>? overrides { get; set; }
        }

        private class ImportOverride
        {
            public Dictionary<string, object> predicate { get; set; } = new Dictionary<string, object>();
            public string model { get; set; } = string.Empty;
        }

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

        // Exposed so we can see them in the DataGrid
        internal class ParentItemMapping
        {
            public string ParentName { get; set; } = string.Empty;
            public string ParentType { get; set; } = "imported";
        }

        // Exposed so we can see which files are skipped
        internal class SkippedParentFile
        {
            public string FileName { get; set; } = string.Empty;  // e.g., "debug_item"
            public bool IsSkipped { get; set; } = true;           // user can toggle
        }
    }
}