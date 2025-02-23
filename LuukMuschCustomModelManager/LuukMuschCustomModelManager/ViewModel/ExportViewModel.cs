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
using System.Collections.ObjectModel;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    /// <summary>
    /// A small helper model to track each parent name and whether it’s “checked” for JSON export.
    /// </summary>
    internal class ParentWhitelistEntry : ObservableObject
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string ParentName { get; set; } = string.Empty;
    }


    internal class ExportViewModel : ObservableObject
    {
        // Default paths – adjust as needed.
        private readonly string _defaultExportPath = @"C:\Users\mrluu\AppData\Roaming\.minecraft\resourcepacks\harambe";
        private readonly string _defaultJsonExportFolder = @"C:\Users\mrluu\AppData\Roaming\.minecraft\resourcepacks\harambe\assets\minecraft\models\item";

        // ---------------------------
        //  Bound Properties
        // ---------------------------

        private string _exportPath = string.Empty;
        /// <summary>
        /// Where the YAML file goes (e.g. “CustomModelDataHarambe.yml”).
        /// </summary>
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
        /// <summary>
        /// Where the .json files are read/written for your item models.
        /// </summary>
        public string JsonExportFolder
        {
            get => _jsonExportFolder;
            set
            {
                _jsonExportFolder = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The UI uses checkboxes for each discovered parent name.
        /// Only “IsSelected == true” entries will be exported to JSON.
        /// </summary>
        private ObservableCollection<ParentWhitelistEntry> _parentWhitelist = new ObservableCollection<ParentWhitelistEntry>();
        public ObservableCollection<ParentWhitelistEntry> ParentWhitelist => _parentWhitelist;

        // Commands
        public ICommand ExportCommand { get; }
        public ICommand ExportToJsonCommand { get; }

        // ---------------------------
        //  Constructor
        // ---------------------------

        public ExportViewModel()
        {
            // Initialize paths
            ExportPath = _defaultExportPath;
            JsonExportFolder = _defaultJsonExportFolder;

            // Commands
            ExportCommand = new RelayCommand(ExportData);
            ExportToJsonCommand = new RelayCommand(ExportToJson);

            // On creation, load all distinct parent names from the DB into the whitelist
            LoadParentWhitelistFromDb();
        }

        private void LoadParentWhitelistFromDb()
        {
            using var context = new AppDbContext();

            // Gather all distinct parent names in the DB
            var allParents = context.ParentItems
                .OrderBy(p => p.Name)
                .Select(p => p.Name)
                .Distinct()
                .ToList();

            _parentWhitelist.Clear();
            foreach (var pName in allParents)
            {
                // By default, let them be un-checked, or you can set IsSelected=true if you like
                var entry = new ParentWhitelistEntry
                {
                    ParentName = pName,
                    IsSelected = false
                };
                _parentWhitelist.Add(entry);
            }
        }

        // ---------------------------
        //  YAML Export
        // ---------------------------

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

            // Build nested dictionary in memory
            foreach (var outerGroup in groupedItems) // e.g. "food", "imported", etc.
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

            // Serialize to YAML
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            string yamlOutput = serializer.Serialize(exportStructure);

            // Post-process to insert blank lines when stepping out from deeper indentation
            yamlOutput = InsertCustomBlankLines(yamlOutput);

            Directory.CreateDirectory(ExportPath);
            string filePath = Path.Combine(ExportPath, "CustomModelDataHarambe.yml");
            File.WriteAllText(filePath, yamlOutput);

            MessageBox.Show($"Export completed!\n\nFile: {filePath}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ---------------------------
        //  JSON Export
        // ---------------------------

        private void ExportToJson(object? obj)
        {
            // We only export if “IsSelected == true” in the ParentWhitelist
            var selectedParents = ParentWhitelist
                .Where(e => e.IsSelected)
                .Select(e => e.ParentName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (selectedParents.Count == 0)
            {
                MessageBox.Show("No parent items checked in the whitelist. Please select at least one.",
                    "Export JSON", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int updatedJsonFiles = 0;

            using var context = new AppDbContext();

            var allCmdItems = context.CustomModelDataItems
                .Include(cmd => cmd.ParentItems)
                .ToList();

            // We'll gather parents that are "whitelisted" (IsSelected=true)
            // Key: parentName, Value: list of CMDs
            var parentsLookup = new Dictionary<string, List<CustomModelData>>(StringComparer.OrdinalIgnoreCase);

            foreach (var cmd in allCmdItems)
            {
                foreach (var parent in cmd.ParentItems)
                {
                    // Only proceed if it’s one of the chosen parents
                    if (!selectedParents.Contains(parent.Name))
                        continue;

                    string parentName = parent.Name;
                    if (!parentsLookup.ContainsKey(parentName))
                        parentsLookup[parentName] = new List<CustomModelData>();

                    parentsLookup[parentName].Add(cmd);
                }
            }

            // Now we do the .json merge/override logic, but only for those whitelisted parents
            foreach (var kvp in parentsLookup)
            {
                string parentName = kvp.Key;
                var cmdItemsForParent = kvp.Value
                    .OrderBy(c => c.CustomModelNumber) // ensure ascending order by CMD
                    .ToList();

                string jsonPath = Path.Combine(JsonExportFolder, parentName + ".json");

                JObject rootObj;
                bool fileExists = File.Exists(jsonPath);

                if (fileExists)
                {
                    var text = File.ReadAllText(jsonPath);
                    try
                    {
                        rootObj = JObject.Parse(text);
                    }
                    catch
                    {
                        // If parse fails, create a fresh root object
                        rootObj = CreateDefaultJsonRoot(parentName);
                    }
                }
                else
                {
                    // If no file, create a basic skeleton
                    rootObj = CreateDefaultJsonRoot(parentName);
                }

                if (!(rootObj["overrides"] is JArray oldOverrides))
                {
                    oldOverrides = new JArray();
                    rootObj["overrides"] = oldOverrides;
                }

                // We keep “non-CMD overrides” at the top
                var nonCmdOverrides = new List<JObject>();

                // We keep “existing CMD overrides (that match an item in cmdItemsForParent)”
                // then we’ll unify them with “new ones” and re-sort them ascending
                var retainedCmdOverrides = new List<JObject>();
                var foundCmdNumbersInJson = new HashSet<int>();

                foreach (var ovrdToken in oldOverrides)
                {
                    if (ovrdToken is not JObject ovrdObj)
                    {
                        // If it’s not even an object, preserve
                        nonCmdOverrides.Add(new JObject());
                        continue;
                    }

                    var predObj = ovrdObj["predicate"] as JObject;
                    if (predObj == null)
                    {
                        // no "predicate"? Then it's not a CMD override; keep as “non-CMD override”
                        nonCmdOverrides.Add(ovrdObj);
                        continue;
                    }

                    var cmdValToken = predObj["custom_model_data"];
                    if (cmdValToken == null)
                    {
                        // again, no custom_model_data => treat as non-CMD
                        nonCmdOverrides.Add(ovrdObj);
                        continue;
                    }

                    // parse the integer
                    int oldCmdNumber;
                    try
                    {
                        oldCmdNumber = cmdValToken.Value<int>();
                    }
                    catch
                    {
                        // if it can’t parse, skip
                        continue;
                    }

                    // If that CMD # is relevant to one of our items, we keep it
                    var matchingCmd = cmdItemsForParent
                        .FirstOrDefault(c => c.CustomModelNumber == oldCmdNumber);
                    if (matchingCmd == null)
                        continue; // not a relevant override => drop it.

                    foundCmdNumbersInJson.Add(oldCmdNumber);
                    retainedCmdOverrides.Add(ovrdObj);
                }

                // Next, we add any “new” overrides that weren’t in the file
                var newlyAddedCmdOverrides = new List<JObject>();
                foreach (var cmdItem in cmdItemsForParent)
                {
                    if (!foundCmdNumbersInJson.Contains(cmdItem.CustomModelNumber))
                    {
                        var newOverrideObj = new JObject
                        {
                            ["predicate"] = new JObject
                            {
                                ["custom_model_data"] = cmdItem.CustomModelNumber
                            },
                            ["model"] = cmdItem.ModelPath
                        };
                        newlyAddedCmdOverrides.Add(newOverrideObj);
                    }
                }

                // Combine existing CMD overrides + new ones, then sort them ascending by CMD
                var allCmdOverrides = retainedCmdOverrides.Concat(newlyAddedCmdOverrides).ToList();
                allCmdOverrides.Sort((a, b) =>
                {
                    int ca = a["predicate"]?["custom_model_data"]?.Value<int>() ?? 0;
                    int cb = b["predicate"]?["custom_model_data"]?.Value<int>() ?? 0;
                    return ca.CompareTo(cb); // ascending
                });

                // Final overrides = “non-CMD overrides” (preserve at top) + sorted CMD overrides
                var finalOverrides = new List<JObject>();
                finalOverrides.AddRange(nonCmdOverrides);
                finalOverrides.AddRange(allCmdOverrides);

                // write it back
                rootObj["overrides"] = new JArray(finalOverrides);

                // Convert to pretty JSON
                string prettyJson = rootObj.ToString(Formatting.Indented);

                // Optionally do a custom “one-line override” format
                string finalJson = ReformatOverrides(prettyJson);

                Directory.CreateDirectory(JsonExportFolder);
                File.WriteAllText(jsonPath, finalJson);
                updatedJsonFiles++;
            }

            MessageBox.Show($"Updated {updatedJsonFiles} JSON file(s) in '{JsonExportFolder}'.",
                "Export JSON", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Creates a minimal skeleton JSON object for an item that had no .json file yet.
        /// </summary>
        private JObject CreateDefaultJsonRoot(string parentName)
        {
            var newRoot = new JObject
            {
                ["parent"] = "item/generated",
                ["textures"] = new JObject
                {
                    ["layer0"] = "item/" + parentName
                },
                ["overrides"] = new JArray()
            };
            return newRoot;
        }

        // ---------------------------
        //  Grouping (for YAML)
        // ---------------------------

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

        // ---------------------------
        //  Reformatting the “overrides”
        // ---------------------------

        private string ReformatOverrides(string fullIndentedJson)
        {
            var lines = fullIndentedJson.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
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

        // ---------------------------
        //  Custom blank lines in YAML
        // ---------------------------

        private string InsertCustomBlankLines(string yaml)
        {
            var lines = yaml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var result = new List<string>();

            for (int i = 0; i < lines.Length - 1; i++)
            {
                string currentLine = lines[i];
                result.Add(currentLine);

                // Compare indentation with next line
                int currIndent = CountIndent(currentLine);
                int nextIndent = CountIndent(lines[i + 1]);

                // If the next line is at a shallower indentation, we've closed a block
                if (nextIndent < currIndent)
                {
                    // If it's top-level (indent=0), add 2 blank lines
                    if (nextIndent == 0)
                    {
                        result.Add(string.Empty);
                        result.Add(string.Empty);
                    }
                    else
                    {
                        // stepping out to a parent block => 1 blank line
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

        private int CountIndent(string line)
        {
            int count = 0;
            while (count < line.Length && line[count] == ' ')
            {
                count++;
            }
            return count;
        }
    }
}