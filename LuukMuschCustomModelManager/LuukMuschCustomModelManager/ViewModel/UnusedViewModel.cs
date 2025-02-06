using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using MaterialDesignThemes.Wpf;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class UnusedViewModel : ObservableObject
    {
        private readonly AppDbContext _context;
        private bool _isDialogOpen;

        public UnusedViewModel()
        {
            _context = new AppDbContext();
            LoadData();

            EditCommand = new RelayCommand(OpenEditDialog, CanEdit);
            DeleteUnusedCommand = new RelayCommand(DeleteUnusedItem, CanDelete);
        }

        #region Properties

        private ObservableCollection<CustomModelData> _unusedItems = new ObservableCollection<CustomModelData>();
        public ObservableCollection<CustomModelData> UnusedItems
        {
            get => _unusedItems;
            set
            {
                _unusedItems = value;
                OnPropertyChanged();
            }
        }

        private CustomModelData? _selectedUnusedItem;
        public CustomModelData? SelectedUnusedItem
        {
            get => _selectedUnusedItem;
            set
            {
                if (_selectedUnusedItem != value)
                {
                    _selectedUnusedItem = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        OpenEditDialog(null);
                    }
                }
            }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteUnusedCommand { get; }

        #endregion

        #region Methods

        private void LoadData()
        {
            UnusedItems = new ObservableCollection<CustomModelData>(
                _context.CustomModelDataItems
                    .Where(x => !x.Status) // Only load items where Status == false
                    .ToList()
            );
        }

        private bool CanEdit(object? parameter) => SelectedUnusedItem != null;

        private async void OpenEditDialog(object? obj)
        {
            if (_selectedUnusedItem == null || _isDialogOpen) return;
            _isDialogOpen = true;

            try
            {
                var parentItems = new ObservableCollection<ParentItem>(_context.ParentItems.ToList());
                var blockTypes = new ObservableCollection<BlockType>(_context.BlockTypes.ToList());
                var shaderArmorColorInfos = new ObservableCollection<ShaderArmorColorInfo>(_context.ShaderArmorColorInfos.ToList());

                // Note the extra "true" to indicate we're coming from the 'Unused' view.
                var viewModel = new AddEditCMDViewModel(
                    _selectedUnusedItem,
                    parentItems,
                    blockTypes,
                    shaderArmorColorInfos,
                    _context,
                    isFromUnused: true
                );

                var result = await DialogHost.Show(viewModel, "UnusedDialog");

                if (result is true)
                {
                    // If the item has become used, remove it from the UnusedItems collection
                    if (_selectedUnusedItem.Status)
                    {
                        UnusedItems.Remove(_selectedUnusedItem);
                    }

                    // Save changes
                    _context.SaveChanges();
                }
            }
            finally
            {
                _isDialogOpen = false;
                SelectedUnusedItem = null;
            }
        }

        private bool CanDelete(object? parameter) => parameter is CustomModelData;

        private void DeleteUnusedItem(object? parameter)
        {
            if (parameter is CustomModelData item)
            {
                _context.CustomModelDataItems.Remove(item);
                _context.SaveChanges();
                UnusedItems.Remove(item);
            }
        }

        #endregion
    }
}