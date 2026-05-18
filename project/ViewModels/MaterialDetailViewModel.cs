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
    public partial class MaterialDetailViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public MaterialDetailViewModel(MainViewModel mainViewModel, Material material)
        {
            _mainViewModel = mainViewModel;
            Material = material;
            LoadReviews();

            TranslationSource.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item" || e.PropertyName == "Item[]" || e.PropertyName == "CurrentLanguage")
                {
                    OnPropertyChanged(nameof(Message));
                }
            };
        }

        [ObservableProperty]
        private Material _material;

        [ObservableProperty]
        private ObservableCollection<Review> _reviews = new();

        [ObservableProperty]
        private string _newReviewText = string.Empty;

        public TranslationSource Loc => TranslationSource.Instance;

        [ObservableProperty]
        private string _newReviewRatingText = "5";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Message))]
        private string _messageKey = string.Empty;

        public string Message => string.IsNullOrEmpty(MessageKey) ? string.Empty : Loc[MessageKey];

        private void LoadReviews()
        {
            using var context = new AppDbContext();
            var revs = context.Reviews
                .Include(r => r.User)
                .Where(r => r.MaterialId == Material.Id && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            Reviews = new ObservableCollection<Review>(revs);
        }

        [RelayCommand]
        private void SubmitReview()
        {
            if (!SessionManager.Instance.IsLoggedIn)
            {
                MessageKey = "ErrLoginToReview";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewReviewText))
            {
                MessageKey = "ErrEmptyReview";
                return;
            }

            if (!int.TryParse(NewReviewRatingText, out int rating) || rating < 1 || rating > 5)
            {
                MessageKey = "ErrInvalidRating";
                return;
            }

            using var context = new AppDbContext();
            var review = new Review
            {
                UserId = SessionManager.Instance.CurrentUser.Id,
                MaterialId = Material.Id,
                Text = NewReviewText,
                Rating = rating,
                IsApproved = false, // Модерация
                CreatedAt = System.DateTime.Now
            };

            context.Reviews.Add(review);
            context.SaveChanges();

            NewReviewText = string.Empty;
            NewReviewRatingText = "5";
            MessageKey = "SuccessReview";
        }

        [RelayCommand]
        private void GoBack()
        {
            _mainViewModel.GoToCatalog();
        }
    }
}
