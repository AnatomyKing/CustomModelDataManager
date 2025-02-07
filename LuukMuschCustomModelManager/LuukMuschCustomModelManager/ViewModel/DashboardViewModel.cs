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
    internal class DashboardViewModel : ObservableObject
    {
        private readonly AppDbContext _context;
        private bool _isDialogOpen;
        private string _searchText = string.Empty;
        private int _highestCustomModelNumber;
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

        void LoadData()
        {
            CustomModelDataItems = new ObservableCollection<CustomModelData>(
                _context.CustomModelDataItems
                    .Where(cmd => cmd.Status)
                    .Include(cmd => cmd.ParentItems)
                    .Include(cmd => cmd.CustomVariations).ThenInclude(cv => cv.BlockType)
                    .Include(cmd => cmd.ShaderArmors).ThenInclude(sa => sa.ShaderArmorColorInfo)
                    .Include(cmd => cmd.BlockTypes).ThenInclude(cmbt => cmbt.BlockType)
                    .ToList());

            // Filter out the unused parent (ID==1) from the ParentItems shown in the dialog.
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

            CustomModelDataItems.CollectionChanged += (_, _) => UpdateHighestCustomModelNumber();
            UpdateHighestCustomModelNumber();
            FilterData();
        }

        void UpdateHighestCustomModelNumber()
        {
            HighestCustomModelNumber = _context.CustomModelDataItems.Any()
                ? _context.CustomModelDataItems.Max(x => x.CustomModelNumber)
                : 0;
        }

        void FilterData()
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
        /// If an unused item exists in the DB, then by default we pass it in (and set isFromUnused true)
        /// so that the toggle is on; but the user may toggle it off to create a new record.
        /// </summary>
        async void OpenAddDialog(object? obj)
        {
            var unusedItem = _context.CustomModelDataItems
                .Where(cmd => !cmd.Status)
                .OrderBy(cmd => cmd.CustomModelNumber)
                .FirstOrDefault();

            CustomModelData newData;
            bool isNew;
            if (unusedItem != null)
            {
                newData = unusedItem;
                // Reset status to used (the user can later uncheck if they want an unused item)
                newData.Status = true;
                isNew = false;
            }
            else
            {
                newData = new CustomModelData
                {
                    CustomModelNumber = HighestCustomModelNumber + 1,
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

            // In add mode, pass isFromUnused = !isNew.
            await OpenDialogAsync(newData, isNew, isEdit: false);
        }

        async void OpenEditDialog(object? obj)
        {
            if (SelectedCustomModelData != null)
            {
                // When editing from dashboard, we force isFromUnused = false.
                await OpenDialogAsync(SelectedCustomModelData, false, isEdit: true);
            }
        }

        bool CanEdit(object? parameter) => SelectedCustomModelData != null;

        /// <summary>
        /// Opens the dialog host.
        /// After the dialog returns, if the dialog was “saved” (result true) then:
        /// – if viewModel.IsNewItem is true then we add the new item to the context and our lists;
        /// – otherwise we update the existing item.
        /// </summary>
        async System.Threading.Tasks.Task OpenDialogAsync(CustomModelData customModelData, bool isNew, bool isEdit)
        {
            if (_isDialogOpen) return;
            _isDialogOpen = true;

            try
            {
                // If editing, force isFromUnused to false.
                bool isFromUnusedParam = isEdit ? false : !isNew;
                var viewModel = new AddEditCMDViewModel(customModelData, ParentItems, BlockTypes, ShaderArmorColorInfos, _context, HighestCustomModelNumber + 1, isFromUnused: isFromUnusedParam, isEdit: isEdit);
                object? result = await DialogHost.Show(viewModel, "RootDialog");

                if (result is true)
                {
                    // Check the viewModel flag to see if a completely new item was created.
                    if (viewModel.IsNewItem)
                    {
                        // Add the new item.
                        CustomModelData finalItem = viewModel.FinalCustomModelData;
                        _context.CustomModelDataItems.Add(finalItem);
                        CustomModelDataItems.Add(finalItem);
                        if (finalItem.Status) // Only show used items in the dashboard.
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
                        // Existing item updated.
                        if (!CustomModelDataItems.Contains(customModelData))
                        {
                            CustomModelDataItems.Add(customModelData);
                        }
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
                SelectedCustomModelData = null;
                SelectedFlattenedItem = null;
            }
        }
    }
}