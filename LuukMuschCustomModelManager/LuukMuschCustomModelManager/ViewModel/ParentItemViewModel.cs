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
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<ParentItem> ParentItems { get; private set; } = new();

        private string _newParentItemName = string.Empty;
        public string NewParentItemName
        {
            get => _newParentItemName;
            set { _newParentItemName = value; OnPropertyChanged(); }
        }

        private string _newParentItemType = string.Empty;
        public string NewParentItemType
        {
            get => _newParentItemType;
            set { _newParentItemType = value; OnPropertyChanged(); }
        }

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

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(ButtonContent)); }
        }

        public string ButtonContent => IsEditing ? "Update Parent Item" : "Add Parent Item";

        public ICommand AddOrUpdateParentItemCommand { get; }
        public ICommand DeleteParentItemCommand { get; }
        public ICommand CancelEditCommand { get; }

        private void LoadData()
        {
            ParentItems = new ObservableCollection<ParentItem>(_context.ParentItems.OrderBy(p => p.Name).ToList());
            OnPropertyChanged(nameof(ParentItems));
        }

        private void AddOrUpdateParentItem(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewParentItemName) || string.IsNullOrWhiteSpace(NewParentItemType))
                return;

            if (IsEditing && SelectedParentItem != null)
            {
                SelectedParentItem.Name = NewParentItemName;
                SelectedParentItem.Type = NewParentItemType;
                _context.ParentItems.Update(SelectedParentItem);
            }
            else
            {
                var newParent = new ParentItem { Name = NewParentItemName, Type = NewParentItemType };
                _context.ParentItems.Add(newParent);
            }
            _context.SaveChanges();
            CancelEdit(null);
            LoadData();
        }

        private void DeleteParentItem(object? parameter)
        {
            if (parameter is ParentItem parentToDelete)
            {
                _context.ParentItems.Remove(parentToDelete);
                _context.SaveChanges();
                LoadData();
            }
        }

        private void CancelEdit(object? parameter)
        {
            NewParentItemName = string.Empty;
            NewParentItemType = string.Empty;
            SelectedParentItem = null;
            IsEditing = false;
        }
    }
}
