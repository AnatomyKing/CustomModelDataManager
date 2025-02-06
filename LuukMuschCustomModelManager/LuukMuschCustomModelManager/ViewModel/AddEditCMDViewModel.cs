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

namespace LuukMuschCustomModelManager.ViewModels.Views
{
    internal class AddEditCMDViewModel : ObservableObject
    {
        private readonly CustomModelData _originalCustomModelData;
        private readonly AppDbContext _context;

        public AddEditCMDViewModel(
            CustomModelData customModelData,
            ObservableCollection<ParentItem> parentItems,
            ObservableCollection<BlockType> blockTypes,
            ObservableCollection<ShaderArmorColorInfo> shaderArmorColorInfos,
            AppDbContext context)
        {
            _originalCustomModelData = customModelData;
            _context = context;

            ParentItems = parentItems;
            BlockTypes = blockTypes;
            ShaderArmorColorInfos = shaderArmorColorInfos;

            CancelCommand = new RelayCommand(Cancel);
            SaveCommand = new RelayCommand(Save);
            ClearArmorInfoCommand = new RelayCommand(ClearArmorInfo);
            ClearBlockInfoCommand = new RelayCommand(ClearBlockInfo);

            // Create an editable copy of the original data.
            EditedCustomModelData = CreateEditableCopy(customModelData);
            CustomVariations = new ObservableCollection<CustomVariation>(EditedCustomModelData.CustomVariations);

            _selectedParentItems = new ArrayList();
            PreSelectProperties();
        }

        #region Properties

        public CustomModelData EditedCustomModelData { get; }
        public ObservableCollection<ParentItem> ParentItems { get; }
        public ObservableCollection<BlockType> BlockTypes { get; }
        public ObservableCollection<ShaderArmorColorInfo> ShaderArmorColorInfos { get; }
        public ObservableCollection<CustomVariation> CustomVariations { get; }

        private IList _selectedParentItems;
        public IList SelectedParentItems
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

        #endregion

        #region Command Methods

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
            SelectedBlockType = null;
        }

        private void ClearArmorInfo(object? obj)
        {
            SelectedShaderArmorColorInfo = null;
        }

        #endregion

        #region Private Methods

        private CustomModelData CreateEditableCopy(CustomModelData original)
        {
            return new CustomModelData
            {
                Name = original.Name,
                ModelPath = original.ModelPath,
                CustomModelNumber = original.CustomModelNumber,
                Status = original.Status,
                ParentItems = new List<ParentItem>(original.ParentItems),
                CustomVariations = new List<CustomVariation>(original.CustomVariations),
                ShaderArmors = new List<CustomModel_ShaderArmor>(original.ShaderArmors),
                BlockTypes = new List<CustomModel_BlockType>(original.BlockTypes)
            };
        }

        private void PreSelectProperties()
        {
            // Preselect parent items except the default unused parent (ID == 1) if the item is used.
            foreach (var parent in EditedCustomModelData.ParentItems)
            {
                if (EditedCustomModelData.Status && parent.ParentItemID == 1)
                    continue;

                if (!_selectedParentItems.Contains(parent))
                    _selectedParentItems.Add(parent);
            }

            if (EditedCustomModelData.ShaderArmors.Any())
            {
                SelectedShaderArmorColorInfo = ShaderArmorColorInfos
                    .FirstOrDefault(s => EditedCustomModelData.ShaderArmors.Any(sa => sa.ShaderArmorColorInfoID == s.ShaderArmorColorInfoID));
            }

            var firstVariation = CustomVariations.FirstOrDefault();
            if (firstVariation != null)
            {
                SelectedBlockType = BlockTypes.FirstOrDefault(b => b.BlockTypeID == firstVariation.BlockTypeID);
                NewBlockData = firstVariation.BlockData;
                NewVariationNumber = firstVariation.Variation;
            }
            else
            {
                NewVariationNumber = 1;
            }
        }

        private void UpdateOriginalData()
        {
            _originalCustomModelData.Name = EditedCustomModelData.Name;
            _originalCustomModelData.ModelPath = EditedCustomModelData.ModelPath;
            _originalCustomModelData.CustomModelNumber = EditedCustomModelData.CustomModelNumber;
            _originalCustomModelData.Status = EditedCustomModelData.Status;

            if (!_originalCustomModelData.Status)
            {
                // When the item is unused, strip its data and assign the default unused parent.
                StripDataAndRenameUnusedItem();
            }
            else
            {
                // When the item is used, ensure that the default unused parent (ID == 1) is removed.

                // Remove any default unused parent from the original ParentItems.
                var defaultParents = _originalCustomModelData.ParentItems.Where(p => p.ParentItemID == 1).ToList();
                foreach (var p in defaultParents)
                {
                    _originalCustomModelData.ParentItems.Remove(p);
                }

                // Remove the default unused parent from the SelectedParentItems collection.
                for (int i = SelectedParentItems.Count - 1; i >= 0; i--)
                {
                    if (SelectedParentItems[i] is ParentItem parent && parent.ParentItemID == 1)
                        SelectedParentItems.RemoveAt(i);
                }

                // Rebuild the original ParentItems based solely on the cleaned SelectedParentItems.
                _originalCustomModelData.ParentItems.Clear();
                foreach (var item in SelectedParentItems)
                {
                    if (item is ParentItem p && p.ParentItemID != 1)
                        _originalCustomModelData.ParentItems.Add(p);
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
            if (SelectedBlockType != null && !_originalCustomModelData.BlockTypes.Any(b => b.BlockTypeID == SelectedBlockType.BlockTypeID))
            {
                _originalCustomModelData.BlockTypes.Add(new CustomModel_BlockType
                {
                    BlockTypeID = SelectedBlockType.BlockTypeID,
                    CustomModelDataID = _originalCustomModelData.CustomModelDataID
                });
            }

            if (SelectedShaderArmorColorInfo != null && !_originalCustomModelData.ShaderArmors.Any(sa => sa.ShaderArmorColorInfoID == SelectedShaderArmorColorInfo.ShaderArmorColorInfoID))
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
                }
                else
                {
                    _originalCustomModelData.CustomVariations.Add(new CustomVariation
                    {
                        BlockData = NewBlockData,
                        Variation = NewVariationNumber,
                        BlockTypeID = SelectedBlockType.BlockTypeID,
                        CustomModelDataID = _originalCustomModelData.CustomModelDataID
                    });
                }
            }
        }

        private void UpdateNewVariationNumber()
        {
            NewVariationNumber = SelectedBlockType != null
                ? ((_context.CustomVariations
                    .Where(cv => cv.BlockTypeID == SelectedBlockType.BlockTypeID)
                    .Max(cv => (int?)cv.Variation) ?? 0) + 1)
                : 1;
        }

        #endregion
    }
}