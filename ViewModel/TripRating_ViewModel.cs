using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
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

        public TripRating_ViewModel()
        {
            userService = new UserService();
            fellowTravelerService = new FellowTravelerService();
            tripService = new TripService();
        }

        [ObservableProperty]
        private ObservableCollection<FellowTravelerModel> usersForEstimate = new();

        [ObservableProperty]
        private bool isReviewFormOpen = false;

        [ObservableProperty]
        private bool isUserProfileShow = false;

        [ObservableProperty]
        private FellowTravelerModel selectedUser;


        [RelayCommand]
        public async Task LoadTravelers()
        {
            // Проверяем, загружены ли данные уже. 
            // Если список не пуст, выходим, чтобы не дублировать данные при повторном вызове.
            if (UsersForEstimate.Any()) return;

            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                string currentUserId = Preferences.Get("CurrentUserKey", "");

                var allUsers = await userService.GetUsers();
                var allFellowTravelers = await fellowTravelerService.GetFellowTravelers();
                var allTrips = await tripService.GetTrips();

                var currentTrip = allTrips.FirstOrDefault(t => t.Id == tripId);

                // Получаем ID пользователя, которого сейчас оцениваем (чтобы пропустить его)
                string excludeId = SelectedUser?.FellowUserId;

                // Создатель поездки
                if (currentTrip.UserId != currentUserId && currentTrip.UserId != excludeId)
                {
                    var driverUser = allUsers.FirstOrDefault(u => u.Id == currentTrip.UserId);
                    UsersForEstimate.Add(new FellowTravelerModel 
                    {
                        FellowUserId = currentTrip.UserId,
                        UserFellowName = driverUser?.Name ?? "Водитель",
                        UserFellowAvatar = driverUser?.ProfilePhoto ?? "defoltavataricon.png",
                        UserFellowRating = driverUser?.Rating ?? 0.00
                    });
                }

                // Фильтруем попутчиков: берем тех, кто в этой поездке и это не мы и не выбранный пользователь
                var fellowTravelers = allFellowTravelers.Where(f => f.TripId == tripId && f.FellowUserId != currentUserId && f.FellowUserId != excludeId).ToList(); // Исключаем выбранного

                // Добавляем попутчика в список
                foreach (var ft in fellowTravelers)
                {
                    var user = allUsers.FirstOrDefault(u => u.Id == ft.FellowUserId);

                    // Наполняем модель данными пользователя
                    ft.UserFellowName = user?.Name ?? "Попутчик";
                    ft.UserFellowAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                    ft.UserFellowRating = user?.Rating ?? 0.00;

                    UsersForEstimate.Add(ft);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить данные!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
            }
        }


        // Метод обновления списка, который исключает текущего выбранного пользователя
        private void RefreshFilteredList()
        {
            // Мы не очищаем UsersForEstimate, а просто вызываем LoadTravelers, 
            // но в LoadTravelers добавим проверку на SelectedUser.Id
        }


        // Открытие формы
        [RelayCommand]
        private void OpenReviewForm(FellowTravelerModel user)
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
            bool send = await Shell.Current.DisplayAlertAsync("Внимание", "Отзыв будет опубликован необратимо. Внести изменения или удалить его после подтверждения будет невозможно!", "Отправить", "Отменить");
            if (!send) return;

            IsReviewFormOpen = false;
            string lastSelectedId = SelectedUser?.FellowUserId;
            SelectedUser = null;      // Очищаем выбор

            // После закрытия формы снова обновляем список, чтобы вернуть пользователя
            UsersForEstimate.Clear();
            LoadTravelers();
        }


        // К профилю пользователя
        [RelayCommand]
        private async Task GoCheckProfile(FellowTravelerModel fellowTraveler)
        {
            try
            {
                Preferences.Set("SelectedUserIdForCheckProfile", fellowTraveler.FellowUserId);
                Preferences.Set("PreviousPageCheckProfile", "FellowTravelerPage");
                await Shell.Current.GoToAsync("//CheckProfilePage");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось открыть старницу!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "Ок"); }
        }

        [RelayCommand]
        public async Task GoBackTripDetails()
        {
            IsReviewFormOpen = false;
            string lastSelectedId = SelectedUser?.FellowUserId;
            SelectedUser = null;      // Очищаем выбор

            // После закрытия формы снова обновляем список, чтобы вернуть пользователя
            UsersForEstimate.Clear();
            LoadTravelers();
            await Shell.Current.GoToAsync("//TripDetailsPage");
        }
    }
}
