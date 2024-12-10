using System.Configuration;
using System.Data;
using System.Windows;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.View;
using LuukMuschCustomModelManager.ViewModels;

namespace LuukMuschCustomModelManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // initialize AppdbContext database
                AppDbContext.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            MainWindow = new MainView
            {
                DataContext = new MainViewModel()
            };

            MainWindow.Show();
        }
    }
}