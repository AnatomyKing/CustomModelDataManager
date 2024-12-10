using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using LuukMuschCustomModelManager.Model;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ExportViewModel : ObservableObject
    {
        #region Fields

        private readonly string _defaultExportPath = @"C:\Users\mrluu\AppData\Roaming\.minecraft\resourcepacks\harambe\assets";

        #endregion

        #region Properties

        private string _exportPath;
        public string ExportPath
        {
            get => _exportPath;
            set
            {
                _exportPath = value;
                OnPropertyChanged();
            }
        }

        private bool _advancedExport;
        public bool AdvancedExport
        {
            get => _advancedExport;
            set
            {
                _advancedExport = value;
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
            AdvancedExport = false; // Default to simple export
        }

        #endregion

        #region Methods

        private void ExportData(object? obj)
        {
            using var context = new AppDbContext();

            // Fetch all data
            var allItems = context.CustomModelDataItems
                .Include(cmd => cmd.ParentItem)
                .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                .Include(cmd => cmd.BlockTypes).ThenInclude(cmbt => cmbt.BlockType)
                .ToList();

            // Group and process items
            var groupedItems = GroupItemsByParent(allItems);

            // Ensure the export directory exists
            Directory.CreateDirectory(ExportPath);
            string filePath = Path.Combine(ExportPath, "export.yml");

            // Write data to file
            WriteExportFile(groupedItems, filePath);

            // Notify user
            MessageBox.Show($"Export completed!\n\nFile: {filePath}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private Dictionary<string, List<CustomModelData>> GroupItemsByParent(IEnumerable<CustomModelData> items)
        {
            return items.GroupBy(i => i.ParentItem?.Name ?? "(No Parent)")
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderBy(i => i.Name).ToList()
                        );
        }

        private void WriteExportFile(Dictionary<string, List<CustomModelData>> groupedItems, string filePath)
        {
            using var writer = new StreamWriter(filePath);

            foreach (var (parentName, items) in groupedItems)
            {
                writer.WriteLine($"{parentName}:");

                foreach (var item in items)
                {
                    string oldFormat = CreateOldFormatLine(item);

                    if (!AdvancedExport)
                    {
                        writer.WriteLine($"  - {oldFormat}");
                    }
                    else
                    {
                        WriteAdvancedExportDetails(writer, item, oldFormat);
                    }
                }

                writer.WriteLine(); // Blank line between groups
            }
        }

        private void WriteAdvancedExportDetails(StreamWriter writer, CustomModelData item, string oldFormat)
        {
            writer.WriteLine($"  - {oldFormat}");
            writer.WriteLine(); // Blank line after old format

            writer.WriteLine($"    custom_model_number: {item.CustomModelNumber}");
            writer.WriteLine($"    status: \"{(item.Status ? "Used" : "Unused")}\"");
            writer.WriteLine($"    model_path: \"{EscapeForYaml(item.ModelPath)}\"");

            WriteBlockTypes(writer, item);
            WriteShaderArmors(writer, item);
            WriteCustomVariations(writer, item);
        }

        private void WriteBlockTypes(StreamWriter writer, CustomModelData item)
        {
            writer.WriteLine("    block_types:");
            var blockTypeNames = item.BlockTypes
                .Select(bt => bt.BlockType?.Name ?? "Unknown")
                .Distinct();
            foreach (var name in blockTypeNames)
            {
                writer.WriteLine($"      - \"{name}\"");
            }
        }

        private void WriteShaderArmors(StreamWriter writer, CustomModelData item)
        {
            writer.WriteLine("    shader_armors:");
            var shaderInfos = item.ShaderArmors
                .Select(sa => sa.ShaderArmorColorInfo)
                .Where(info => info != null)
                .Distinct()
                .ToList();

            if (shaderInfos.Any())
            {
                foreach (var info in shaderInfos)
                {
                    writer.WriteLine($"      - name: \"{EscapeForYaml(info.Name)}\"");
                    writer.WriteLine($"        HEX: \"{info.HEX}\"");
                    writer.WriteLine($"        RGB: \"{info.RGB}\"");
                    writer.WriteLine($"        Decimal: {info.Color}");
                }
            }
        }

        private void WriteCustomVariations(StreamWriter writer, CustomModelData item)
        {
            writer.WriteLine("    custom_variations:");
            var variations = item.CustomVariations
                .OrderBy(cv => cv.BlockType?.Name)
                .ThenBy(cv => cv.Variation)
                .ToList();

            foreach (var variation in variations)
            {
                writer.WriteLine($"      - block_type: \"{variation.BlockType?.Name ?? "Unknown"}\"");
                writer.WriteLine($"        variation: {variation.Variation}");
                writer.WriteLine($"        block_data: \"{EscapeForYaml(variation.BlockData)}\"");
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
                line += $" #CustomVariation = {firstVar.BlockType?.Name ?? "Unknown"} {firstVar.Variation} [{firstVar.BlockData}]";
            }

            var shaderInfos = item.ShaderArmors
                .Select(sa => sa.ShaderArmorColorInfo)
                .Where(info => info != null)
                .Distinct()
                .ToList();

            if (shaderInfos.Any())
            {
                var shaderInfoDetails = shaderInfos
                    .Select(info => $"{info.Name}, {info.HEX}, {info.RGB}, {info.Color}")
                    .ToList();

                line += $" #ShaderInfo = ({string.Join(" | ", shaderInfoDetails)})";
            }

            return line;
        }

        private string EscapeForYaml(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        #endregion
    }
}