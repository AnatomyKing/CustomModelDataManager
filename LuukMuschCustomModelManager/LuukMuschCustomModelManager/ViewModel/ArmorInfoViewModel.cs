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
    internal class ArmorInfoViewModel : ObservableObject
    {
        private readonly AppDbContext _context;

        public ArmorInfoViewModel()
        {
            _context = new AppDbContext();
            LoadData();

            AddOrUpdateArmorInfoCommand = new RelayCommand(AddOrUpdateArmorInfo);
            DeleteArmorInfoCommand = new RelayCommand(DeleteArmorInfo);
            CancelArmorInfoEditCommand = new RelayCommand(CancelArmorInfoEdit);
            // Set default values for new inputs.
            ResetArmorInfoFields();
        }

        // Collection of Armor Infos
        public ObservableCollection<ShaderArmorColorInfo> ArmorInfos { get; private set; } = new ObservableCollection<ShaderArmorColorInfo>();

        // Input fields
        private string _newName = string.Empty;
        public string NewName
        {
            get => _newName;
            set { _newName = value; OnPropertyChanged(); }
        }

        private string _newHEX = string.Empty;
        public string NewHEX
        {
            get => _newHEX;
            set { _newHEX = value; OnPropertyChanged(); }
        }

        private string _newRGB = string.Empty;
        public string NewRGB
        {
            get => _newRGB;
            set { _newRGB = value; OnPropertyChanged(); }
        }

        private int _newColor;
        public int NewColor
        {
            get => _newColor;
            set { _newColor = value; OnPropertyChanged(); }
        }

        // Selected Armor Info for editing
        private ShaderArmorColorInfo? _selectedArmorInfo;
        public ShaderArmorColorInfo? SelectedArmorInfo
        {
            get => _selectedArmorInfo;
            set
            {
                _selectedArmorInfo = value;
                OnPropertyChanged();
                if (_selectedArmorInfo != null)
                {
                    NewName = _selectedArmorInfo.Name;
                    NewHEX = _selectedArmorInfo.HEX;
                    NewRGB = _selectedArmorInfo.RGB;
                    NewColor = _selectedArmorInfo.Color;
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

        public string ButtonContent => IsEditing ? "Update Armor Info" : "Add Armor Info";

        // Commands
        public ICommand AddOrUpdateArmorInfoCommand { get; }
        public ICommand DeleteArmorInfoCommand { get; }
        public ICommand CancelArmorInfoEditCommand { get; }

        private void LoadData()
        {
            ArmorInfos = new ObservableCollection<ShaderArmorColorInfo>(
                _context.ShaderArmorColorInfos.OrderBy(a => a.Name).ToList()
            );
            OnPropertyChanged(nameof(ArmorInfos));
        }

        private void AddOrUpdateArmorInfo(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewName) ||
                string.IsNullOrWhiteSpace(NewHEX) ||
                string.IsNullOrWhiteSpace(NewRGB))
                return;

            if (IsEditing && SelectedArmorInfo != null)
            {
                // Update existing record.
                SelectedArmorInfo.Name = NewName.Trim();
                SelectedArmorInfo.HEX = NewHEX.Trim();
                SelectedArmorInfo.RGB = NewRGB.Trim();
                SelectedArmorInfo.Color = NewColor;
                _context.ShaderArmorColorInfos.Update(SelectedArmorInfo);
            }
            else
            {
                // Add new record.
                var newInfo = new ShaderArmorColorInfo
                {
                    Name = NewName.Trim(),
                    HEX = NewHEX.Trim(),
                    RGB = NewRGB.Trim(),
                    Color = NewColor
                };
                _context.ShaderArmorColorInfos.Add(newInfo);
            }
            _context.SaveChanges();
            ResetArmorInfoFields();
            SelectedArmorInfo = null;
            IsEditing = false;
            LoadData();
        }

        private void DeleteArmorInfo(object? parameter)
        {
            if (parameter is ShaderArmorColorInfo infoToDelete)
            {
                _context.ShaderArmorColorInfos.Remove(infoToDelete);
                _context.SaveChanges();
                LoadData();
            }
        }

        private void CancelArmorInfoEdit(object? parameter)
        {
            ResetArmorInfoFields();
            SelectedArmorInfo = null;
            IsEditing = false;
        }

        private void ResetArmorInfoFields()
        {
            NewName = string.Empty;
            NewHEX = "#";         // Default to a hash symbol
            NewRGB = "0,0,0";      // Default RGB value
            NewColor = 0;
            OnPropertyChanged(nameof(NewName));
            OnPropertyChanged(nameof(NewHEX));
            OnPropertyChanged(nameof(NewRGB));
            OnPropertyChanged(nameof(NewColor));
        }
    }
}