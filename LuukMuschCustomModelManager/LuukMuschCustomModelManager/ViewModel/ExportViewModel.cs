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

            Directory.CreateDirectory(ExportPath);
            string filePath = Path.Combine(ExportPath, "export.yml");

            WriteExportFile(groupedItems, filePath);

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
        /// Writes the export file in a nested YAML-like format.
        /// Example output:
        ///   tools:
        ///     Axe:
        ///       - axe_belmont = CustomModelData: 3349 | NOTE_BLOCK 1 [hhhh] | #belmont = #000007 | 0,0,7 | 7
        ///       - another_item = CustomModelData: 3350 | ... 
        ///     Pickaxe:
        ///       - pick_item = CustomModelData: 3348 | ...
        /// </summary>
        private void WriteExportFile(Dictionary<string, Dictionary<string, List<CustomModelData>>> groupedItems, string filePath)
        {
            using var writer = new StreamWriter(filePath);

            foreach (var outerGroup in groupedItems)
            {
                // Write parent item type.
                writer.WriteLine($"{outerGroup.Key}:");

                foreach (var innerGroup in outerGroup.Value)
                {
                    // Write parent item name indented.
                    writer.WriteLine($"  {innerGroup.Key}:");

                    foreach (var item in innerGroup.Value)
                    {
                        string exportLine = CreateOldFormatLine(item);
                        writer.WriteLine($"    - {exportLine}");
                    }
                    writer.WriteLine();
                }
                writer.WriteLine();
            }
        }

        private string CreateOldFormatLine(CustomModelData item)
        {
            string line = $"{item.Name} = CustomModelData: {item.CustomModelNumber}";

            var variations = item.CustomVariations
                .OrderBy(cv => cv.BlockType?.Name)
                .ThenBy(cv => cv.Variation)
                .ToList();

            if (variations.Any())
            {
                var firstVar = variations.First();
                line += $" | {firstVar.BlockType?.Name ?? "Unknown"} {firstVar.Variation} [{firstVar.BlockData}]";
            }

            var shaderInfos = item.ShaderArmors
                .Select(sa => sa.ShaderArmorColorInfo)
                .Where(info => info != null)
                .Distinct()
                .ToList();

            if (shaderInfos.Any())
            {
                foreach (var info in shaderInfos)
                {
                    line += $" | #{info!.Name} = {info.HEX} | {info.RGB} | {info.Color}";
                }
            }

            return line;
        }

        #endregion
    }
}
