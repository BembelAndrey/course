using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using project.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace project.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public TranslationSource Loc => TranslationSource.Instance;

        public AdminViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoadData();
            TranslationSource.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item[]" || e.PropertyName == "CurrentLanguage")
                {
                    OnPropertyChanged(nameof(AdminOrderMessage));
                    OnPropertyChanged(nameof(AdminMaterialMessage));
                    OnPropertyChanged(nameof(AdminReviewMessage));

                    var tempMat = Materials;
                    Materials = null;
                    Materials = tempMat;
                    
                    var tempRev = PendingReviews;
                    PendingReviews = null;
                    PendingReviews = tempRev;

                    var tempOrders = Orders;
                    Orders = null;
                    Orders = tempOrders;
                    
                    var tempStat = OrderStatuses;
                    OrderStatuses = null;
                    OrderStatuses = tempStat;
                    
                    var tempExec = Executors;
                    Executors = null;
                    Executors = tempExec;
                }
            };
        }

        [ObservableProperty]
        private ObservableCollection<Material> _materials = new();

        [ObservableProperty]
        private ObservableCollection<Review> _pendingReviews = new();

        [ObservableProperty]
        private ObservableCollection<Order> _orders = new();

        [ObservableProperty]
        private ObservableCollection<User> _executors = new();

        [ObservableProperty]
        private User? _selectedExecutor;

        [ObservableProperty]
        private Material? _selectedMaterial;

        [ObservableProperty]
        private Review? _selectedPendingReview;

        [ObservableProperty]
        private Order? _selectedOrder;

        public bool IsServiceSelected => SelectedOrder != null && !string.IsNullOrEmpty(SelectedOrder.SurfaceType);

        [ObservableProperty]
        private ObservableCollection<OrderStatus> _orderStatuses = new() 
        { 
            OrderStatus.Pending, 
            OrderStatus.Processing, 
            OrderStatus.Completed, 
            OrderStatus.Cancelled 
        };

        [ObservableProperty]
        private OrderStatus _selectedOrderStatus;

        [ObservableProperty]
        private string _editTitleRu = string.Empty;

        [ObservableProperty]
        private string _editTitleEn = string.Empty;

        [ObservableProperty]
        private string _editCategoryRu = string.Empty;

        [ObservableProperty]
        private string _editCategoryEn = string.Empty;

        [ObservableProperty]
        private decimal _editPrice;

        [ObservableProperty]
        private string _editDescriptionRu = string.Empty;

        [ObservableProperty]
        private string _editDescriptionEn = string.Empty;

        [ObservableProperty]
        private string _editImagePath = string.Empty;

        [ObservableProperty]
        private int _editStockQuantity;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AdminMaterialMessage))]
        private string _adminMaterialMessageKey = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AdminReviewMessage))]
        private string _adminReviewMessageKey = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AdminOrderMessage))]
        private string _adminOrderMessageKey = string.Empty;

        public string AdminMaterialMessage => string.IsNullOrEmpty(AdminMaterialMessageKey) ? string.Empty : Loc[AdminMaterialMessageKey];
        public string AdminReviewMessage => string.IsNullOrEmpty(AdminReviewMessageKey) ? string.Empty : Loc[AdminReviewMessageKey];
        public string AdminOrderMessage => string.IsNullOrEmpty(AdminOrderMessageKey) ? string.Empty : Loc[AdminOrderMessageKey];

        [RelayCommand]
        private void PickImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                EditImagePath = openFileDialog.FileName;
            }
        }

        partial void OnSelectedMaterialChanged(Material? value)
        {
            if (value != null)
            {
                EditTitleRu = value.TitleRu;
                EditTitleEn = value.TitleEn;
                EditCategoryRu = value.CategoryRu;
                EditCategoryEn = value.CategoryEn;
                EditPrice = value.Price;
                EditDescriptionRu = value.DescriptionRu;
                EditDescriptionEn = value.DescriptionEn;
                EditImagePath = value.ImagePath;
                EditStockQuantity = value.StockQuantity;
            }
            else
            {
                EditTitleRu = string.Empty;
                EditTitleEn = string.Empty;
                EditCategoryRu = string.Empty;
                EditCategoryEn = string.Empty;
                EditPrice = 0;
                EditDescriptionRu = string.Empty;
                EditDescriptionEn = string.Empty;
                EditImagePath = string.Empty;
                EditStockQuantity = 0;
            }
        }

        partial void OnSelectedOrderChanged(Order? value)
        {
            OnPropertyChanged(nameof(IsServiceSelected));
            if (value != null)
            {
                SelectedOrderStatus = value.Status;
                SelectedExecutor = Executors.FirstOrDefault(e => e.Id == value.ExecutorId);
            }
        }

        private void LoadData()
        {
            using var context = new AppDbContext();
            
            var mats = context.Materials
                .Where(m => m.CategoryEn != "CatServices") // Скрываем виртуальные товары-услуги
                .ToList();
            Materials = new ObservableCollection<Material>(mats);
            
            foreach (var mat in mats)
            {
                if (!string.IsNullOrWhiteSpace(mat.CategoryEn))
                {
                    TranslationSource.Instance.AddOrUpdateTranslation("RU", mat.CategoryEn, mat.CategoryRu);
                }
            }

            Executors = new ObservableCollection<User>(
                context.Users.Where(u => u.Role == Role.Executor).ToList()
            );
            
            PendingReviews = new ObservableCollection<Review>(
                context.Reviews
                .Include(r => r.User)
                .Include(r => r.Material)
                .Where(r => !r.IsApproved)
                .OrderBy(r => r.CreatedAt)
                .ToList()
            );

            Orders = new ObservableCollection<Order>(
                context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Material)
                .OrderByDescending(o => o.OrderDate)
                .ToList()
            );
        }

        private string ProcessImage(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)) return string.Empty;
            
            if (sourcePath.StartsWith("Images\\Products\\") || sourcePath.StartsWith("Images/Products/"))
                return sourcePath;

            if (!File.Exists(sourcePath)) return sourcePath;

            try
            {
                string fileName = Path.GetFileName(sourcePath);
                string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
                
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                string destPath = Path.Combine(destDir, fileName);
                
                if (File.Exists(destPath))
                {
                    string name = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    destPath = Path.Combine(destDir, $"{name}_{Guid.NewGuid().ToString().Substring(0, 8)}{ext}");
                }

                File.Copy(sourcePath, destPath, true);
                return Path.Combine("Images", "Products", Path.GetFileName(destPath));
            }
            catch
            {
                return sourcePath;
            }
        }

        [RelayCommand]
        private void AddMaterial()
        {
            AdminMaterialMessageKey = string.Empty;
            if (string.IsNullOrWhiteSpace(EditTitleRu) || string.IsNullOrWhiteSpace(EditTitleEn) ||
                string.IsNullOrWhiteSpace(EditCategoryRu) || string.IsNullOrWhiteSpace(EditCategoryEn) ||
                string.IsNullOrWhiteSpace(EditDescriptionRu) || string.IsNullOrWhiteSpace(EditDescriptionEn))
            {
                AdminMaterialMessageKey = "EmptyFields";
                return;
            }

            using var context = new AppDbContext();

            // Проверка уникальности названия
            if (context.Materials.Any(m => m.TitleRu.ToLower() == EditTitleRu.ToLower() || m.TitleEn.ToLower() == EditTitleEn.ToLower()))
            {
                AdminMaterialMessageKey = "ErrDuplicateTitle";
                return;
            }

            if (EditPrice <= 0)
            {
                AdminMaterialMessageKey = "InvalidAmount";
                return;
            }

            var newMaterial = new Material
            {
                TitleRu = EditTitleRu,
                TitleEn = EditTitleEn,
                CategoryRu = EditCategoryRu,
                CategoryEn = EditCategoryEn,
                Price = EditPrice,
                DescriptionRu = EditDescriptionRu,
                DescriptionEn = EditDescriptionEn,
                ImagePath = ProcessImage(EditImagePath),
                StockQuantity = EditStockQuantity
            };

            context.Materials.Add(newMaterial);
            context.SaveChanges();

            TranslationSource.Instance.AddOrUpdateTranslation("RU", EditCategoryEn, EditCategoryRu);

            AdminMaterialMessageKey = "MatAdded";
            SelectedMaterial = null;
            LoadData();
        }

        [RelayCommand]
        private void UpdateMaterial()
        {
            AdminMaterialMessageKey = string.Empty;
            if (SelectedMaterial == null)
            {
                AdminMaterialMessageKey = "SelectToEdit";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditTitleRu) || string.IsNullOrWhiteSpace(EditTitleEn) ||
                string.IsNullOrWhiteSpace(EditCategoryRu) || string.IsNullOrWhiteSpace(EditCategoryEn) ||
                string.IsNullOrWhiteSpace(EditDescriptionRu) || string.IsNullOrWhiteSpace(EditDescriptionEn))
            {
                AdminMaterialMessageKey = "EmptyFields";
                return;
            }
            if (EditPrice <= 0)
            {
                AdminMaterialMessageKey = "InvalidAmount";
                return;
            }

            using var context = new AppDbContext();

            // Проверка уникальности названия (исключая текущий товар)
            if (context.Materials.Any(m => m.Id != SelectedMaterial.Id && (m.TitleRu.ToLower() == EditTitleRu.ToLower() || m.TitleEn.ToLower() == EditTitleEn.ToLower())))
            {
                AdminMaterialMessageKey = "ErrDuplicateTitle";
                return;
            }

            var material = context.Materials.Find(SelectedMaterial.Id);
            if (material != null)
            {
                bool catRuChanged = material.CategoryRu != EditCategoryRu;
                bool catEnChanged = material.CategoryEn != EditCategoryEn;

                if ((catRuChanged && !catEnChanged) || (!catRuChanged && catEnChanged))
                {
                    AdminMaterialMessageKey = "ErrCatMismatch";
                    return;
                }

                material.TitleRu = EditTitleRu;
                material.TitleEn = EditTitleEn;
                material.CategoryRu = EditCategoryRu;
                material.CategoryEn = EditCategoryEn;
                material.Price = EditPrice;
                material.DescriptionRu = EditDescriptionRu;
                material.DescriptionEn = EditDescriptionEn;
                material.ImagePath = ProcessImage(EditImagePath);
                
                int addedStock = EditStockQuantity - material.StockQuantity;
                if (addedStock > 0)
                {
                    // АЛГОРИТМ FIFO: Распределение новых поступлений по задолженностям в заказах
                    var pendingItemsWithDeficit = context.OrderItems
                        .Include(oi => oi.Order)
                        .Where(oi => oi.MaterialId == material.Id && oi.Deficit > 0)
                        .Where(oi => oi.Order.Status != OrderStatus.Completed && oi.Order.Status != OrderStatus.Cancelled)
                        .OrderBy(oi => oi.Order.OrderDate)
                        .ToList();

                    foreach (var item in pendingItemsWithDeficit)
                    {
                        int canFulfill = Math.Min(item.Deficit, addedStock);
                        item.Deficit -= canFulfill;
                        addedStock -= canFulfill;
                        
                        if (addedStock <= 0) break;
                    }
                    
                    // Только остаток после закрытия всех "дыр" идет в свободную продажу
                    material.StockQuantity += addedStock;
                }
                else
                {
                    // Если админ вручную уменьшил или не менял, просто обновляем число
                    material.StockQuantity = EditStockQuantity;
                }
                
                context.SaveChanges();
                
                TranslationSource.Instance.AddOrUpdateTranslation("RU", EditCategoryEn, EditCategoryRu);
                TranslationSource.Instance.AddOrUpdateTranslation("EN", EditCategoryEn, EditCategoryEn);
                
                AdminMaterialMessageKey = "MatUpdated";
                LoadData(); 
            }
        }

        [RelayCommand]
        private void DeleteMaterial()
        {
            AdminMaterialMessageKey = string.Empty;
            if (SelectedMaterial == null)
            {
                AdminMaterialMessageKey = "SelectToDelete";
                return;
            }

            // Список подстрок КРИТИЧЕСКИХ материалов из CalculatorService
            string[] criticalSubstrings = { 
                "soundproject tape", "шумощит 18", "Лист ГВЛ", "Подложка демпферная", 
                "seal 310", "Саморезы", "Профиль CD", "Профиль ud", "Дюбеля", 
                "экоакустик 30", "ЭкоАкустик 80", "Панель звукоизоляционная", "Лист гипса knauf", "за 88.50", "vibro p", "vibro pl", "protecktor"
            };

            bool isCritical = criticalSubstrings.Any(s => 
                SelectedMaterial.TitleRu.ToLower().Contains(s.ToLower()) || 
                SelectedMaterial.TitleEn.ToLower().Contains(s.ToLower()));

            if (isCritical)
            {
                AdminMaterialMessageKey = "ErrCriticalMaterial";
                return;
            }

            using var context = new AppDbContext();
            var material = context.Materials.Find(SelectedMaterial.Id);
            if (material != null)
            {
                // Удаляем из корзины текущего сеанса (если он там есть)
                CartService.Instance.Remove(material.Id);

                context.Materials.Remove(material);
                context.SaveChanges();
                
                SelectedMaterial = null;
                AdminMaterialMessageKey = "MatDeleted";
                LoadData();
            }
        }

        [RelayCommand]
        private void ApproveReview()
        {
            AdminReviewMessageKey = string.Empty;
            if (SelectedPendingReview != null)
            {
                using var context = new AppDbContext();
                var rev = context.Reviews.Find(SelectedPendingReview.Id);
                if (rev != null)
                {
                    rev.IsApproved = true;
                    context.SaveChanges();
                    
                    AdminReviewMessageKey = "RevApproved";
                    LoadData();
                }
            }
        }

        [RelayCommand]
        private void RejectReview()
        {
            AdminReviewMessageKey = string.Empty;
            if (SelectedPendingReview != null)
            {
                using var context = new AppDbContext();
                var rev = context.Reviews.Find(SelectedPendingReview.Id);
                if (rev != null)
                {
                    context.Reviews.Remove(rev);
                    context.SaveChanges();
                    
                    AdminReviewMessageKey = "RevRejected";
                    LoadData();
                }
            }
        }

        [RelayCommand]
        private void UpdateOrderStatus()
        {
            AdminOrderMessageKey = string.Empty;
            if (SelectedOrder == null) return;

            // ПРОВЕРКА: Если заказ уже завершен или отменен, менять его нельзя
            if (SelectedOrder.Status == OrderStatus.Completed || SelectedOrder.Status == OrderStatus.Cancelled)
            {
                AdminOrderMessageKey = "ErrOrderLocked"; 
                return;
            }

            using var context = new AppDbContext();
            var order = context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Material)
                .FirstOrDefault(o => o.Id == SelectedOrder.Id);

            if (order != null)
            {
                bool statusChanged = order.Status != SelectedOrderStatus;
                bool executorChanged = order.ExecutorId != SelectedExecutor?.Id;

                // ПРОВЕРКА ДЕФИЦИТА при попытке перевести из Pending в рабочий статус
                if (order.Status == OrderStatus.Pending && 
                    SelectedOrderStatus != OrderStatus.Pending && 
                    SelectedOrderStatus != OrderStatus.Cancelled)
                {
                    if (order.Items.Any(i => i.Deficit > 0))
                    {
                        AdminOrderMessageKey = "ErrInsufficientStockForStatus";
                        return;
                    }
                }

                // НАЧИСЛЕНИЕ ЗАРПЛАТЫ: Если статус меняется на Completed
                if (statusChanged && SelectedOrderStatus == OrderStatus.Completed)
                {
                    // Важно: берем мастера из формы (SelectedExecutor), так как админ мог его только что выбрать
                    int? targetMasterId = SelectedExecutor?.Id ?? order.ExecutorId;
                    
                    if (targetMasterId != null)
                    {
                        var dbMaster = context.Users.Find(targetMasterId);
                        if (dbMaster != null)
                        {
                            decimal pay = CalculateEarnings(order);
                            if (pay > 0)
                            {
                                dbMaster.TotalEarnings += pay;
                            }
                            order.CompletionDate = DateTime.Now;
                            
                            // Синхронизация сессии, если админ и есть мастер
                            if (SessionManager.Instance.CurrentUser != null && SessionManager.Instance.CurrentUser.Id == dbMaster.Id)
                            {
                                SessionManager.Instance.CurrentUser.TotalEarnings = dbMaster.TotalEarnings;
                            }
                        }
                    }
                }

                // ВОЗВРАТ МАТЕРИАЛОВ: Если заказ отменяется
                if (statusChanged && SelectedOrderStatus == OrderStatus.Cancelled)
                {
                    foreach (var item in order.Items)
                    {
                        if (item.Material != null)
                        {
                            // Возвращаем на склад только то количество, которое реально было списано (Quantity минус Deficit)
                            int reserved = item.Quantity - item.Deficit;
                            if (reserved > 0)
                            {
                                item.Material.StockQuantity += reserved;
                                item.Deficit = item.Quantity; // Помечаем, что товар больше не зарезервирован
                            }
                        }
                    }
                }

                order.Status = SelectedOrderStatus;
                order.ExecutorId = SelectedExecutor?.Id;

                if (executorChanged && order.ExecutorId != null)
                {
                    if (order.Status == OrderStatus.Pending)
                        order.Status = OrderStatus.Processing;
                    
                    CalculateOrderSchedule(order, context);
                }

                context.SaveChanges();
                
                // Отправка email при смене статуса
                if (statusChanged)
                {
                    var customer = context.Users.Find(order.UserId);
                    if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
                    {
                        string translatedStatus = TranslationSource.Instance[order.Status.ToString()];
                        EmailService.SendOrderNotification(customer.Email, order.Id, translatedStatus);
                    }
                }
                
                AdminOrderMessageKey = "OrderUpdated";
                LoadData();
            }
        }

        private decimal CalculateEarnings(Order order)
        {
            if (order == null || order.Area <= 0 || string.IsNullOrEmpty(order.SurfaceType)) 
                return 0;

            decimal rate = order.SurfaceType switch
            {
                "Wall" => 50m,
                "Floor" => 30m,
                "Ceiling" => 70m,
                _ => 0m
            };

            return Math.Round(rate * (decimal)order.Area, 2);
        }

        private void CalculateOrderSchedule(Order order, AppDbContext context)
        {
            if (order.ExecutorId == null) return;

            // Находим последний заказ этого мастера
            var lastOrder = context.Orders
                .Where(o => o.ExecutorId == order.ExecutorId && o.Id != order.Id && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing))
                .OrderByDescending(o => o.ScheduledEndDate)
                .FirstOrDefault();

            DateTime startDate = lastOrder?.ScheduledEndDate?.AddDays(1) ?? DateTime.Now.Date;
            startDate = GetNextWorkDay(startDate);

            double totalHours = order.SurfaceType switch
            {
                "Floor" => order.Area * 1,
                "Wall" => order.Area * 3,
                "Ceiling" => order.Area * 4,
                _ => 8 // По дефолту 1 день
            };

            int totalDays = (int)Math.Ceiling(totalHours / 8.0);
            if (totalDays < 1) totalDays = 1;

            order.ScheduledStartDate = startDate;
            order.ScheduledEndDate = startDate;

            for (int i = 1; i < totalDays; i++)
            {
                order.ScheduledEndDate = GetNextWorkDay(order.ScheduledEndDate.Value.AddDays(1));
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
    }
}
