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

        public ObservableCollection<CustomModelData> UnusedItems { get; private set; } = new ObservableCollection<CustomModelData>();

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
                    .Where(x => !x.Status)
                    .ToList()
            );

            OnPropertyChanged(nameof(UnusedItems));
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

                var viewModel = new AddEditCMDViewModel(_selectedUnusedItem, parentItems, blockTypes, shaderArmorColorInfos, _context);
                var result = await DialogHost.Show(viewModel, "UnusedDialog");

                if (result is true)
                {
                    HandleItemStatusChange();
                    _context.SaveChanges();
                }
            }
            finally
            {
                _isDialogOpen = false;
                SelectedUnusedItem = null;
            }
        }

        private void HandleItemStatusChange()
        {
            if (_selectedUnusedItem != null && _selectedUnusedItem.Status)
            {
                UnusedItems.Remove(_selectedUnusedItem);
                OnPropertyChanged(nameof(UnusedItems));
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
                OnPropertyChanged(nameof(UnusedItems));
            }
        }

        #endregion
    }
}