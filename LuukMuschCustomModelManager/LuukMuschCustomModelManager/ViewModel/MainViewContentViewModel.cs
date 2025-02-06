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
        public ObservableCollection<KeyValuePair<string, CustomModelData>> FlattenedCustomModelItems { get; private set; }
            = new ObservableCollection<KeyValuePair<string, CustomModelData>>();

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

        // New property: the SelectedFlattenedItem from the grouped ListBox.
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
                    // Set the underlying CMD item and open the edit dialog.
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
            // Load used CMD items from the database.
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
                if (cmd.ParentItems.Any())
                {
                    foreach (var parent in cmd.ParentItems)
                    {
                        FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>(parent.Name, cmd));
                    }
                }
                else
                {
                    FlattenedCustomModelItems.Add(new KeyValuePair<string, CustomModelData>("(No Parent)", cmd));
                }
            }

            // Initialize the grouped view.
            GroupedCustomModels = CollectionViewSource.GetDefaultView(FlattenedCustomModelItems);
            GroupedCustomModels.GroupDescriptions.Add(new PropertyGroupDescription("Key"));
            GroupedCustomModels.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
            GroupedCustomModels.SortDescriptions.Add(new SortDescription("Value.Name", ListSortDirection.Ascending));

            // Update highest CMD number.
            CustomModelDataItems.CollectionChanged += (_, _) => UpdateHighestCustomModelNumber();
            UpdateHighestCustomModelNumber();

            // Apply the initial filter.
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
            // First, check if any unused CMD items exist.
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
                SelectedCustomModelData = null;
            }
        }

        private bool CanEdit(object? parameter)
        {
            return SelectedCustomModelData != null;
        }

        #endregion

        #region Dialog Logic

        private async Task OpenDialogAsync(CustomModelData customModelData, bool isNew)
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;

            try
            {
                // Pass the current _context into the AddEditCMDViewModel.
                AddEditCMDViewModel viewModel = new AddEditCMDViewModel(customModelData, ParentItems, BlockTypes, ShaderArmorColorInfos, _context);
                object? result = await DialogHost.Show(viewModel, "RootDialog");

                if (result is true)
                {
                    if (isNew)
                    {
                        _context.CustomModelDataItems.Add(customModelData);
                        CustomModelDataItems.Add(customModelData);
                        // Also add entries to the flattened collection.
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
                    else if (!customModelData.Status)
                    {
                        CustomModelDataItems.Remove(customModelData);
                    }

                    _context.SaveChanges();
                    UpdateHighestCustomModelNumber();
                    GroupedCustomModels?.Refresh();
                }
            }
            finally
            {
                _isDialogOpen = false;
                SelectedCustomModelData = null;
            }
        }

        #endregion
    }
}