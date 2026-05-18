using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project.Data;
using project.Models;
using project.Services;
using System.Linq;

namespace project.ViewModels
{
    public partial class RegistrationViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public RegistrationViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [RelayCommand]
        private void Register()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = TranslationSource.Instance["ErrEmptyFields"];
                return;
            }

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                ErrorMessage = TranslationSource.Instance["ErrInvalidEmail"];
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = TranslationSource.Instance["ErrPassMismatch"];
                return;
            }

            using (var context = new AppDbContext())
            {
                var existingUser = context.Users.FirstOrDefault(u => u.Username == Username);
                if (existingUser != null)
                {
                    ErrorMessage = TranslationSource.Instance["ErrUserExists"];
                    return;
                }

                var newUser = new User
                {
                    Username = Username,
                    Email = Email,
                    PasswordHash = User.HashPassword(Password),
                    FullNameRu = Username,
                    FullNameEn = Username,
                    Role = Role.Client
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                // Автоматический вход после успешной регистрации
                SessionManager.Instance.Login(newUser);
                _mainViewModel.RefreshAuth();
                _mainViewModel.CurrentViewModel = new CabinetViewModel(_mainViewModel);
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.CurrentViewModel = new LoginViewModel(_mainViewModel);
        }
    }
}
