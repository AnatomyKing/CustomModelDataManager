using System.Collections.ObjectModel;
using System.Windows.Input;
using LuukMuschCustomModelManager.Databases;
using LuukMuschCustomModelManager.Helpers;
using LuukMuschCustomModelManager.Model;
using System;
using Microsoft.EntityFrameworkCore;
using LuukMuschCustomModelManager.ViewModels.Views;
using LuukMuschCustomModelManager.ViewModels;

namespace LuukMuschCustomModelManager.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        private object? _currentView;

        public MainViewModel()
        {
            CurrentView = new DashboardViewModel();
            NavigateCommand = new RelayCommand(Navigate);
        }

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateCommand { get; }

        private void Navigate(object? parameter)
        {
            string? viewName = parameter as string;

            CurrentView = viewName switch
            {
                "ParentItemViewModel" => new ParentItemViewModel(),
                "UnusedViewModel" => new UnusedViewModel(),
                "BlockTypeViewModel" => new BlockTypeViewModel(),
                "ArmorInfoViewModel" => new ArmorInfoViewModel(),
                "ImportViewModel" => new ImportViewModel(),
                "ExportViewModel" => new ExportViewModel(),
                "GenerationViewModel" => new GenerationViewModel(),
                _ => new DashboardViewModel()
            };
        }
    }
}