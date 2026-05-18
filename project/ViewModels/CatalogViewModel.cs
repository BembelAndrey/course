using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project.Data;
using project.Models;
using project.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace project.ViewModels
{
    public partial class CatalogViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private List<Material> _allMaterials;

        public CatalogViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoadMaterials();
            TranslationSource.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item[]" || e.PropertyName == "CurrentLanguage")
                {
                    OnPropertyChanged(nameof(Categories));
                    // Force refresh of the collection bindings
                    var temp = FilteredMaterials;
                    FilteredMaterials = null;
                    FilteredMaterials = temp;
                }
            };
        }

        public TranslationSource Loc => TranslationSource.Instance;

        [ObservableProperty]
        private ObservableCollection<Material> _filteredMaterials = new();

        [ObservableProperty]
        private ObservableCollection<string> _categories = new();

        [ObservableProperty]
        private string _selectedCategory = "AllCategories";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isQuickBuyOpen;

        [ObservableProperty]
        private Material _quickBuyMaterial;

        [ObservableProperty]
        private string _quickBuyQuantityText = "1";

        [ObservableProperty]
        private string _quickBuyAddress = string.Empty;

        [ObservableProperty]
        private string _quickBuyMessage = string.Empty;

        partial void OnSelectedCategoryChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        private void LoadMaterials()
        {
            using var context = new AppDbContext();
            _allMaterials = context.Materials.ToList();

            var distinctCategories = _allMaterials.Where(m => m.CategoryEn != "CatServices").Select(m => m.CategoryEn).Distinct().ToList();
            
            // Динамическое обновление переводов для новых категорий
            foreach (var mat in _allMaterials)
            {
                if (!string.IsNullOrWhiteSpace(mat.CategoryEn))
                {
                    TranslationSource.Instance.AddOrUpdateTranslation("RU", mat.CategoryEn, mat.CategoryRu);
                    TranslationSource.Instance.AddOrUpdateTranslation("EN", mat.CategoryEn, mat.CategoryEn);
                }
            }

            distinctCategories.Insert(0, "AllCategories");
            Categories = new ObservableCollection<string>(distinctCategories);

            SelectedCategory = "AllCategories";
            SearchText = string.Empty;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allMaterials == null) return;

            var filtered = _allMaterials.Where(m => m.CategoryEn != "CatServices").AsQueryable();

            if (SelectedCategory != "AllCategories")
            {
                filtered = filtered.Where(m => m.CategoryEn == SelectedCategory);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lowerSearch = SearchText.ToLower();
                filtered = filtered.Where(m => 
                    m.TitleRu.ToLower().Contains(lowerSearch) || 
                    m.TitleEn.ToLower().Contains(lowerSearch) || 
                    m.DescriptionRu.ToLower().Contains(lowerSearch) || 
                    m.DescriptionEn.ToLower().Contains(lowerSearch));
            }

            FilteredMaterials = new ObservableCollection<Material>(filtered.ToList());
        }

        [RelayCommand]
        private void GoToDetail(Material selectedMaterial)
        {
            if (selectedMaterial != null)
            {
                _mainViewModel.CurrentViewModel = new MaterialDetailViewModel(_mainViewModel, selectedMaterial);
            }
        }

        [RelayCommand]
        private void OpenQuickBuy(Material m)
        {
            if (!SessionManager.Instance.IsLoggedIn)
            {
                _mainViewModel.CurrentViewModel = new LoginViewModel(_mainViewModel);
                return;
            }
            QuickBuyMaterial = m;
            IsQuickBuyOpen = true;
            QuickBuyQuantityText = "1";
            QuickBuyAddress = string.Empty;
            QuickBuyMessage = string.Empty;
        }

        [RelayCommand]
        private void CloseQuickBuy() => IsQuickBuyOpen = false;

        [RelayCommand]
        private void ConfirmQuickBuy()
        {
            if (!int.TryParse(QuickBuyQuantityText, out int qty) || qty <= 0)
            {
                QuickBuyMessage = Loc["InvalidQty"];
                return;
            }

            using var context = new AppDbContext();
            var material = context.Materials.Find(QuickBuyMaterial.Id);
            if (material != null)
            {
                if (material.StockQuantity < qty)
                {
                    QuickBuyMessage = Loc["InsufficientStock"] + " " + material.StockQuantity;
                    return;
                }

                CartService.Instance.Add(material, qty);
                QuickBuyMessage = Loc["AddedToCartSuccess"];
                
                // Hide overlay after 1 second automatically
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsQuickBuyOpen = false;
                    });
                });
            }
        }
    }
}
