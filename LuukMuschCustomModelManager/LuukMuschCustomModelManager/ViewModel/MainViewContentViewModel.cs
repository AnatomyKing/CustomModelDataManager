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

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class MainViewContentViewModel : ObservableObject
    {
        private readonly AppDbContext _context;
        private bool _isDialogOpen;
        private string _searchText = string.Empty;
        private int _highestCustomModelNumber;
        private CustomModelData? _selectedCustomModelData;

        public MainViewContentViewModel()
        {
            _context = new AppDbContext();
            LoadData();
            AddCommand = new RelayCommand(OpenAddDialog);
            EditCommand = new RelayCommand(OpenEditDialog, CanEdit);
        }

        #region Properties

        public ObservableCollection<CustomModelData> CustomModelDataItems { get; private set; } = new ObservableCollection<CustomModelData>();
        public ObservableCollection<ParentItem> ParentItems { get; private set; } = new ObservableCollection<ParentItem>();
        public ObservableCollection<BlockType> BlockTypes { get; private set; } = new ObservableCollection<BlockType>();
        public ObservableCollection<ShaderArmorColorInfo> ShaderArmorColorInfos { get; private set; } = new ObservableCollection<ShaderArmorColorInfo>();

        // Flattened collection: one entry per CMD item–parent relationship.
        public ObservableCollection<KeyValuePair<string, CustomModelData>> FlattenedCustomModelItems { get; private set; } = new ObservableCollection<KeyValuePair<string, CustomModelData>>();

        // Grouped view built on the flattened collection.
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

        // This property is used internally (when opening the dialog)
        public CustomModelData? SelectedCustomModelData
        {
            get => _selectedCustomModelData;
            set
            {
                _selectedCustomModelData = value;
                OnPropertyChanged();
            }
        }

        // Selected flattened item from the grouped ListBox.
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

        #endregion

        #region Data Loading and Initialization

        private void LoadData()
        {
            // Load only used CMD items.
            CustomModelDataItems = new ObservableCollection<CustomModelData>(
                _context.CustomModelDataItems
                    .Where(cmd => cmd.Status)
                    .Include(cmd => cmd.ParentItems)
                    .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                    .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                    .Include(cmd => cmd.BlockTypes).ThenInclude(cmbt => cmbt.BlockType)
                    .ToList());

            ParentItems = new ObservableCollection<ParentItem>(_context.ParentItems.ToList());
            BlockTypes = new ObservableCollection<BlockType>(_context.BlockTypes.ToList());
            ShaderArmorColorInfos = new ObservableCollection<ShaderArmorColorInfo>(_context.ShaderArmorColorInfos.ToList());

            // Build the flattened collection.
            FlattenedCustomModelItems.Clear();
            foreach (var cmd in CustomModelDataItems)
            {
                // Exclude the default unused parent (ID == 1) from display.
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

            CustomModelDataItems.CollectionChanged += (_, _) => UpdateHighestCustomModelNumber();
            UpdateHighestCustomModelNumber();
            FilterData();
        }

        #endregion

        #region Command Logic

        private void UpdateHighestCustomModelNumber()
        {
            HighestCustomModelNumber = _context.CustomModelDataItems.Any()
                ? _context.CustomModelDataItems.Max(x => x.CustomModelNumber)
                : 0;
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

        private async void OpenAddDialog(object? obj)
        {
            // Look for the lowest unused item.
            var unusedItem = _context.CustomModelDataItems
                .Where(cmd => !cmd.Status)
                .OrderBy(cmd => cmd.CustomModelNumber)
                .FirstOrDefault();

            CustomModelData newData;
            bool isNew;
            if (unusedItem != null)
            {
                newData = unusedItem;
                newData.Status = true; // Reuse this unused item.
                isNew = false;
            }
            else
            {
                newData = new CustomModelData
                {
                    CustomModelNumber = HighestCustomModelNumber + 1,
                    Status = true
                };
                isNew = true;
            }

            await OpenDialogAsync(newData, isNew);
        }

        private async void OpenEditDialog(object? obj)
        {
            if (SelectedCustomModelData != null)
            {
                await OpenDialogAsync(SelectedCustomModelData, false);
            }
        }

        private bool CanEdit(object? parameter) => SelectedCustomModelData != null;

        #endregion

        #region Dialog Logic

        private async System.Threading.Tasks.Task OpenDialogAsync(CustomModelData customModelData, bool isNew)
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;

            try
            {
                // Pass the current context and lookup collections.
                AddEditCMDViewModel viewModel = new AddEditCMDViewModel(customModelData, ParentItems, BlockTypes, ShaderArmorColorInfos, _context);
                object? result = await DialogHost.Show(viewModel, "RootDialog");

                if (result is true)
                {
                    if (isNew)
                    {
                        // For new items, add them to the context and main view.
                        _context.CustomModelDataItems.Add(customModelData);
                        CustomModelDataItems.Add(customModelData);
                        if (customModelData.ParentItems.Any())
                        {
                            foreach (var parent in customModelData.ParentItems)
                            {
                                FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>(parent.Name, customModelData));
                            }
                        }
                        else
                        {
                            FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>("(No Parent)", customModelData));
                        }
                    }
                    else
                    {
                        // For re-used unused items now marked as used:
                        // If the item is not already in the used collection, add it.
                        if (!CustomModelDataItems.Contains(customModelData))
                        {
                            CustomModelDataItems.Add(customModelData);
                        }
                        // Remove any existing flattened entries for this item.
                        for (int i = FlattenedCustomModelItems.Count - 1; i >= 0; i--)
                        {
                            if (FlattenedCustomModelItems[i].Value == customModelData)
                                FlattenedCustomModelItems.RemoveAt(i);
                        }
                        // Add new flattened entries based on the current ParentItems.
                        if (customModelData.ParentItems.Any())
                        {
                            foreach (var parent in customModelData.ParentItems)
                            {
                                // (Skip default unused parent if somehow still present.)
                                if (parent.ParentItemID != 1)
                                {
                                    FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>(parent.Name, customModelData));
                                }
                            }
                        }
                        else
                        {
                            FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>("(No Parent)", customModelData));
                        }
                    }

                    _context.SaveChanges();
                    UpdateHighestCustomModelNumber();
                    GroupedCustomModels?.Refresh();
                }
            }
            finally
            {
                _isDialogOpen = false;
                // Clear selections so the same item can be reopened.
                SelectedCustomModelData = null;
                SelectedFlattenedItem = null;
            }
        }

        #endregion
    }
}