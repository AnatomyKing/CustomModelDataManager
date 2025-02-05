using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using System.Windows.Input;


namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class ParentItemViewModel : ObservableObject
    {
        private readonly AppDbContext _context;

        public ParentItemViewModel()
        {
            _context = new AppDbContext();
            LoadData();
            AddOrUpdateParentItemCommand = new RelayCommand(AddOrUpdateParentItem);
            DeleteParentItemCommand = new RelayCommand(DeleteParentItem);
        }

        // Collection of Parent Items for display in the ListBox.
        public ObservableCollection<ParentItem> ParentItems { get; private set; } = new();

        // Bound to the "Parent Item Name" TextBox.
        private string _newParentItemName = string.Empty;
        public string NewParentItemName
        {
            get => _newParentItemName;
            set
            {
                _newParentItemName = value;
                OnPropertyChanged();
            }
        }

        // Bound to the "Parent Item Type" TextBox.
        private string _newParentItemType = string.Empty;
        public string NewParentItemType
        {
            get => _newParentItemType;
            set
            {
                _newParentItemType = value;
                OnPropertyChanged();
            }
        }

        // The currently selected ParentItem in the ListBox.
        private ParentItem? _selectedParentItem;
        public ParentItem? SelectedParentItem
        {
            get => _selectedParentItem;
            set
            {
                _selectedParentItem = value;
                OnPropertyChanged();

                if (_selectedParentItem != null)
                {
                    // Fill the text boxes with the selected item's values.
                    NewParentItemName = _selectedParentItem.Name;
                    NewParentItemType = _selectedParentItem.Type;
                    IsEditing = true;
                }
                else
                {
                    IsEditing = false;
                }
                OnPropertyChanged(nameof(ButtonContent));
            }
        }

        // Tracks whether we're editing an existing item.
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ButtonContent));
            }
        }

        // Button content changes based on editing state.
        public string ButtonContent => IsEditing ? "Update Parent Item" : "Add Parent Item";

        // Commands
        public ICommand AddOrUpdateParentItemCommand { get; }
        public ICommand DeleteParentItemCommand { get; }

        // Loads ParentItems from the database.
        private void LoadData()
        {
            ParentItems = new ObservableCollection<ParentItem>(
                _context.ParentItems.OrderBy(p => p.Name).ToList());
            OnPropertyChanged(nameof(ParentItems));
        }

        // Adds a new ParentItem or updates an existing one.
        private void AddOrUpdateParentItem(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewParentItemName) ||
                string.IsNullOrWhiteSpace(NewParentItemType))
                return;

            if (IsEditing && SelectedParentItem != null)
            {
                // Update the existing item.
                SelectedParentItem.Name = NewParentItemName;
                SelectedParentItem.Type = NewParentItemType;
                _context.ParentItems.Update(SelectedParentItem);
            }
            else
            {
                // Add a new item.
                var newParent = new ParentItem
                {
                    Name = NewParentItemName,
                    Type = NewParentItemType
                };
                _context.ParentItems.Add(newParent);
            }
            _context.SaveChanges();

            // Clear fields and reset editing state.
            NewParentItemName = string.Empty;
            NewParentItemType = string.Empty;
            SelectedParentItem = null;
            IsEditing = false;

            // Reload list.
            LoadData();
        }

        // Deletes the specified ParentItem.
        private void DeleteParentItem(object? parameter)
        {
            if (parameter is ParentItem parentToDelete)
            {
                _context.ParentItems.Remove(parentToDelete);
                _context.SaveChanges();
                LoadData();
            }
        }
    }
}