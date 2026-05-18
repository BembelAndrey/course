using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project.Services;
using project.Models;
using System.Windows;

namespace project.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentViewModel;

        public TranslationSource Loc => TranslationSource.Instance;

        public bool IsAdmin => SessionManager.Instance.IsLoggedIn && SessionManager.Instance.IsAdmin;
        public bool IsExecutor => SessionManager.Instance.IsLoggedIn && SessionManager.Instance.CurrentUser.Role == Role.Executor;
        public bool ShowMenu => !IsAdmin && !IsExecutor; 
        public bool IsLoggedIn => SessionManager.Instance.IsLoggedIn;

        public Visibility MenuVisibility => ShowMenu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LogoutVisibility => IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;

        public MainViewModel()
        {
            GoToHome();
        }

        public void RefreshAuth()
        {
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsExecutor));
            OnPropertyChanged(nameof(ShowMenu));
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(MenuVisibility));
            OnPropertyChanged(nameof(LogoutVisibility));
        }

        [RelayCommand]
        public void GoToHome() => CurrentViewModel = new HomeViewModel();

        [RelayCommand]
        public void GoToCatalog() => CurrentViewModel = new CatalogViewModel(this);

        [RelayCommand]
        public void GoToCalculator() => CurrentViewModel = new CalculatorViewModel();

        [RelayCommand]
        public void GoToCart()
        {
            if (SessionManager.Instance.IsLoggedIn)
                CurrentViewModel = new CartViewModel();
            else
                CurrentViewModel = new LoginViewModel(this);
        }

        [RelayCommand]
        public void GoToCabinet()
        {
            if (SessionManager.Instance.IsLoggedIn)
            {
                if (SessionManager.Instance.IsAdmin)
                    CurrentViewModel = new AdminViewModel(this);
                else if (SessionManager.Instance.CurrentUser.Role == Role.Executor)
                    CurrentViewModel = new ExecutorViewModel(this);
                else
                    CurrentViewModel = new CabinetViewModel(this);
            }
            else
            {
                CurrentViewModel = new LoginViewModel(this);
            }
        }

        [RelayCommand]
        public void Logout()
        {
            SessionManager.Instance.Logout();
            RefreshAuth();
            GoToHome();
        }

        [RelayCommand]
        public void ToggleLanguage()
        {
            Loc.ToggleLanguage();
        }
    }
}
