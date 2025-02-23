using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ExportViewModel : ObservableObject
    {
        private readonly string _defaultExportPath = @"C:\Users\mrluu\AppData\Roaming\.minecraft\resourcepacks\harambe";
        private readonly string _defaultJsonExportFolder = @"C:\Users\mrluu\Downloads\parentitems";

        private string _exportPath = string.Empty;
        public string ExportPath
        {
            get => _exportPath;
            set
            {
                _exportPath = value;
                OnPropertyChanged();
            }
        }

        private string _jsonExportFolder = string.Empty;
        public string JsonExportFolder
        {
            get => _jsonExportFolder;
            set
            {
                _jsonExportFolder = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExportCommand { get; }
        public ICommand ExportToJsonCommand { get; }

        public ExportViewModel()
        {
            ExportPath = _defaultExportPath;
            JsonExportFolder = _defaultJsonExportFolder;

            ExportCommand = new RelayCommand(ExportData);
            ExportToJsonCommand = new RelayCommand(ExportToJson);
        }

        private void ExportData(object? obj)
        {
            using var context = new AppDbContext();

            var allItems = context.CustomModelDataItems
                .Include(cmd => cmd.ParentItems)
                .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                .Include(cmd => cmd.BlockTypes).ThenInclude(cmbt => cmbt.BlockType)
                .ToList();

            var groupedItems = GroupItemsByParentAndType(allItems);
            var exportStructure = new Dictionary<string, object>();

            // Build nested dictionary
            foreach (var outerGroup in groupedItems) // e.g. "food", "imported"
            {
                var outerMapping = new Dictionary<string, object>();

                foreach (var innerGroup in outerGroup.Value) // e.g. "apple", "melon_slice"
                {
                    var cmdMapping = new Dictionary<string, object>();

                    foreach (var item in innerGroup.Value)
                    {
                        var itemMapping = new Dictionary<string, object>
                        {
                            { "custom_model_data", item.CustomModelNumber },
                            { "item_model_path", item.ModelPath }
                        };

                        // If there's a Variation, store block info
                        var variations = item.CustomVariations
                            .OrderBy(cv => cv.BlockType?.Name)
                            .ThenBy(cv => cv.Variation)
                            .ToList();

                        if (variations.Any())
                        {
                            var firstVar = variations.First();
                            var blockInfo = new Dictionary<string, object>
                            {
                                { "type", firstVar.BlockType?.Name ?? "Unknown" },
                                { "variation", firstVar.Variation },
                                { "blockdata", firstVar.BlockData }
                            };
                            itemMapping["block_info"] = blockInfo;

                            if (!string.IsNullOrWhiteSpace(firstVar.BlockModelPath))
                            {
                                itemMapping["linked_block_model_path"] = firstVar.BlockModelPath;
                            }
                        }

                        // If there's shader info
                        var shaderInfos = item.ShaderArmors
                            .Select(sa => sa.ShaderArmorColorInfo)
                            .Where(si => si != null)
                            .Distinct()
                            .ToList();

                        if (shaderInfos.Any())
                        {
                            var shaderList = new List<Dictionary<string, object>>();
                            foreach (var shader in shaderInfos)
                            {
                                var shaderMapping = new Dictionary<string, object>
                                {
                                    { "name", shader!.Name },
                                    { "hex", shader.HEX },
                                    { "rgb", shader.RGB },
                                    { "color", shader.Color }
                                };
                                shaderList.Add(shaderMapping);
                            }
                            itemMapping["shader_info"] = shaderList;
                        }

                        cmdMapping[item.Name] = itemMapping;
                    }

                    outerMapping[innerGroup.Key] = cmdMapping;
                }

                exportStructure[outerGroup.Key] = outerMapping;
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            string yamlOutput = serializer.Serialize(exportStructure);

            // Post-process to insert blank lines when stepping out from deeper indentation
            yamlOutput = InsertCustomBlankLines(yamlOutput);

            System.IO.Directory.CreateDirectory(ExportPath);
            string filePath = System.IO.Path.Combine(ExportPath, "CustomModelDataHarambe.yml");
            System.IO.File.WriteAllText(filePath, yamlOutput);

            MessageBox.Show($"Export completed!\n\nFile: {filePath}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportToJson(object? obj)
        {
            int updatedJsonFiles = 0;

            using var context = new AppDbContext();

            var allCmdItems = context.CustomModelDataItems
                .Include(cmd => cmd.ParentItems)
                .ToList();

            var parentsLookup = new Dictionary<string, List<CustomModelData>>(StringComparer.OrdinalIgnoreCase);

            // Only create JSON for parent.Type = "imported"
            foreach (var cmd in allCmdItems)
            {
                foreach (var parent in cmd.ParentItems)
                {
                    if (!string.Equals(parent.Type, "imported", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string parentName = parent.Name;
                    if (!parentsLookup.ContainsKey(parentName))
                        parentsLookup[parentName] = new List<CustomModelData>();

                    parentsLookup[parentName].Add(cmd);
                }
            }

            foreach (var kvp in parentsLookup)
            {
                string parentName = kvp.Key;
                var cmdItemsForParent = kvp.Value
                    .OrderBy(c => c.CustomModelNumber)
                    .ToList();

                string jsonPath = System.IO.Path.Combine(JsonExportFolder, parentName + ".json");

                JObject rootObj;
                bool fileExists = System.IO.File.Exists(jsonPath);

                if (fileExists)
                {
                    var text = System.IO.File.ReadAllText(jsonPath);
                    try
                    {
                        rootObj = JObject.Parse(text);
                    }
                    catch
                    {
                        rootObj = new JObject();
                    }
                }
                else
                {
                    rootObj = new JObject
                    {
                        ["parent"] = "item/generated",
                        ["textures"] = new JObject
                        {
                            ["layer0"] = "item/" + parentName
                        }
                    };
                }

                if (!(rootObj["overrides"] is JArray oldOverrides))
                {
                    oldOverrides = new JArray();
                    rootObj["overrides"] = oldOverrides;
                }

                var nonCmdOverrides = new List<JObject>();
                var retainedCmdOverrides = new List<JObject>();
                var foundCmdNumbersInJson = new HashSet<int>();

                foreach (var ovrdToken in oldOverrides)
                {
                    if (ovrdToken is not JObject ovrdObj)
                    {
                        nonCmdOverrides.Add(new JObject());
                        continue;
                    }

                    var predObj = ovrdObj["predicate"] as JObject;
                    if (predObj == null)
                    {
                        nonCmdOverrides.Add(ovrdObj);
                        continue;
                    }

                    var cmdValToken = predObj["custom_model_data"];
                    if (cmdValToken == null)
                    {
                        nonCmdOverrides.Add(ovrdObj);
                        continue;
                    }

                    int oldCmdNumber;
                    try
                    {
                        oldCmdNumber = cmdValToken.Value<int>();
                    }
                    catch
                    {
                        continue;
                    }

                    var matchingCmd = cmdItemsForParent.FirstOrDefault(c => c.CustomModelNumber == oldCmdNumber);
                    if (matchingCmd == null) continue;

                    foundCmdNumbersInJson.Add(oldCmdNumber);
                    retainedCmdOverrides.Add(ovrdObj);
                }

                var newOverrides = new List<JObject>();
                newOverrides.AddRange(nonCmdOverrides);
                newOverrides.AddRange(retainedCmdOverrides);

                // Add missing overrides
                foreach (var cmdItem in cmdItemsForParent)
                {
                    if (!foundCmdNumbersInJson.Contains(cmdItem.CustomModelNumber))
                    {
                        var newOvrd = new JObject
                        {
                            ["predicate"] = new JObject
                            {
                                ["custom_model_data"] = cmdItem.CustomModelNumber
                            },
                            ["model"] = cmdItem.ModelPath
                        };
                        newOverrides.Add(newOvrd);
                    }
                }

                rootObj["overrides"] = new JArray(newOverrides);

                // Convert to pretty JSON
                string prettyJson = rootObj.ToString(Formatting.Indented);
                string finalJson = ReformatOverrides(prettyJson);

                System.IO.File.WriteAllText(jsonPath, finalJson);
                updatedJsonFiles++;
            }

            MessageBox.Show($"Updated {updatedJsonFiles} JSON file(s) in '{JsonExportFolder}'.",
                "Export JSON", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region --- JSON overrides reformatting (unchanged) ---

        private string ReformatOverrides(string fullIndentedJson)
        {
            var lines = fullIndentedJson.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            var result = new List<string>();

            bool inOverridesArray = false;
            var overrideLines = new List<string>();
            int braceBalance = 0;

            JObject root;
            try
            {
                root = JObject.Parse(fullIndentedJson);
            }
            catch
            {
                return fullIndentedJson;
            }
            var overridesToken = root["overrides"] as JArray;
            if (overridesToken == null)
            {
                return fullIndentedJson;
            }
            int totalOverrides = overridesToken.Count;
            int processedCount = 0;

            foreach (var line in lines)
            {
                string trimmed = line.TrimStart();

                if (!inOverridesArray)
                {
                    if (trimmed.StartsWith("\"overrides\": ["))
                    {
                        inOverridesArray = true;
                        result.Add(line);
                        continue;
                    }
                    else
                    {
                        result.Add(line);
                        continue;
                    }
                }
                else
                {
                    if (trimmed.StartsWith("]"))
                    {
                        inOverridesArray = false;

                        if (overrideLines.Count > 0)
                        {
                            processedCount++;
                            var singleLine = CompressOverrideObject(overrideLines, processedCount < totalOverrides);
                            result.Add("\t\t" + singleLine);
                            overrideLines.Clear();
                        }

                        result.Add(line);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    if (trimmed.StartsWith("{"))
                    {
                        if (overrideLines.Count > 0)
                        {
                            processedCount++;
                            var singleLine = CompressOverrideObject(overrideLines, processedCount < totalOverrides);
                            result.Add("\t\t" + singleLine);
                            overrideLines.Clear();
                        }

                        overrideLines.Add(line);
                        braceBalance = 1;
                    }
                    else
                    {
                        overrideLines.Add(line);
                        if (trimmed.Contains('}'))
                        {
                            braceBalance--;
                            if (braceBalance <= 0)
                            {
                                processedCount++;
                                var singleLine = CompressOverrideObject(overrideLines, processedCount < totalOverrides);
                                result.Add("\t\t" + singleLine);
                                overrideLines.Clear();
                            }
                        }
                        if (trimmed.Contains('{'))
                        {
                            braceBalance++;
                        }
                    }
                }
            }

            return string.Join(Environment.NewLine, result);
        }

        private string CompressOverrideObject(List<string> lines, bool addTrailingComma)
        {
            var singleLine = string.Join(" ", lines.Select(l => l.Trim()));
            singleLine = Regex.Replace(singleLine, @"\s+", " ");

            if (addTrailingComma && !singleLine.EndsWith(","))
            {
                if (singleLine.EndsWith("}"))
                {
                    singleLine = singleLine.Insert(singleLine.Length - 1, ",");
                }
            }
            return singleLine;
        }

        #endregion

        #region --- Grouping ---

        private Dictionary<string, Dictionary<string, List<CustomModelData>>> GroupItemsByParentAndType(IEnumerable<CustomModelData> items)
        {
            var groups = new Dictionary<string, Dictionary<string, List<CustomModelData>>>();

            foreach (var item in items)
            {
                if (item.ParentItems.Any())
                {
                    foreach (var parent in item.ParentItems)
                    {
                        string outerKey = parent.Type; // e.g. "food"
                        string innerKey = parent.Name;  // e.g. "apple"

                        if (!groups.ContainsKey(outerKey))
                            groups[outerKey] = new Dictionary<string, List<CustomModelData>>();

                        if (!groups[outerKey].ContainsKey(innerKey))
                            groups[outerKey][innerKey] = new List<CustomModelData>();

                        groups[outerKey][innerKey].Add(item);
                    }
                }
                else
                {
                    const string outerKey = "(No Parent)";
                    const string innerKey = "(No Parent)";

                    if (!groups.ContainsKey(outerKey))
                        groups[outerKey] = new Dictionary<string, List<CustomModelData>>();

                    if (!groups[outerKey].ContainsKey(innerKey))
                        groups[outerKey][innerKey] = new List<CustomModelData>();

                    groups[outerKey][innerKey].Add(item);
                }
            }

            return groups;
        }

        #endregion

        #region --- Custom Blank Lines ---

        /// <summary>
        /// Inserts blank lines when "stepping out" of deeper indentation.
        /// 
        /// Rules:
        ///  - If the next line's indentation is 0 (jumping to a new top-level key), add 2 blank lines.
        ///  - If the next line's indentation is less than the current line's indentation (but not 0),
        ///    add 1 blank line.
        /// 
        /// This reproduces exactly:
        /// 
        /// food:
        ///   apple:
        ///     banana:
        ///       ...
        /// 
        ///   melon_slice:
        ///     fusion_core:
        ///       ...
        /// 
        /// 
        /// imported:
        ///   cauldron:
        ///     cauldron:
        ///       ...
        /// </summary>
        private string InsertCustomBlankLines(string yaml)
        {
            var lines = yaml.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            var result = new List<string>();

            for (int i = 0; i < lines.Length - 1; i++)
            {
                string currentLine = lines[i];
                result.Add(currentLine);

                // Check indentation difference
                int currIndent = CountIndent(currentLine);
                int nextIndent = CountIndent(lines[i + 1]);

                // If the next line is at a shallower indentation, we've closed a block
                if (nextIndent < currIndent)
                {
                    // If it's top-level (indent=0), we add 2 blank lines
                    if (nextIndent == 0)
                    {
                        result.Add(string.Empty);
                        result.Add(string.Empty);
                    }
                    else
                    {
                        // stepping out to a parent block, add 1 blank line
                        result.Add(string.Empty);
                    }
                }
            }

            // Add the final line
            if (lines.Length > 0)
            {
                result.Add(lines[^1]);
            }

            return string.Join("\n", result);
        }

        /// <summary>
        /// Counts how many leading spaces are on a line.
        /// </summary>
        private int CountIndent(string line)
        {
            int count = 0;
            while (count < line.Length && line[count] == ' ')
            {
                count++;
            }
            return count;
        }

        #endregion
    }
}