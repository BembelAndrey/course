using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project.Data;
using project.Models;
using project.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace project.ViewModels
{
    public partial class CartViewModel : ObservableObject
    {
        public TranslationSource Loc => TranslationSource.Instance;

        [ObservableProperty]
        private ObservableCollection<CartItem> _items;

        [ObservableProperty]
        private decimal _totalCost;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Message))]
        private string _messageKey = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Message))]
        private string _messageParam = string.Empty;

        public string Message => string.IsNullOrEmpty(MessageKey) ? string.Empty : 
            (string.IsNullOrEmpty(MessageParam) ? Loc[MessageKey] : $"{Loc[MessageKey]} {MessageParam}");

        [ObservableProperty]
        private string _address = string.Empty;

        public CartViewModel()
        {
            LoadCart();
            TranslationSource.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item" || e.PropertyName == "Item[]" || e.PropertyName == "CurrentLanguage")
                {
                    OnPropertyChanged(nameof(TotalCost));
                    OnPropertyChanged(nameof(Message));
                }
            };
        }

        private void LoadCart()
        {
            Items = new ObservableCollection<CartItem>(CartService.Instance.Items);
            TotalCost = CartService.Instance.TotalCost;
        }

        [RelayCommand]
        private void RemoveFromCart(CartItem item)
        {
            if (item != null)
            {
                CartService.Instance.Remove(item.Material.Id);
                LoadCart();
            }
        }

        [RelayCommand]
        private void Checkout()
        {
            if (!CartService.Instance.Items.Any())
            {
                MessageKey = "ErrEmptyFields";
                MessageParam = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                MessageKey = "EmptyAddress";
                MessageParam = string.Empty;
                return;
            }

            using var context = new AppDbContext();

            // Проверка наличия
            foreach (var item in CartService.Instance.Items)
            {
                var mat = context.Materials.Find(item.Material.Id);
                if (mat == null || mat.StockQuantity < item.Quantity)
                {
                    string matTitle = Loc.CurrentLanguage == "RU" ? mat?.TitleRu : mat?.TitleEn;
                    MessageKey = "InsufficientStockShort";
                    MessageParam = $"{matTitle ?? ""} ({mat?.StockQuantity ?? 0})";
                    return;
                }
            }

            var order = new Order
            {
                UserId = SessionManager.Instance.CurrentUser.Id,
                TotalCost = CartService.Instance.TotalCost,
                OrderName = "CatalogOrder", // Сохраняем КЛЮЧ
                Address = Address,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.Now
            };

            foreach (var item in CartService.Instance.Items)
            {
                var mat = context.Materials.Find(item.Material.Id);
                if (mat != null)
                {
                    mat.StockQuantity -= item.Quantity; // вычитаем со склада

                    order.Items.Add(new OrderItem
                    {
                        MaterialId = item.Material.Id,
                        Quantity = item.Quantity,
                        Price = item.Material.Price
                    });
                }
            }
            context.Orders.Add(order);
            context.SaveChanges();
            CartService.Instance.Clear();
            LoadCart();

            MessageKey = "SuccessOrder";
            MessageParam = $"{order.TotalCost} $";
        }
    }
}
