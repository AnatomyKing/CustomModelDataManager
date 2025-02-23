using System.Collections.ObjectModel;
using System.Windows.Input;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using System;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using LuukMuschCustomModelManager.View;
using System.Windows;
using ZstdSharp.Unsafe;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics;
using LuukMuschCustomModelManager.ViewModels.Views;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class DashboardViewModel : ObservableObject
    {
        private readonly AppDbContext _context;
        private bool _isDialogOpen;
        private string _searchText = string.Empty;
        private int _highestCustomModelNumber;
        private int _nextAvailableCustomModelNumber;
        private CustomModelData? _selectedCustomModelData;

        public DashboardViewModel()
        {
            _context = new AppDbContext();
            LoadData();
            AddCommand = new RelayCommand(OpenAddDialog);
            EditCommand = new RelayCommand(OpenEditDialog, CanEdit);
        }

        public ObservableCollection<CustomModelData> CustomModelDataItems { get; private set; } = new ObservableCollection<CustomModelData>();
        public ObservableCollection<ParentItem> ParentItems { get; private set; } = new ObservableCollection<ParentItem>();
        public ObservableCollection<BlockType> BlockTypes { get; private set; } = new ObservableCollection<BlockType>();
        public ObservableCollection<ShaderArmorColorInfo> ShaderArmorColorInfos { get; private set; } = new ObservableCollection<ShaderArmorColorInfo>();

        public ObservableCollection<KeyValuePair<string, CustomModelData>> FlattenedCustomModelItems { get; private set; } = new ObservableCollection<KeyValuePair<string, CustomModelData>>();
        public ICollectionView? GroupedCustomModels { get; private set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterData();
                }
            }
        }

        public int HighestCustomModelNumber
        {
            get => _highestCustomModelNumber;
            private set
            {
                if (_highestCustomModelNumber != value)
                {
                    _highestCustomModelNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        // NEW: The next available CMD from 27001 up (no gaps).
        public int NextAvailableCustomModelNumber
        {
            get => _nextAvailableCustomModelNumber;
            private set
            {
                if (_nextAvailableCustomModelNumber != value)
                {
                    _nextAvailableCustomModelNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public CustomModelData? SelectedCustomModelData
        {
            get => _selectedCustomModelData;
            set
            {
                _selectedCustomModelData = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, CustomModelData>? _selectedFlattenedItem;
        public KeyValuePair<string, CustomModelData>? SelectedFlattenedItem
        {
            get => _selectedFlattenedItem;
            set
            {
                _selectedFlattenedItem = value;
                OnPropertyChanged();
                if (value.HasValue)
                {
                    SelectedCustomModelData = value.Value.Value;
                    OpenEditDialog(null);
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }

        private void LoadData()
        {
            // Load "used" CMD items
            CustomModelDataItems = new ObservableCollection<CustomModelData>(
                _context.CustomModelDataItems
                    .Where(cmd => cmd.Status)
                    .Include(cmd => cmd.ParentItems)
                    .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                    .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                    .Include(cmd => cmd.BlockTypes).ThenInclude(cmbt => cmbt.BlockType)
                    .ToList());

            // Filter out the "Unused" parent (ID==1)
            ParentItems = new ObservableCollection<ParentItem>(
                _context.ParentItems.Where(p => p.ParentItemID != 1).ToList());

            BlockTypes = new ObservableCollection<BlockType>(_context.BlockTypes.ToList());
            ShaderArmorColorInfos = new ObservableCollection<ShaderArmorColorInfo>(_context.ShaderArmorColorInfos.ToList());

            FlattenedCustomModelItems.Clear();
            foreach (var cmd in CustomModelDataItems)
            {
                var validParents = cmd.ParentItems.Where(p => p.ParentItemID != 1).ToList();
                if (validParents.Any())
                {
                    foreach (var parent in validParents)
                    {
                        FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>(parent.Name, cmd));
                    }
                }
                else
                {
                    FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>("(No Parent)", cmd));
                }
            }

            GroupedCustomModels = CollectionViewSource.GetDefaultView(FlattenedCustomModelItems);
            GroupedCustomModels.GroupDescriptions.Add(new PropertyGroupDescription("Key"));
            GroupedCustomModels.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
            GroupedCustomModels.SortDescriptions.Add(new SortDescription("Value.Name", ListSortDirection.Ascending));

            // Whenever the collection changes, re-check the highest & next-available numbers
            CustomModelDataItems.CollectionChanged += (_, _) =>
            {
                UpdateHighestCustomModelNumber();
                UpdateNextAvailableCustomModelNumber();
            };

            UpdateHighestCustomModelNumber();
            UpdateNextAvailableCustomModelNumber();
            FilterData();
        }

        private void UpdateHighestCustomModelNumber()
        {
            HighestCustomModelNumber = _context.CustomModelDataItems.Any()
                ? _context.CustomModelDataItems.Max(x => x.CustomModelNumber)
                : 0;
        }

        // NEW: Start from 27001, find the first free.
        private void UpdateNextAvailableCustomModelNumber()
        {
            var usedNumbers = _context.CustomModelDataItems
                .Where(x => x.CustomModelNumber >= 27001)
                .Select(x => x.CustomModelNumber)
                .ToHashSet();

            int candidate = 27001;
            while (usedNumbers.Contains(candidate))
            {
                candidate++;
            }

            NextAvailableCustomModelNumber = candidate;
        }

        private void FilterData()
        {
            if (GroupedCustomModels == null) return;

            GroupedCustomModels.Filter = item =>
            {
                if (item is KeyValuePair<string, CustomModelData> kvp)
                {
                    if (string.IsNullOrEmpty(SearchText))
                        return true;

                    string searchLower = SearchText.ToLower();
                    var cmd = kvp.Value;
                    return cmd.Name.ToLower().Contains(searchLower)
                           || cmd.CustomModelNumber.ToString().Contains(searchLower);
                }
                return false;
            };

            GroupedCustomModels.Refresh();
        }

        /// <summary>
        /// Open the add dialog.
        /// If an unused item exists, we reuse it; else we create brand new with NextAvailableCustomModelNumber.
        /// </summary>
        private async void OpenAddDialog(object? obj)
        {
            var unusedItem = _context.CustomModelDataItems
                .Where(cmd => !cmd.Status)
                .OrderBy(cmd => cmd.CustomModelNumber)
                .FirstOrDefault();

            CustomModelData newData;
            bool isNew;
            if (unusedItem != null)
            {
                // We have an "unused" item, let's reuse it
                newData = unusedItem;
                // Mark it "used" for now (the user can revert in the dialog if they want)
                newData.Status = true;
                isNew = false;
            }
            else
            {
                // Brand-new, gapless from 27001 up
                newData = new CustomModelData
                {
                    CustomModelNumber = NextAvailableCustomModelNumber,
                    Status = true,
                    Name = string.Empty,
                    ModelPath = string.Empty,
                    ParentItems = new List<ParentItem>(),
                    CustomVariations = new List<CustomVariation>(),
                    ShaderArmors = new List<CustomModel_ShaderArmor>(),
                    BlockTypes = new List<CustomModel_BlockType>()
                };
                isNew = true;
            }

            // "isFromUnused" is the inverse of isNew
            await OpenDialogAsync(newData, isNew, isEdit: false);
        }

        private async void OpenEditDialog(object? obj)
        {
            if (SelectedCustomModelData != null)
            {
                // Editing an existing item from the dashboard => isFromUnused = false
                await OpenDialogAsync(SelectedCustomModelData, false, isEdit: true);
            }
        }

        private bool CanEdit(object? parameter) => SelectedCustomModelData != null;

        private async System.Threading.Tasks.Task OpenDialogAsync(CustomModelData customModelData, bool isNew, bool isEdit)
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;

            try
            {
                bool isFromUnusedParam = isEdit ? false : !isNew;

                // Pass the "next free" CMD so the AddEditCMDViewModel can use it if truly new
                var viewModel = new AddEditCMDViewModel(
                    customModelData,
                    ParentItems,
                    BlockTypes,
                    ShaderArmorColorInfos,
                    _context,
                    NextAvailableCustomModelNumber,   // <--- The crucial difference!
                    isFromUnusedParam,
                    isEdit
                );

                object? result = await DialogHost.Show(viewModel, "RootDialog");
                if (result is true)
                {
                    // If the user saved:
                    if (viewModel.IsNewItem)
                    {
                        // Actually add the new item to EF + local collections
                        var finalItem = viewModel.FinalCustomModelData;
                        _context.CustomModelDataItems.Add(finalItem);
                        CustomModelDataItems.Add(finalItem);

                        if (finalItem.Status)
                        {
                            if (finalItem.ParentItems.Any())
                            {
                                foreach (var parent in finalItem.ParentItems)
                                {
                                    if (parent.ParentItemID != 1)
                                        FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>(parent.Name, finalItem));
                                }
                            }
                            else
                            {
                                FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>("(No Parent)", finalItem));
                            }
                        }
                    }
                    else
                    {
                        // Updated an existing item
                        if (!CustomModelDataItems.Contains(customModelData))
                        {
                            CustomModelDataItems.Add(customModelData);
                        }

                        // Remove old references from FlattenedCustomModelItems
                        for (int i = FlattenedCustomModelItems.Count - 1; i >= 0; i--)
                        {
                            if (FlattenedCustomModelItems[i].Value == customModelData)
                                FlattenedCustomModelItems.RemoveAt(i);
                        }

                        if (customModelData.ParentItems.Any())
                        {
                            foreach (var parent in customModelData.ParentItems)
                            {
                                if (parent.ParentItemID != 1)
                                    FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>(parent.Name, customModelData));
                            }
                        }
                        else
                        {
                            FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>("(No Parent)", customModelData));
                        }
                    }

                    _context.SaveChanges();

                    // Update highest & next‐available once again
                    UpdateHighestCustomModelNumber();
                    UpdateNextAvailableCustomModelNumber();
                    GroupedCustomModels?.Refresh();
                }
            }
            finally
            {
                _isDialogOpen = false;
                SelectedCustomModelData = null;
                SelectedFlattenedItem = null;
            }
        }
    }
}