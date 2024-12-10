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
        #region Fields

        private readonly AppDbContext _context;

        #endregion

        #region Constructor

        public ArmorInfoViewModel()
        {
            _context = new AppDbContext();
            LoadData();

            AddArmorInfoCommand = new RelayCommand(AddArmorInfo);
            DeleteArmorInfoCommand = new RelayCommand(DeleteArmorInfo);
        }

        #endregion

        #region Properties

        public ObservableCollection<ShaderArmorColorInfo> ArmorInfos { get; private set; } = new ObservableCollection<ShaderArmorColorInfo>();

        public string NewName { get; set; } = string.Empty;
        public string NewHEX { get; set; } = string.Empty;
        public string NewRGB { get; set; } = string.Empty;
        public int NewColor { get; set; }

        public ICommand AddArmorInfoCommand { get; }
        public ICommand DeleteArmorInfoCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all ShaderArmorColorInfo records from the database and updates the collection.
        /// </summary>
        private void LoadData()
        {
            ArmorInfos = new ObservableCollection<ShaderArmorColorInfo>(
                _context.ShaderArmorColorInfos.OrderBy(a => a.Name).ToList()
            );
            OnPropertyChanged(nameof(ArmorInfos));
        }

        /// <summary>
        /// Adds a new ShaderArmorColorInfo record to the database.
        /// </summary>
        private void AddArmorInfo(object? parameter)
        {
            if (!ValidateNewArmorInfo()) return;

            ShaderArmorColorInfo newInfo = new ShaderArmorColorInfo
            {
                Name = NewName.Trim(),
                HEX = NewHEX.Trim(),
                RGB = NewRGB.Trim(),
                Color = NewColor
            };

            _context.ShaderArmorColorInfos.Add(newInfo);
            _context.SaveChanges();

            ResetNewArmorInfoFields();
            LoadData();
        }

        /// <summary>
        /// Deletes the selected ShaderArmorColorInfo record from the database.
        /// </summary>
        private void DeleteArmorInfo(object? parameter)
        {
            if (parameter is ShaderArmorColorInfo infoToDelete)
            {
                _context.ShaderArmorColorInfos.Remove(infoToDelete);
                _context.SaveChanges();
                LoadData();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Validates the new ShaderArmorColorInfo input fields.
        /// </summary>
        private bool ValidateNewArmorInfo()
        {
            return !string.IsNullOrWhiteSpace(NewName) &&
                   !string.IsNullOrWhiteSpace(NewHEX) &&
                   !string.IsNullOrWhiteSpace(NewRGB);
        }

        /// <summary>
        /// Resets the input fields for adding a new ShaderArmorColorInfo.
        /// </summary>
        private void ResetNewArmorInfoFields()
        {
            NewName = string.Empty;
            NewHEX = string.Empty;
            NewRGB = string.Empty;
            NewColor = 0;

            OnPropertyChanged(nameof(NewName));
            OnPropertyChanged(nameof(NewHEX));
            OnPropertyChanged(nameof(NewRGB));
            OnPropertyChanged(nameof(NewColor));
        }

        #endregion
    }
}