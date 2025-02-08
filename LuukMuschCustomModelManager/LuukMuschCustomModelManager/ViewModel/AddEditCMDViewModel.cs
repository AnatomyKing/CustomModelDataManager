using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using System.Windows;
using LuukMuschCustomModelManager.Databases;
using System.Collections;
using System.ComponentModel;

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class AddEditCMDViewModel : ObservableObject
    {
        private CustomModelData _originalCustomModelData;
        private readonly AppDbContext _context;
        private readonly int _newModelNumber;
        private CustomModelData? _unusedData;
        private bool _useUnused;
        private readonly bool _initialIsFromUnused;

        public bool IsNewItem { get; private set; }
        public CustomModelData FinalCustomModelData => _originalCustomModelData;

        public bool IsAddMode { get; }

        public string ToggleLabelText => IsAddMode ? "Enable Unused:" : "Enable Edit:";

        public bool IsToggleEnabled => IsAddMode ? true : !_initialIsFromUnused;
        public bool IsStatusEditable => !UseUnused;
        public bool IsCustomModelNumberEditable => !UseUnused;

        public bool UseUnused
        {
            get => _useUnused;
            set
            {
                if (_useUnused != value)
                {
                    _useUnused = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStatusEditable));
                    OnPropertyChanged(nameof(IsCustomModelNumberEditable));
                    if (IsAddMode)
                        ToggleUnusedItem();
                }
            }
        }

        public AddEditCMDViewModel(CustomModelData customModelData,
            ObservableCollection<ParentItem> parentItems,
            ObservableCollection<BlockType> blockTypes,
            ObservableCollection<ShaderArmorColorInfo> shaderArmorColorInfos,
            AppDbContext context,
            int newModelNumber,
            bool isFromUnused,
            bool isEdit)
        {
            IsAddMode = !isEdit;
            _context = context;
            _newModelNumber = newModelNumber;
            _initialIsFromUnused = isFromUnused;

            if (IsAddMode)
            {
                if (isFromUnused)
                {
                    _unusedData = customModelData;
                    _originalCustomModelData = customModelData;
                    _useUnused = true;
                    IsNewItem = false;
                    EditedCustomModelData = CreateEditableCopy(_unusedData, copyCollections: true);
                }
                else
                {
                    _useUnused = false;
                    IsNewItem = true;
                    _originalCustomModelData = new CustomModelData
                    {
                        CustomModelNumber = newModelNumber,
                        Status = true,
                        Name = string.Empty,
                        ModelPath = string.Empty,
                        ParentItems = new List<ParentItem>(),
                        CustomVariations = new List<CustomVariation>(),
                        ShaderArmors = new List<CustomModel_ShaderArmor>(),
                        BlockTypes = new List<CustomModel_BlockType>()
                    };
                    EditedCustomModelData = CreateEditableCopy(_originalCustomModelData, copyCollections: false);
                }
            }
            else
            {
                _originalCustomModelData = customModelData;
                _useUnused = true;
                IsNewItem = false;
                EditedCustomModelData = CreateEditableCopy(customModelData, copyCollections: true);
            }

            ParentItems = parentItems;
            BlockTypes = blockTypes;
            ShaderArmorColorInfos = shaderArmorColorInfos;

            foreach (var parent in ParentItems)
            {
                parent.PropertyChanged += ParentItem_PropertyChanged;
            }

            CancelCommand = new RelayCommand(Cancel);
            SaveCommand = new RelayCommand(Save);
            ClearArmorInfoCommand = new RelayCommand(ClearArmorInfo);
            ClearBlockInfoCommand = new RelayCommand(ClearBlockInfo);

            CustomVariations = new ObservableCollection<CustomVariation>(EditedCustomModelData.CustomVariations);
            _selectedParentItems = new ObservableCollection<ParentItem>();

            PreSelectProperties();

            // In edit mode, if the original item was unused then the fields remain read-only.
            if (!IsAddMode && !_originalCustomModelData.Status)
            {
                EditedCustomModelData.Status = true;
                OnPropertyChanged(nameof(Status));
            }
        }

        private void ToggleUnusedItem()
        {
            if (UseUnused)
            {
                if (_unusedData != null)
                {
                    // Switch back to using the unused item from DB.
                    _originalCustomModelData = _unusedData;
                    IsNewItem = false;
                    EditedCustomModelData = CreateEditableCopy(_unusedData, copyCollections: true);
                }
                else
                {
                    // No unused item exists – create a new entity (treated as new).
                    _originalCustomModelData = new CustomModelData
                    {
                        CustomModelNumber = _newModelNumber,
                        Status = true,
                        Name = string.Empty,
                        ModelPath = string.Empty,
                        ParentItems = new List<ParentItem>(),
                        CustomVariations = new List<CustomVariation>(),
                        ShaderArmors = new List<CustomModel_ShaderArmor>(),
                        BlockTypes = new List<CustomModel_BlockType>()
                    };
                    IsNewItem = true;
                    EditedCustomModelData = CreateEditableCopy(_originalCustomModelData, copyCollections: false);
                }
            }
            else
            {
                // User does not want to reuse an existing unused item.
                // Create a completely new entity.
                _originalCustomModelData = new CustomModelData
                {
                    CustomModelNumber = _newModelNumber,
                    Status = true,
                    Name = string.Empty,
                    ModelPath = string.Empty,
                    ParentItems = new List<ParentItem>(),
                    CustomVariations = new List<CustomVariation>(),
                    ShaderArmors = new List<CustomModel_ShaderArmor>(),
                    BlockTypes = new List<CustomModel_BlockType>()
                };
                IsNewItem = true;
                EditedCustomModelData = CreateEditableCopy(_originalCustomModelData, copyCollections: false);
            }
            PreSelectProperties();
            OnPropertyChanged(nameof(EditedCustomModelData));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(ModelPath));
            OnPropertyChanged(nameof(CustomModelNumber));
            OnPropertyChanged(nameof(Status));
        }

        public CustomModelData EditedCustomModelData { get; set; }
        public ObservableCollection<ParentItem> ParentItems { get; }
        public ObservableCollection<BlockType> BlockTypes { get; }
        public ObservableCollection<ShaderArmorColorInfo> ShaderArmorColorInfos { get; }
        public ObservableCollection<CustomVariation> CustomVariations { get; }

        private ObservableCollection<ParentItem> _selectedParentItems;
        public ObservableCollection<ParentItem> SelectedParentItems
        {
            get => _selectedParentItems;
            set
            {
                _selectedParentItems = value;
                OnPropertyChanged();
            }
        }

        private BlockType? _selectedBlockType;
        public BlockType? SelectedBlockType
        {
            get => _selectedBlockType;
            set
            {
                if (_selectedBlockType != value)
                {
                    _selectedBlockType = value;
                    OnPropertyChanged();
                    UpdateNewVariationNumber();
                }
            }
        }

        private ShaderArmorColorInfo? _selectedShaderArmorColorInfo;
        public ShaderArmorColorInfo? SelectedShaderArmorColorInfo
        {
            get => _selectedShaderArmorColorInfo;
            set
            {
                _selectedShaderArmorColorInfo = value;
                OnPropertyChanged();
            }
        }

        private string _newBlockData = string.Empty;
        public string NewBlockData
        {
            get => _newBlockData;
            set
            {
                if (_newBlockData != value)
                {
                    _newBlockData = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _newVariationNumber;
        public int NewVariationNumber
        {
            get => _newVariationNumber;
            set
            {
                if (_newVariationNumber != value)
                {
                    _newVariationNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        // New property for Block Model Path
        private string _newBlockModelPath = string.Empty;
        public string NewBlockModelPath
        {
            get => _newBlockModelPath;
            set
            {
                if (_newBlockModelPath != value)
                {
                    _newBlockModelPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => EditedCustomModelData.Name;
            set
            {
                if (EditedCustomModelData.Name != value)
                {
                    EditedCustomModelData.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ModelPath
        {
            get => EditedCustomModelData.ModelPath;
            set
            {
                if (EditedCustomModelData.ModelPath != value)
                {
                    EditedCustomModelData.ModelPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CustomModelNumber
        {
            get => EditedCustomModelData.CustomModelNumber;
            set
            {
                if (EditedCustomModelData.CustomModelNumber != value)
                {
                    EditedCustomModelData.CustomModelNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Status
        {
            get => EditedCustomModelData.Status;
            set
            {
                if (EditedCustomModelData.Status != value)
                {
                    EditedCustomModelData.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearArmorInfoCommand { get; }
        public ICommand ClearBlockInfoCommand { get; }

        private void ParentItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ParentItem parent && e.PropertyName == nameof(ParentItem.IsSelected))
            {
                if (parent.IsSelected)
                {
                    if (!SelectedParentItems.Contains(parent))
                        SelectedParentItems.Add(parent);
                }
                else
                {
                    if (SelectedParentItems.Contains(parent))
                        SelectedParentItems.Remove(parent);
                }
            }
        }

        private void Cancel(object? obj)
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        private void Save(object? obj)
        {
            UpdateOriginalData();
            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        private void ClearBlockInfo(object? obj)
        {
            NewBlockData = string.Empty;
            NewBlockModelPath = string.Empty;
            SelectedBlockType = null;
        }

        private void ClearArmorInfo(object? obj)
        {
            SelectedShaderArmorColorInfo = null;
        }

        // If copyCollections is false, join collections will be created empty.
        private CustomModelData CreateEditableCopy(CustomModelData original, bool copyCollections)
        {
            return new CustomModelData
            {
                Name = original.Name,
                ModelPath = original.ModelPath,
                CustomModelNumber = original.CustomModelNumber,
                Status = original.Status,
                ParentItems = copyCollections ? new List<ParentItem>(original.ParentItems) : new List<ParentItem>(),
                CustomVariations = copyCollections ? new List<CustomVariation>(original.CustomVariations) : new List<CustomVariation>(),
                ShaderArmors = copyCollections ? new List<CustomModel_ShaderArmor>(original.ShaderArmors) : new List<CustomModel_ShaderArmor>(),
                BlockTypes = copyCollections ? new List<CustomModel_BlockType>(original.BlockTypes) : new List<CustomModel_BlockType>()
            };
        }

        private void PreSelectProperties()
        {
            // Clear current selection for a fresh start.
            SelectedParentItems.Clear();
            foreach (var parent in ParentItems)
            {
                parent.IsSelected = false;
            }

            // Now preselect the ParentItems that belong to the current item.
            foreach (var parent in EditedCustomModelData.ParentItems)
            {
                // Skip the behind–the–scenes "Unused" parent (ID==1)
                if (EditedCustomModelData.Status && parent.ParentItemID == 1)
                    continue;
                parent.IsSelected = true;
                SelectedParentItems.Add(parent);
            }

            if (EditedCustomModelData.ShaderArmors.Any())
            {
                SelectedShaderArmorColorInfo = ShaderArmorColorInfos
                    .FirstOrDefault(s => EditedCustomModelData.ShaderArmors
                        .Any(sa => sa.ShaderArmorColorInfoID == s.ShaderArmorColorInfoID));
            }

            var firstVariation = CustomVariations.FirstOrDefault();
            if (firstVariation != null)
            {
                SelectedBlockType = BlockTypes.FirstOrDefault(b => b.BlockTypeID == firstVariation.BlockTypeID);
                NewBlockData = firstVariation.BlockData;
                NewVariationNumber = firstVariation.Variation;
                NewBlockModelPath = firstVariation.BlockModelPath;
            }
            else
            {
                NewVariationNumber = 1;
            }
        }

        private void UpdateOriginalData()
        {
            // Copy changes from the edited copy back to the original object.
            _originalCustomModelData.Name = EditedCustomModelData.Name;
            _originalCustomModelData.ModelPath = EditedCustomModelData.ModelPath;
            _originalCustomModelData.CustomModelNumber = EditedCustomModelData.CustomModelNumber;
            _originalCustomModelData.Status = EditedCustomModelData.Status;

            if (!_originalCustomModelData.Status)
            {
                StripDataAndRenameUnusedItem();
            }
            else
            {
                // Remove any default parent (ID==1) from the original before updating.
                var defaultParents = _originalCustomModelData.ParentItems.Where(p => p.ParentItemID == 1).ToList();
                foreach (var p in defaultParents)
                {
                    _originalCustomModelData.ParentItems.Remove(p);
                }

                for (int i = SelectedParentItems.Count - 1; i >= 0; i--)
                {
                    if (SelectedParentItems[i].ParentItemID == 1)
                        SelectedParentItems.RemoveAt(i);
                }

                _originalCustomModelData.ParentItems.Clear();
                foreach (var item in SelectedParentItems)
                {
                    _originalCustomModelData.ParentItems.Add(item);
                }

                UpdateRelations();
                AddOrUpdateCustomVariation();
            }
        }

        private void StripDataAndRenameUnusedItem()
        {
            string newName = GenerateUnusedName();
            _originalCustomModelData.Name = newName;
            _originalCustomModelData.ModelPath = string.Empty;

            // Ensure the ParentItems collection is loaded so that removals are tracked.
            _context.Entry(_originalCustomModelData).Collection(c => c.ParentItems).Load();
            _originalCustomModelData.ParentItems.Clear();
            var defaultParent = _context.ParentItems.FirstOrDefault(p => p.ParentItemID == 1);
            if (defaultParent != null)
            {
                _originalCustomModelData.ParentItems.Add(defaultParent);
            }

            _originalCustomModelData.BlockTypes.Clear();
            _originalCustomModelData.CustomVariations.Clear();
            _originalCustomModelData.ShaderArmors.Clear();
        }

        private string GenerateUnusedName()
        {
            var existingUnusedNames = _context.CustomModelDataItems
                .Where(x => !x.Status && x.Name.StartsWith("Unused"))
                .Select(x => x.Name)
                .ToList();

            char letter = 'A';
            while (existingUnusedNames.Contains($"Unused {letter}"))
            {
                letter++;
            }

            return $"Unused {letter}";
        }

        private void UpdateRelations()
        {
            if (SelectedBlockType != null &&
                !_originalCustomModelData.BlockTypes.Any(b => b.BlockTypeID == SelectedBlockType.BlockTypeID))
            {
                _originalCustomModelData.BlockTypes.Add(new CustomModel_BlockType
                {
                    BlockTypeID = SelectedBlockType.BlockTypeID,
                    CustomModelDataID = _originalCustomModelData.CustomModelDataID
                });
            }

            if (SelectedShaderArmorColorInfo != null &&
                !_originalCustomModelData.ShaderArmors.Any(sa => sa.ShaderArmorColorInfoID == SelectedShaderArmorColorInfo.ShaderArmorColorInfoID))
            {
                _originalCustomModelData.ShaderArmors.Add(new CustomModel_ShaderArmor
                {
                    ShaderArmorColorInfoID = SelectedShaderArmorColorInfo.ShaderArmorColorInfoID,
                    CustomModelDataID = _originalCustomModelData.CustomModelDataID
                });
            }
        }

        private void AddOrUpdateCustomVariation()
        {
            if (!string.IsNullOrWhiteSpace(NewBlockData) && SelectedBlockType != null)
            {
                var existingVariation = _originalCustomModelData.CustomVariations.FirstOrDefault(v => v.BlockTypeID == SelectedBlockType.BlockTypeID);
                if (existingVariation != null)
                {
                    existingVariation.BlockData = NewBlockData;
                    existingVariation.Variation = NewVariationNumber;
                    existingVariation.BlockModelPath = NewBlockModelPath;
                }
                else
                {
                    _originalCustomModelData.CustomVariations.Add(new CustomVariation
                    {
                        BlockData = NewBlockData,
                        Variation = NewVariationNumber,
                        BlockTypeID = SelectedBlockType.BlockTypeID,
                        CustomModelDataID = _originalCustomModelData.CustomModelDataID,
                        BlockModelPath = NewBlockModelPath
                    });
                }
            }
        }

        private void UpdateNewVariationNumber()
        {
            NewVariationNumber = SelectedBlockType != null
                ? ((_context.CustomVariations.Where(cv => cv.BlockTypeID == SelectedBlockType.BlockTypeID).Max(cv => (int?)cv.Variation) ?? 0) + 1)
                : 1;
        }
    }
}
