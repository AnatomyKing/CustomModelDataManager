using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ExportViewModel : ObservableObject
    {
        #region Fields

        private readonly string _defaultExportPath = @"C:\Users\mrluu\AppData\Roaming\.minecraft\resourcepacks\harambe";

        #endregion

        #region Properties

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

        public ICommand ExportCommand { get; }

        #endregion

        #region Constructor

        public ExportViewModel()
        {
            ExportPath = _defaultExportPath;
            ExportCommand = new RelayCommand(ExportData);
        }

        #endregion

        #region Methods

        private void ExportData(object? obj)
        {
            using var context = new AppDbContext();

            var allItems = context.CustomModelDataItems
                .Include(cmd => cmd.ParentItems)
                .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                .Include(cmd => cmd.BlockTypes).ThenInclude(cmbt => cmbt.BlockType)
                .ToList();

            // Group items first by parent type and then by parent item name.
            var groupedItems = GroupItemsByParentAndType(allItems);

            // Build a nested object that will be serialized to YAML.
            var exportStructure = new Dictionary<string, object>();

            foreach (var outerGroup in groupedItems)
            {
                // Outer key: parent's type (e.g. "imported")
                var outerMapping = new Dictionary<string, object>();

                foreach (var innerGroup in outerGroup.Value)
                {
                    // Inner key: parent's name (e.g. "paper")
                    var cmdMapping = new Dictionary<string, object>();

                    foreach (var item in innerGroup.Value)
                    {
                        // Build a mapping for each CustomModelData item.
                        var itemMapping = new Dictionary<string, object>
                        {
                            { "custom_model_data", item.CustomModelNumber },
                            { "item_model_path", item.ModelPath }
                        };

                        // Block variation info (if available, take the first variation)
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

                        // Shader info – include as a list if available.
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

                        // Use the CMD item's name as the key.
                        cmdMapping[item.Name] = itemMapping;
                    }

                    // Add the inner mapping under the parent's name.
                    outerMapping[innerGroup.Key] = cmdMapping;
                }

                // Add the outer mapping under the parent's type.
                exportStructure[outerGroup.Key] = outerMapping;
            }

            // Serialize the export structure to YAML.
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            string yamlOutput = serializer.Serialize(exportStructure);

            // Post-process the YAML to insert extra blank lines between groups and between items.
            yamlOutput = InsertBlankLines(yamlOutput);

            Directory.CreateDirectory(ExportPath);
            string filePath = Path.Combine(ExportPath, "CustomModelDataHarambe.yml");
            File.WriteAllText(filePath, yamlOutput);

            MessageBox.Show($"Export completed!\n\nFile: {filePath}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Groups CustomModelData items into a two-level dictionary:
        /// Outer key: parent item type.
        /// Inner key: parent item name.
        /// Value: list of CustomModelData items assigned to that parent.
        /// If an item has no parents, it is grouped under "(No Parent)".
        /// </summary>
        private Dictionary<string, Dictionary<string, List<CustomModelData>>> GroupItemsByParentAndType(IEnumerable<CustomModelData> items)
        {
            var groups = new Dictionary<string, Dictionary<string, List<CustomModelData>>>();

            foreach (var item in items)
            {
                if (item.ParentItems.Any())
                {
                    foreach (var parent in item.ParentItems)
                    {
                        // Use the parent's Type as the outer key and its Name as the inner key.
                        string outerKey = parent.Type;
                        string innerKey = parent.Name;

                        if (!groups.ContainsKey(outerKey))
                            groups[outerKey] = new Dictionary<string, List<CustomModelData>>();

                        if (!groups[outerKey].ContainsKey(innerKey))
                            groups[outerKey][innerKey] = new List<CustomModelData>();

                        groups[outerKey][innerKey].Add(item);
                    }
                }
                else
                {
                    // For items with no parent, group them under "(No Parent)".
                    string outerKey = "(No Parent)";
                    string innerKey = "(No Parent)";

                    if (!groups.ContainsKey(outerKey))
                        groups[outerKey] = new Dictionary<string, List<CustomModelData>>();

                    if (!groups[outerKey].ContainsKey(innerKey))
                        groups[outerKey][innerKey] = new List<CustomModelData>();

                    groups[outerKey][innerKey].Add(item);
                }
            }

            return groups;
        }

        /// <summary>
        /// Inserts extra blank lines into the YAML string so that groups are clearly separated.
        /// This method:
        ///   - Inserts a blank line immediately after lines ending with ":" at indent levels 0 or 2.
        ///   - Inserts a blank line before a line at indent level 4 if the previous line was indented at level 6 or more.
        /// Adjust the logic as needed.
        /// </summary>
        private string InsertBlankLines(string input)
        {
            var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
            var resultLines = new List<string>();

            for (int i = 0; i < lines.Count; i++)
            {
                string currentLine = lines[i];
                int currIndent = currentLine.TakeWhile(char.IsWhiteSpace).Count();

                // If this line is at indent level 4 (an item key) and the previous line was indented at 6 or more,
                // insert a blank line before the current line.
                if (i > 0)
                {
                    int prevIndent = lines[i - 1].TakeWhile(char.IsWhiteSpace).Count();
                    if (currIndent == 4 && prevIndent >= 6)
                    {
                        resultLines.Add(string.Empty);
                    }
                }

                resultLines.Add(currentLine);

                // If current line ends with ":" and indent is 0 or 2, then add a blank line after it.
                if (currentLine.TrimEnd().EndsWith(":") && (currIndent == 0 || currIndent == 2))
                {
                    resultLines.Add(string.Empty);
                }
            }

            return string.Join(Environment.NewLine, resultLines);
        }

        #endregion
    }
}