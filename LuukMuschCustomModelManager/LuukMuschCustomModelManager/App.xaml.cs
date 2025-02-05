using System;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.View;
using LuukMuschCustomModelManager.ViewModels;

namespace LuukMuschCustomModelManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure MySQL is running
            if (!EnsureMySQLIsRunning())
            {
                MessageBox.Show("Failed to start MySQL automatically. The application will now exit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            try
            {
                // Initialize Database
                AppDbContext.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            // Launch Main Window
            MainWindow = new MainView
            {
                DataContext = new MainViewModel()
            };

            MainWindow.Show();
        }

        /// <summary>
        /// Ensures MySQL is running by checking both XAMPP and MySQL service.
        /// </summary>
        private bool EnsureMySQLIsRunning()
        {
            int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                if (IsMySQLRunning())
                    return true;

                StartMySQL();

                Thread.Sleep(3000); // Wait for 3 seconds before retrying
                attempts++;
            }

            return false; // MySQL never started after max attempts
        }

        /// <summary>
        /// Checks if MySQL is running as a process.
        /// </summary>
        private bool IsMySQLRunning()
        {
            return Process.GetProcessesByName("mysqld").Any();
        }

        /// <summary>
        /// Starts MySQL either as a XAMPP process or a Windows service.
        /// </summary>
        private void StartMySQL()
        {
            try
            {
                // First, try starting MySQL via XAMPP (mysqld.exe)
                string xamppPath = ConfigurationManager.AppSettings["XAMPPPath"] ?? @"C:\xampp\mysql\bin";
                string mysqldPath = System.IO.Path.Combine(xamppPath, "mysqld.exe");

                if (System.IO.File.Exists(mysqldPath))
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = mysqldPath,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process.Start(processStartInfo);
                    Debug.WriteLine("Started MySQL via XAMPP (mysqld.exe).");
                    return;
                }

                // Fallback: Try starting MySQL as a Windows service
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c net start MySQL",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(psi);
                process?.WaitForExit();

                Debug.WriteLine("Started MySQL via Windows Service.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start MySQL: {ex.Message}");
            }
        }
    }
}