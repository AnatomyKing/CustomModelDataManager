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
            AddParentItemCommand = new RelayCommand(AddParentItem);
            DeleteParentItemCommand = new RelayCommand(DeleteParentItem);
        }

        // Collection of Parent Items for display in the ListBox
        public ObservableCollection<ParentItem> ParentItems { get; private set; } = new();

        // Properties bound to input fields
        public string NewParentItemName { get; set; } = string.Empty;
        public string NewParentItemType { get; set; } = string.Empty;

        // Commands
        public ICommand AddParentItemCommand { get; }
        public ICommand DeleteParentItemCommand { get; }

        // Loads all parent items from the database into the collection
        private void LoadData()
        {
            ParentItems = new ObservableCollection<ParentItem>(_context.ParentItems.OrderBy(p => p.Name).ToList());
            OnPropertyChanged(nameof(ParentItems));
        }

        // Adds a new Parent Item to the database and reloads the list
        private void AddParentItem(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewParentItemName) || string.IsNullOrWhiteSpace(NewParentItemType))
                return;

            var newParent = new ParentItem
            {
                Name = NewParentItemName,
                Type = NewParentItemType
            };

            _context.ParentItems.Add(newParent);
            _context.SaveChanges();

            // Clear the input fields
            NewParentItemName = string.Empty;
            NewParentItemType = string.Empty;

            // Reload the list to reflect the changes
            LoadData();

            OnPropertyChanged(nameof(NewParentItemName));
            OnPropertyChanged(nameof(NewParentItemType));
        }

        // Deletes the selected Parent Item from the database
        private void DeleteParentItem(object? parameter)
        {
            if (parameter is ParentItem parentToDelete)
            {
                _context.ParentItems.Remove(parentToDelete);
                _context.SaveChanges();

                // Reload the list to reflect the changes
                LoadData();
            }
        }
    }
}