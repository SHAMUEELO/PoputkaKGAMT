using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using PoputkaKGAMT.ViewModel;
using System.Collections.ObjectModel;

namespace PoputkaKGAMT.ViewModel
{
    partial class TripRating_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");
        private readonly UserService userService;
        private readonly FellowTravelerService fellowTravelerService;
        private readonly TripService tripService;
        private readonly RatingServices ratingServices;

        public TripRating_ViewModel()
        {
            userService = new UserService();
            fellowTravelerService = new FellowTravelerService();
            tripService = new TripService();
            ratingServices = new RatingServices();
        }

        [ObservableProperty]
        private ObservableCollection<TripRatingModel> usersForEstimate = new();

        [ObservableProperty]
        private bool isReviewFormOpen = false;

        [ObservableProperty]
        private bool isUserProfileShow = false;

        [ObservableProperty]
        private string review = "";

        [ObservableProperty]
        private int estimate;

        [ObservableProperty]
        private TripRatingModel selectedUser;

        // Таймер
        private System.Threading.Timer? _reloadTimer;

        // Загружаем
        [RelayCommand]
        public async Task LoadTravelers()
        {
            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                string currentUserId = Preferences.Get("CurrentUserKey", "");

                var allUsers = await userService.GetUsers();
                var allFellowTravelers = await fellowTravelerService.GetFellowTravelers();
                var allTrips = await tripService.GetTrips();
                var allRating = await ratingServices.GetRatingDates();

                var currentTrip = allTrips.FirstOrDefault(t => t.Id == tripId);

                // Получаем ID пользователя, которого сейчас оцениваем (чтобы пропустить его)
                string excludeId = SelectedUser?.RecipientUserId;

                // Собираем новые данные в временный список при обновлении с писка таймером
                var newUsersForEstimate = new List<TripRatingModel>();

                // Создатель поездки
                if (currentTrip?.UserId != currentUserId && currentTrip.UserId != excludeId)
                {
                    var driverUser = allUsers.FirstOrDefault(u => u.Id == currentTrip.UserId);
                    var driverReview = allRating.FirstOrDefault(r => r.TripId == tripId && r.AppraiserUserId == currentUserId && r.RecipientUserId == currentTrip.UserId);
                   
                    newUsersForEstimate.Add(new TripRatingModel
                    {
                        RecipientUserId = currentTrip.UserId,
                        UserName = driverUser?.Name ?? "Водитель",
                        UserRating = driverUser?.Rating ?? 0.0,
                        Review = driverReview?.Review ?? "",
                        Created = driverReview?.Created ?? "",
                        Estimate = driverReview?.Estimate ?? 0,
                        UserHaveReview = driverReview != null // Проверяем, ест ли отзыв для текущего пользователя
                    }); 
                }

                // Фильтруем попутчиков: берем тех, кто в этой поездке и это не мы и не выбранный пользователь
                var fellowTravelers = allFellowTravelers.Where(f => f.TripId == tripId && f.FellowUserId != currentUserId && f.FellowUserId != excludeId).ToList(); // Исключаем выбранного

                // Добавляем попутчика в список
                foreach (var ft in fellowTravelers)
                {
                    var user = allUsers.FirstOrDefault(u => u.Id == ft.FellowUserId);
                    var review = allRating.FirstOrDefault(r => r.TripId == tripId && r.AppraiserUserId == currentUserId && r.RecipientUserId == ft.FellowUserId);

                    // Наполняем модель данными пользователя
                    newUsersForEstimate.Add(new TripRatingModel
                    {
                        RecipientUserId = ft.FellowUserId,
                        UserName = user?.Name ?? "Попутчик",
                        UserRating = user?.Rating ?? 0.0,
                        Review = review?.Review ?? "",
                        Created = review?.Created ?? "",
                        Estimate = review?.Estimate ?? 0,
                        UserHaveReview = review != null // Проверяем, ест ли отзыв для текущего пользователя
                    }); 
                }

                // Сравниваем с текущим списком
                var existingIds = UsersForEstimate.Select(u => u.RecipientUserId).ToHashSet();

                // Удаляем лишние(которых нет в новых данных)
                var toRemove = UsersForEstimate.Where(u => !newUsersForEstimate.Any(nu => nu.RecipientUserId == u.RecipientUserId)).ToList();
                foreach (var u in toRemove)
                    UsersForEstimate.Remove(u);

                // Обновляем или добавляем
                foreach (var newUser in newUsersForEstimate)
                {
                    var existing = UsersForEstimate.FirstOrDefault(u => u.RecipientUserId == newUser.RecipientUserId);
                    if (existing != null)
                    {
                        existing.UserName = newUser.UserName;
                        existing.UserRating = newUser.UserRating;
                        existing.Review = newUser.Review;
                        existing.Created = newUser.Created;
                        existing.Estimate = newUser.Estimate;
                        existing.UserHaveReview = newUser.UserHaveReview;
                    }
                    else
                    {
                        UsersForEstimate.Add(newUser);
                    }
                }

                StartReloadTimer();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить данные!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
            }
        }

        // Логика таймера
        private void StartReloadTimer()
        {
            _reloadTimer?.Dispose();

            _reloadTimer = new System.Threading.Timer(async _ =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (IsReviewFormOpen == false) // Только если форма закрыта
                    {
                        await LoadTravelersCommand.ExecuteAsync(null);
                    }
                });
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        private void StopReloadTimer()
        {
            _reloadTimer?.Dispose();
            _reloadTimer = null;
        }


        // Открытие формы
        [RelayCommand]
        private void OpenReviewForm(TripRatingModel user)
        {
            SelectedUser = user;      // Сохраняем, кого оцениваем
            IsReviewFormOpen = true;

            // Перезагружаем список, чтобы выбранный пользователь исчез из него
            UsersForEstimate.Clear();
            LoadTravelers();

        }

        // закрытие формы
        [RelayCommand]
        private async Task CloseReviewFormAndSendData()
        {
            if (Estimate == 0 || Estimate == null)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Оцените качество поездки для попутчика", "OK");
                return;
            }

            bool send = await Shell.Current.DisplayAlertAsync("Внимание", "Отзыв будет опубликован необратимо. Внести изменения или удалить его после подтверждения будет невозможно!", "Отправить", "Отменить");
            if (!send) return;

            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                string currentUserId = Preferences.Get("CurrentUserKey", "");

                // сохранение пользователя в FirebaseЫ
                var review = new
                {
                    id = "",
                    trip_id = tripId, // id поездки
                    appraiser_user_id = currentUserId, // кто оценивает
                    recipient_user_id = SelectedUser.RecipientUserId, // кто получает отзыв
                    estimate = Estimate, // Оценка
                    review = Review, // отзыв
                    createdAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                };

                // Сохраняем в trips
                var result = await firebase.Child("trip_reviews").PostAsync(review);
                await firebase.Child("trip_reviews").Child(result.Key).PatchAsync(new { id = result.Key });

                Estimate = 0;
                Review = "";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
            }

            IsReviewFormOpen = false;
            string lastSelectedId = SelectedUser?.RecipientUserId;
            SelectedUser = null;      // Очищаем выбор

            // После закрытия формы снова обновляем список, чтобы вернуть пользователя
            UsersForEstimate.Clear();
            LoadTravelers();
        }


        // К профилю пользователя
        [RelayCommand]
        private async Task GoCheckProfile(TripRatingModel fellowTraveler)
        {
            try
            {
                StopReloadTimer();
                Preferences.Set("SelectedUserIdForCheckProfile", fellowTraveler.RecipientUserId);
                Preferences.Set("PreviousPageCheckProfile", "TripRatingPage");
                await Shell.Current.GoToAsync("//CheckProfilePage");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось открыть старницу!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "Ок"); }
        }

        [RelayCommand]
        public async Task GoBackTripDetails()
        {
            StopReloadTimer();
            IsReviewFormOpen = false;
            string lastSelectedId = SelectedUser?.RecipientUserId;
            SelectedUser = null;      // Очищаем выбор

            // После закрытия формы снова обновляем список, чтобы вернуть пользователя
            UsersForEstimate.Clear();
            LoadTravelers();
            await Shell.Current.GoToAsync("//TripDetailsPage");
        }
    }
}
