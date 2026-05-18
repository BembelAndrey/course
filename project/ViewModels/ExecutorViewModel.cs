using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using project.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace project.ViewModels
{
    public partial class ExecutorViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        public TranslationSource Loc => TranslationSource.Instance;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private decimal _monthlyEarnings;

        [ObservableProperty]
        private ObservableCollection<Order> _myActiveOrders = new();

        [ObservableProperty]
        private ObservableCollection<DateTime> _workDays = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Message))]
        private string _messageKey = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Message))]
        private string _messageParam = string.Empty;

        public string Message => string.IsNullOrEmpty(MessageKey) ? string.Empty : 
            (string.IsNullOrEmpty(MessageParam) ? Loc[MessageKey] : $"{Loc[MessageKey]} {MessageParam}");

        public ExecutorViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoadMasterProfile();
            LoadOrders();

            TranslationSource.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item" || e.PropertyName == "Item[]" || e.PropertyName == "CurrentLanguage")
                {
                    LoadMasterProfile();
                    var temp = MyActiveOrders;
                    MyActiveOrders = null;
                    MyActiveOrders = temp;
                    OnPropertyChanged(nameof(FullName));
                    OnPropertyChanged(nameof(MonthlyEarnings));
                    OnPropertyChanged(nameof(Message));
                }
            };
        }

        private void LoadMasterProfile()
        {
            var user = SessionManager.Instance.CurrentUser;
            if (user != null)
            {
                FullName = Loc.CurrentLanguage == "RU" ? user.FullNameRu : user.FullNameEn;
                
                using var context = new AppDbContext();
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                
                var completedThisMonth = context.Orders
                    .Where(o => o.ExecutorId == user.Id && o.Status == OrderStatus.Completed && o.CompletionDate >= monthStart)
                    .ToList();
                
                MonthlyEarnings = completedThisMonth.Sum(o => CalculateEarnings(o));
            }
        }

        private void LoadOrders()
        {
            using var context = new AppDbContext();
            int currentMasterId = SessionManager.Instance.CurrentUser.Id;

            var myOrders = context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Material)
                .Include(o => o.User)
                .Where(o => o.ExecutorId == currentMasterId)
                .OrderBy(o => o.ScheduledStartDate ?? o.OrderDate)
                .ToList();

            MyActiveOrders = new ObservableCollection<Order>(myOrders.Where(o => o.Status == OrderStatus.Processing));
            
            WorkDays.Clear();
            foreach (var o in myOrders.Where(o => o.Status == OrderStatus.Processing || o.Status == OrderStatus.Pending))
            {
                if (o.ScheduledStartDate.HasValue && o.ScheduledEndDate.HasValue)
                {
                    for (var dt = o.ScheduledStartDate.Value.Date; dt <= o.ScheduledEndDate.Value.Date; dt = dt.AddDays(1))
                    {
                        if (!WorkDays.Contains(dt)) WorkDays.Add(dt);
                    }
                }
            }
        }

        public decimal CalculateEarnings(Order order)
        {
            if (order == null || order.Area <= 0) return 0;

            decimal rate = order.SurfaceType switch
            {
                "Floor" => 30m,
                "Wall" => 50m,
                "Ceiling" => 70m,
                _ => 0m
            };

            return rate * (decimal)order.Area;
        }

        [RelayCommand]
        private void CompleteOrder(Order order)
        {
            if (order == null) return;

            using var context = new AppDbContext();
            var dbOrder = context.Orders.Include(o => o.User).FirstOrDefault(o => o.Id == order.Id);
            var dbUser = context.Users.Find(SessionManager.Instance.CurrentUser.Id);

            if (dbOrder != null && dbOrder.Status == OrderStatus.Processing && dbUser != null)
            {
                dbOrder.Status = OrderStatus.Completed;
                dbOrder.CompletionDate = DateTime.Now;
                
                decimal earnings = CalculateEarnings(dbOrder);
                dbUser.TotalEarnings += earnings;
                
                context.SaveChanges();
                
                // Пересчет графика (сдвиг оставшихся заказов)
                RescheduleRemainingOrders(dbUser.Id, context);
                context.SaveChanges();

                SessionManager.Instance.CurrentUser.TotalEarnings = dbUser.TotalEarnings;

                if (dbOrder.User != null && !string.IsNullOrWhiteSpace(dbOrder.User.Email))
                {
                    string translatedStatus = Loc[dbOrder.Status.ToString()];
                    EmailService.SendOrderNotification(dbOrder.User.Email, dbOrder.Id, translatedStatus);
                }

                MessageKey = "OrderCompletedExec";
                MessageParam = string.Empty;
                LoadMasterProfile();
                LoadOrders();
            }
        }

        private void RescheduleRemainingOrders(int executorId, AppDbContext context)
        {
            var remainingOrders = context.Orders
                .Where(o => o.ExecutorId == executorId && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing))
                .Where(o => !string.IsNullOrEmpty(o.SurfaceType))
                .OrderBy(o => o.ScheduledStartDate ?? o.OrderDate)
                .ToList();

            DateTime nextAvailableDate = DateTime.Now.Date;

            foreach (var o in remainingOrders)
            {
                double totalHours = o.SurfaceType switch
                {
                    "Floor" => o.Area * 1,
                    "Wall" => o.Area * 3,
                    "Ceiling" => o.Area * 4,
                    _ => 0
                };

                int totalDays = (int)Math.Ceiling(totalHours / 8.0);
                if (totalDays < 1) totalDays = 1;

                o.ScheduledStartDate = GetNextWorkDay(nextAvailableDate);
                o.ScheduledEndDate = o.ScheduledStartDate;

                for (int i = 1; i < totalDays; i++)
                {
                    o.ScheduledEndDate = GetNextWorkDay(o.ScheduledEndDate.Value.AddDays(1));
                }

                nextAvailableDate = o.ScheduledEndDate.Value.AddDays(1);
            }
        }

        private DateTime GetNextWorkDay(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return date;
        }

        [RelayCommand]
        private void Logout()
        {
            SessionManager.Instance.Logout();
            _mainViewModel.RefreshAuth();
            _mainViewModel.GoToHome();
        }
    }
}
