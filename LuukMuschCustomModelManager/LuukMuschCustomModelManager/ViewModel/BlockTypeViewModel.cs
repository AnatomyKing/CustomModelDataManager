﻿using System;
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
        private readonly AppDbContext _context;

        public BlockTypeViewModel()
        {
            _context = new AppDbContext();
            LoadData();
            AddBlockTypeCommand = new RelayCommand(AddOrUpdateBlockType);
            DeleteBlockTypeCommand = new RelayCommand(DeleteBlockType);
            CancelBlockTypeEditCommand = new RelayCommand(CancelBlockTypeEdit);
        }

        public ObservableCollection<BlockType> BlockTypes { get; private set; } = new();

        private string _newBlockTypeName = string.Empty;
        public string NewBlockTypeName
        {
            get => _newBlockTypeName;
            set { _newBlockTypeName = value; OnPropertyChanged(); }
        }

        private BlockType? _selectedBlockType;
        public BlockType? SelectedBlockType
        {
            get => _selectedBlockType;
            set
            {
                _selectedBlockType = value;
                OnPropertyChanged();
                if (_selectedBlockType != null)
                {
                    NewBlockTypeName = _selectedBlockType.Name;
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

        public string ButtonContent => IsEditing ? "Update Block Type" : "Add Block Type";

        public ICommand AddBlockTypeCommand { get; }
        public ICommand DeleteBlockTypeCommand { get; }
        public ICommand CancelBlockTypeEditCommand { get; }

        private void LoadData()
        {
            BlockTypes = new ObservableCollection<BlockType>(_context.BlockTypes.OrderBy(b => b.Name).ToList());
            OnPropertyChanged(nameof(BlockTypes));
        }

        private void AddOrUpdateBlockType(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewBlockTypeName))
                return;

            if (IsEditing && SelectedBlockType != null)
            {
                SelectedBlockType.Name = NewBlockTypeName.Trim();
                _context.BlockTypes.Update(SelectedBlockType);
            }
            else
            {
                var newBlockType = new BlockType { Name = NewBlockTypeName.Trim() };
                _context.BlockTypes.Add(newBlockType);
            }
            _context.SaveChanges();
            CancelBlockTypeEdit(null);
            LoadData();
        }

        private void DeleteBlockType(object? parameter)
        {
            if (parameter is BlockType blockTypeToDelete)
            {
                _context.BlockTypes.Remove(blockTypeToDelete);
                _context.SaveChanges();
                LoadData();
            }
        }

        private void CancelBlockTypeEdit(object? parameter)
        {
            NewBlockTypeName = string.Empty;
            SelectedBlockType = null;
            IsEditing = false;
        }
    }
}