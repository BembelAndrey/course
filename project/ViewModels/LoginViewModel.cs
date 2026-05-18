using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project.Data;
using project.Models;
using project.Services;
using System.Linq;

namespace project.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [RelayCommand]
        private void Login()
        {
            ErrorMessage = string.Empty;

            using (var context = new AppDbContext())
            {
                string hashedPass = User.HashPassword(Password);
                var user = context.Users.FirstOrDefault(u => u.Username == Username && (u.PasswordHash == hashedPass || u.PasswordHash == Password));
                
                if (user != null)
                {
                    if (user.PasswordHash == Password)
                    {
                        user.PasswordHash = hashedPass;
                        context.SaveChanges();
                    }

                    SessionManager.Instance.Login(user);
                    _mainViewModel.RefreshAuth();

                    if (user.Role == Role.Admin)
                    {
                        _mainViewModel.CurrentViewModel = new AdminViewModel(_mainViewModel);
                    }
                    else if (user.Role == Role.Executor)
                    {
                        _mainViewModel.CurrentViewModel = new ExecutorViewModel(_mainViewModel);
                    }
                    else
                    {
                        _mainViewModel.CurrentViewModel = new CabinetViewModel(_mainViewModel);
                    }
                }
                else
                {
                    ErrorMessage = TranslationSource.Instance["ErrInvalidLogin"];
                }
            }
        }

        [RelayCommand]
        private void GoToRegistration()
        {
            _mainViewModel.CurrentViewModel = new RegistrationViewModel(_mainViewModel);
        }
    }
}
