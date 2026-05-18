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
    public partial class CalculatorViewModel : ObservableObject
    {
        private readonly CalculatorService _calcService = new();

        public TranslationSource Loc => TranslationSource.Instance;

        public CalculatorViewModel()
        {
            SurfaceTypes = new ObservableCollection<string> { "SurfWall", "SurfFloor", "SurfCeiling" };
            SelectedSurfaceType = "SurfWall"; 
            TranslationSource.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item" || e.PropertyName == "Item[]" || e.PropertyName == "CurrentLanguage")
                {
                    var selSurf = SelectedSurfaceType;
                    var selLvl = SelectedWorkLevel;

                    SurfaceTypes = new ObservableCollection<string>(SurfaceTypes.ToList());
                    SelectedSurfaceType = selSurf;

                    WorkLevels = new ObservableCollection<string>(WorkLevels.ToList());
                    SelectedWorkLevel = selLvl;
                    
                    Calculate();
                    OnPropertyChanged(nameof(OrderMessage));
                }
            };
        }

        [ObservableProperty]
        private ObservableCollection<string> _surfaceTypes;

        [ObservableProperty]
        private string _selectedSurfaceType = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _workLevels = new();

        [ObservableProperty]
        private string _selectedWorkLevel = string.Empty;

        [ObservableProperty]
        private string _areaText = "10";

        [ObservableProperty]
        private decimal _totalCost;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OrderMessage))]
        private string _messageKey = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OrderMessage))]
        private string _messageParam = string.Empty;

        public string OrderMessage => string.IsNullOrEmpty(MessageKey) ? string.Empty : 
            (string.IsNullOrEmpty(MessageParam) ? Loc[MessageKey] : $"{Loc[MessageKey]} {MessageParam}");

        private string MapSurfToId(string viewKey) => viewKey switch
        {
            "SurfWall" => CalculatorService.SurfWall,
            "SurfFloor" => CalculatorService.SurfFloor,
            "SurfCeiling" => CalculatorService.SurfCeiling,
            _ => viewKey
        };

        private string MapLvlToId(string viewKey) => viewKey switch
        {
            "LvlEco" => CalculatorService.LvlEco,
            "LvlComf" => CalculatorService.LvlComf,
            "LvlStd" => CalculatorService.LvlStd,
            "LvlPrem" => CalculatorService.LvlPrem,
            _ => viewKey
        };

        private string MapIdToLvlKey(string id) => id switch
        {
            CalculatorService.LvlEco => "LvlEco",
            CalculatorService.LvlComf => "LvlComf",
            CalculatorService.LvlStd => "LvlStd",
            CalculatorService.LvlPrem => "LvlPrem",
            _ => id
        };

        partial void OnSelectedSurfaceTypeChanged(string value)
        {
            WorkLevels.Clear();
            var levels = _calcService.GetLevelsForSurface(MapSurfToId(value));
            foreach (var lvl in levels)
            {
                WorkLevels.Add(MapIdToLvlKey(lvl));
            }
            SelectedWorkLevel = WorkLevels.FirstOrDefault() ?? string.Empty;
            Calculate();
        }

        partial void OnSelectedWorkLevelChanged(string value)
        {
            Calculate();
        }

        partial void OnAreaTextChanged(string value)
        {
            Calculate();
        }

        [RelayCommand]
        private void Calculate()
        {
            if (double.TryParse(AreaText, out double area) && area > 0)
            {
                TotalCost = _calcService.CalculateWorkCost(MapSurfToId(SelectedSurfaceType), MapLvlToId(SelectedWorkLevel), area);
                MessageKey = string.Empty;
                MessageParam = string.Empty;
            }
            else
            {
                TotalCost = 0;
            }
        }

        [RelayCommand]
        private void OrderService()
        {
            if (!SessionManager.Instance.IsLoggedIn)
            {
                MessageKey = "LoginReqCalc";
                MessageParam = string.Empty;
                return;
            }

            if (!double.TryParse(AreaText, out double area) || area <= 0)
            {
                MessageKey = "InvalidArea";
                MessageParam = string.Empty;
                return;
            }

            if (TotalCost <= 0)
            {
                MessageKey = "InvalidAmount";
                MessageParam = string.Empty;
                return;
            }

            using var context = new AppDbContext();
            
            string surfRu = Loc.CurrentLanguage == "RU" ? Loc[SelectedSurfaceType] : TranslationSource.Instance["RU", SelectedSurfaceType];
            string lvlRu = Loc.CurrentLanguage == "RU" ? Loc[SelectedWorkLevel] : TranslationSource.Instance["RU", SelectedWorkLevel];

            string surfEn = Loc.CurrentLanguage == "EN" ? Loc[SelectedSurfaceType] : TranslationSource.Instance["EN", SelectedSurfaceType];
            string lvlEn = Loc.CurrentLanguage == "EN" ? Loc[SelectedWorkLevel] : TranslationSource.Instance["EN", SelectedWorkLevel];

            string serviceNameRu = $"Монтаж звукоизоляции: {surfRu} ({lvlRu}), {area} кв.м.";
            string serviceNameEn = $"Soundproofing installation: {surfEn} ({lvlEn}), {area} sq.m.";
            
            var serviceMaterial = context.Materials.FirstOrDefault(m => m.TitleRu == serviceNameRu);
            if (serviceMaterial == null)
            {
                serviceMaterial = new Material 
                { 
                    TitleRu = serviceNameRu, 
                    TitleEn = serviceNameEn,
                    CategoryRu = "Услуги монтажа", 
                    CategoryEn = "CatServices",
                    Price = TotalCost, 
                    DescriptionRu = "Комплексная работа под ключ.",
                    DescriptionEn = "Turnkey complex work."
                };
                context.Materials.Add(serviceMaterial);
                context.SaveChanges();
            }

            var order = new Order
            {
                UserId = SessionManager.Instance.CurrentUser.Id,
                TotalCost = TotalCost,
                OrderName = $"CalcOrder|{SelectedSurfaceType}|{SelectedWorkLevel}", // КЛЮЧИ
                Area = area,
                SurfaceType = MapSurfToId(SelectedSurfaceType),
                Status = OrderStatus.Pending,
                OrderDate = DateTime.Now
            };

            // Добавляем саму услугу как элемент заказа
            order.Items.Add(new OrderItem
            {
                MaterialId = serviceMaterial.Id,
                Quantity = 1,
                Price = TotalCost
            });

            // Списываем комплектующие и добавляем их в заказ с нулевой стоимостью
            var bomItems = _calcService.DeductMaterialsForService(MapSurfToId(SelectedSurfaceType), MapLvlToId(SelectedWorkLevel), area, context);
            foreach (var bomItem in bomItems)
            {
                order.Items.Add(new OrderItem
                {
                    MaterialId = bomItem.MaterialId,
                    Quantity = bomItem.Quantity,
                    Deficit = bomItem.Deficit, // ВАЖНО: сохраняем дефицит в БД
                    Price = 0 // Стоимость уже включена в TotalCost
                });
            }

            context.Orders.Add(order);
            context.SaveChanges();

            MessageKey = "SuccessOrder";
            MessageParam = $"{TotalCost} $";
        }
    }
}
