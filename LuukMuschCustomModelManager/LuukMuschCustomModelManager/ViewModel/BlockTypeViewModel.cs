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
    internal class BlockTypeViewModel : ObservableObject
    {
        #region Fields

        private readonly AppDbContext _context;

        #endregion

        #region Constructor

        public BlockTypeViewModel()
        {
            _context = new AppDbContext();
            LoadData();

            AddBlockTypeCommand = new RelayCommand(AddBlockType);
            DeleteBlockTypeCommand = new RelayCommand(DeleteBlockType);
        }

        #endregion

        #region Properties

        public ObservableCollection<BlockType> BlockTypes { get; private set; } = new ObservableCollection<BlockType>();

        private string _newBlockTypeName = string.Empty;
        public string NewBlockTypeName
        {
            get => _newBlockTypeName;
            set
            {
                if (_newBlockTypeName != value)
                {
                    _newBlockTypeName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddBlockTypeCommand { get; }
        public ICommand DeleteBlockTypeCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all BlockType records from the database into the collection.
        /// </summary>
        private void LoadData()
        {
            BlockTypes = new ObservableCollection<BlockType>(
                _context.BlockTypes.OrderBy(b => b.Name).ToList()
            );
            OnPropertyChanged(nameof(BlockTypes));
        }

        /// <summary>
        /// Adds a new BlockType record to the database.
        /// </summary>
        private void AddBlockType(object? parameter)
        {
            if (!ValidateNewBlockTypeName()) return;

            var newBlockType = new BlockType { Name = NewBlockTypeName.Trim() };

            _context.BlockTypes.Add(newBlockType);
            _context.SaveChanges();

            ResetNewBlockTypeName();
            LoadData();
        }

        /// <summary>
        /// Deletes the selected BlockType record from the database.
        /// </summary>
        private void DeleteBlockType(object? parameter)
        {
            if (parameter is BlockType blockTypeToDelete)
            {
                _context.BlockTypes.Remove(blockTypeToDelete);
                _context.SaveChanges();
                LoadData();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Validates the input for the new BlockType name.
        /// </summary>
        private bool ValidateNewBlockTypeName()
        {
            return !string.IsNullOrWhiteSpace(NewBlockTypeName);
        }

        /// <summary>
        /// Resets the input field for the new BlockType name.
        /// </summary>
        private void ResetNewBlockTypeName()
        {
            NewBlockTypeName = string.Empty;
            OnPropertyChanged(nameof(NewBlockTypeName));
        }

        #endregion
    }
}