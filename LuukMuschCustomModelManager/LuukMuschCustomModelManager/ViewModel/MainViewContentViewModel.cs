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

        // Constructor
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

        private string _searchText = string.Empty;
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

        private int _highestCustomModelNumber;
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

        private CustomModelData? _selectedCustomModelData;
        public CustomModelData? SelectedCustomModelData
        {
            get => _selectedCustomModelData;
            set
            {
                _selectedCustomModelData = value;
                OnPropertyChanged();
                if (value != null)
                {
                    OpenEditDialog(null);
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }

        public ICollectionView? GroupedCustomModels { get; private set; }

        #endregion

        #region Data Loading and Initialization

        private void LoadData()
        {
            // Load data from the database into ObservableCollections
            CustomModelDataItems = new ObservableCollection<CustomModelData>(
                _context.CustomModelDataItems
                    .Where(cmd => cmd.Status)
                    .Include(cmd => cmd.ParentItem)
                    .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                    .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                    .Include(cmd => cmd.BlockTypes).ThenInclude(cmbt => cmbt.BlockType)
                    .ToList());

            ParentItems = new ObservableCollection<ParentItem>(_context.ParentItems.ToList());
            BlockTypes = new ObservableCollection<BlockType>(_context.BlockTypes.ToList());
            ShaderArmorColorInfos = new ObservableCollection<ShaderArmorColorInfo>(_context.ShaderArmorColorInfos.ToList());

            // Initialize grouped and sorted collection view
            GroupedCustomModels = CollectionViewSource.GetDefaultView(CustomModelDataItems);
            GroupedCustomModels.GroupDescriptions.Add(new PropertyGroupDescription("ParentItem.Name"));
            GroupedCustomModels.SortDescriptions.Add(new SortDescription("ParentItem.Name", ListSortDirection.Ascending));
            GroupedCustomModels.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            // Update highest custom model number
            CustomModelDataItems.CollectionChanged += (_, _) => UpdateHighestCustomModelNumber();
            UpdateHighestCustomModelNumber();

            // Apply initial filter
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
                if (item is CustomModelData customModelData)
                {
                    if (string.IsNullOrEmpty(SearchText))
                        return true;

                    string searchLower = SearchText.ToLower();
                    return customModelData.Name.ToLower().Contains(searchLower)
                        || customModelData.CustomModelNumber.ToString().Contains(searchLower);
                }

                return false;
            };

            GroupedCustomModels.Refresh();
        }

        private async void OpenAddDialog(object? obj)
        {
            CustomModelData newData = new CustomModelData
            {
                CustomModelNumber = HighestCustomModelNumber + 1,
                Status = true
            };

            await OpenDialogAsync(newData, true);
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
                AddEditCMDViewModel viewModel = new AddEditCMDViewModel(customModelData, ParentItems, BlockTypes, ShaderArmorColorInfos);
                object? result = await DialogHost.Show(viewModel, "RootDialog");

                if (result is true)
                {
                    if (isNew)
                    {
                        _context.CustomModelDataItems.Add(customModelData);
                        CustomModelDataItems.Add(customModelData);
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