using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using project.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace project.ViewModels
{
    public partial class CabinetViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public CabinetViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoadOrders();
            TranslationSource.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item[]" || e.PropertyName == "CurrentLanguage")
                {
                    var temp = MyOrders;
                    MyOrders = null;
                    MyOrders = temp;
                }
            };
        }

        public string Username => SessionManager.Instance.CurrentUser?.Username ?? TranslationSource.Instance["Guest"];

        [ObservableProperty]
        private ObservableCollection<Order> _myOrders = new();

        private void LoadOrders()
        {
            if (SessionManager.Instance.CurrentUser != null)
            {
                using var context = new AppDbContext();
                var orders = context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Material)
                    .Where(o => o.UserId == SessionManager.Instance.CurrentUser.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
                
                MyOrders = new ObservableCollection<Order>(orders);
            }
        }

        [RelayCommand]
        private void Logout()
        {
            SessionManager.Instance.Logout();
            _mainViewModel.GoToHome();
        }
    }
}
