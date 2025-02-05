using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Data;
using System.IO;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.View;
using LuukMuschCustomModelManager.ViewModels;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace LuukMuschCustomModelManager
{
    public partial class App : Application
    {
        private Process? _mysqlProcess = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure MySQL is running
            if (!EnsureMySQLIsRunning())
            {
                MessageBox.Show("Failed to start MySQL automatically. The application will now exit.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            // Wait a little extra to ensure MySQL is fully ready before attempting any DB work
            Thread.Sleep(5000);

            try
            {
                // Initialize Database (includes migrations and seeding)
                AppDbContext.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            Exit += OnApplicationExit;

            MainWindow = new MainView
            {
                DataContext = new MainViewModel()
            };
            MainWindow.Show();
        }

        private bool EnsureMySQLIsRunning()
        {
            int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                if (IsMySQLRunning())
                    return true;

                StartMySQL();
                // Wait a short period before trying again
                Thread.Sleep(3000);
                attempts++;
            }

            return false;
        }

        private bool IsMySQLRunning()
        {
            return Process.GetProcessesByName("mysqld").Any();
        }

        private void StartMySQL()
        {
            try
            {
                string xamppPath = ConfigurationManager.AppSettings["XAMPPPath"] ?? @"C:\xampp\mysql\bin";
                string mysqldPath = Path.Combine(xamppPath, "mysqld.exe");

                if (File.Exists(mysqldPath))
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = mysqldPath,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    _mysqlProcess = Process.Start(processStartInfo);
                    return;
                }

                // Fallback: attempt to start MySQL using net start
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start MySQL: {ex.Message}");
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            CloseDatabaseConnections();
            StopMySQL();
        }

        private void CloseDatabaseConnections()
        {
            try
            {
                using var context = new AppDbContext();
                var connection = context.Database.GetDbConnection();

                if (connection.State != ConnectionState.Closed)
                {
                    // Save any pending changes and then close connection
                    context.SaveChanges();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to close database connections: {ex.Message}");
            }
        }

        private void StopMySQL()
        {
            try
            {
                // Make sure all DB connections are closed before shutting down MySQL
                CloseDatabaseConnections();
                Thread.Sleep(5000);

                if (_mysqlProcess != null && !_mysqlProcess.HasExited)
                {
                    // Try to close gracefully first
                    _mysqlProcess.CloseMainWindow();
                    if (!_mysqlProcess.WaitForExit(5000))
                    {
                        // If it did not close in time, kill it forcefully
                        _mysqlProcess.Kill();
                        _mysqlProcess.WaitForExit();
                    }
                    return;
                }

                // Fallback: use net stop command
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c net stop MySQL",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(psi);
                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to stop MySQL: {ex.Message}");
            }
        }
    }
}