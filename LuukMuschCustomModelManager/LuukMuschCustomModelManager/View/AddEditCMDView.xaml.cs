using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LuukMuschCustomModelManager.Model;
using LuukMuschCustomModelManager.ViewModels.Views;

namespace LuukMuschCustomModelManager.View
{
    public partial class AddEditCMDView : UserControl
    {
        public AddEditCMDView()
        {
            InitializeComponent();
        }

        // When a parent checkbox is loaded, set its IsChecked state based on whether its ParentItem is in SelectedParentItems.
        private void ParentCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is ParentItem parent)
            {
                if (DataContext is AddEditCMDViewModel vm)
                {
                    cb.IsChecked = vm.SelectedParentItems.Contains(parent);
                }
            }
        }

        private void ParentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is ParentItem parent)
            {
                if (DataContext is AddEditCMDViewModel vm)
                {
                    if (!vm.SelectedParentItems.Contains(parent))
                    {
                        vm.SelectedParentItems.Add(parent);
                    }
                }
            }
        }

        private void ParentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is ParentItem parent)
            {
                if (DataContext is AddEditCMDViewModel vm)
                {
                    if (vm.SelectedParentItems.Contains(parent))
                    {
                        vm.SelectedParentItems.Remove(parent);
                    }
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // (Your existing logic, if any.)
        }
    }
}