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
using System.Windows.Threading;
using LuukMuschCustomModelManager.Model;
using LuukMuschCustomModelManager.ViewModels.Views;

namespace LuukMuschCustomModelManager.View
{
    public partial class AddEditCMDView : UserControl
    {
        public AddEditCMDView()
        {
            InitializeComponent();

            // Set up the CollectionViewSource filter for ParentItems.
            var cvs = (CollectionViewSource)this.Resources["GroupedParentItems"];
            cvs.Filter += ParentItemsFilter;
        }

        #region Filtering

        private void ParentItemsFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is ParentItem parent)
            {
                string search = ParentSearchBox.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(search))
                {
                    e.Accepted = true;
                    return;
                }
                if ((parent.Name != null && parent.Name.ToLower().Contains(search)) ||
                    (parent.Type != null && parent.Type.ToLower().Contains(search)))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }
            else
            {
                e.Accepted = true;
            }
        }

        private void ParentSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var cvs = (CollectionViewSource)this.Resources["GroupedParentItems"];
            cvs.View.Refresh();
        }

        #endregion

        #region Group Header Checkbox Events

        private void GroupHeaderCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox groupCheckBox && groupCheckBox.DataContext is CollectionViewGroup group)
            {
                var groupItems = group.Items.OfType<ParentItem>().ToList();
                bool allSelected = groupItems.All(p => p.IsSelected);
                bool noneSelected = groupItems.All(p => !p.IsSelected);
                groupCheckBox.IsChecked = allSelected ? true : (noneSelected ? false : (bool?)null);
            }
        }

        private void GroupHeaderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox groupCheckBox && groupCheckBox.DataContext is CollectionViewGroup group)
            {
                var groupItems = group.Items.OfType<ParentItem>().ToList();
                bool allSelected = groupItems.All(p => p.IsSelected);

                // If all are selected, unselect all; otherwise, select all.
                bool newState = !allSelected;
                foreach (var parent in groupItems)
                {
                    parent.IsSelected = newState;
                }
            }
            ScheduleRefresh();
        }

        #endregion

        #region Helper Methods

        private void ScheduleRefresh()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var cvs = (CollectionViewSource)this.Resources["GroupedParentItems"];
                cvs.View.Refresh();
            }), DispatcherPriority.Background);
        }

        #endregion
    }
}