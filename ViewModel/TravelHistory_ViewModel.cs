using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Collections.ObjectModel;
using System.Globalization;


namespace PoputkaKGAMT.ViewModel
{
    public partial class TravelHistory_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        private readonly FellowTravelerService fellowtravelerService;
        private readonly TripService tripService;
        private readonly UserService userService;

        public TravelHistory_ViewModel()
        {
            fellowtravelerService = new FellowTravelerService();
            tripService = new TripService();
            userService = new UserService();
        }


        [ObservableProperty]
        private ObservableCollection<TripModel> allHistoryTrips = new();

        [ObservableProperty]
        private bool atLeastOneTrip = false;

        [ObservableProperty]
        private bool isTripsEmpty = true;

        [ObservableProperty]
        private int sortCounter = 0;  // Счетчик

        [ObservableProperty]
        private bool isFirstSort = true;

        private System.Threading.Timer? _reloadTimer; // Таймер на 2 сек.

        [RelayCommand]
        public async Task LoadTrip()
        {
            try
            {
                // Система изменения статуса
                var allFellowTravelersCheck = await fellowtravelerService.GetFellowTravelers();
                var allTripsCheck = await tripService.GetTrips();
                var now = DateTime.Now;

                foreach (var trip in allTripsCheck.Where(t => t.StatusId == "3").ToList())
                {
                    string dateTimeStr = $"{trip.Date} {trip.Time}";
                    if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDateTime))
                    {
                        if (now >= tripDateTime)
                        {
                            // Удаляем ожидания status_id="5"
                            var fellowsToDelete = allFellowTravelersCheck.Where(f => f.TripId == trip.Id && f.StatusId == "5").ToList();

                            if (fellowsToDelete.Any())
                            {
                                await Task.WhenAll(fellowsToDelete.Select(fellow => firebase.Child("fellow_travelers").Child(fellow.Id).DeleteAsync()));
                            }

                            if (trip.SeatsQuentity == trip.OriginalSeatsQuentity)
                            {
                                // Удаляем поездку
                                await firebase.Child("trips").Child(trip.Id).DeleteAsync();
                            }
                            else await firebase.Child("trips").Child(trip.Id).PatchAsync(new { status_id = "2" });


                        }
                    }

                }

                // Если status="1" ИЛИ "2" + пустые, то удалить
                foreach (var trip in allTripsCheck.Where(t => (t.StatusId == "1" || t.StatusId == "2") && t.SeatsQuentity == t.OriginalSeatsQuentity).ToList())
                {
                    // Удаляем поездку
                    await firebase.Child("trips").Child(trip.Id).DeleteAsync();

                    // Удаляем ВСЕХ попутчиков
                    var allFellowsThisTrip = allFellowTravelersCheck.Where(f => f.TripId == trip.Id).ToList();
                    if (allFellowsThisTrip.Any())
                    {
                        await Task.WhenAll(allFellowsThisTrip.Select(f => firebase.Child("fellow_travelers").Child(f.Id).DeleteAsync()));
                    }
                }


                string myId = Preferences.Get("CurrentUserKey", "");

                var allTrips = await tripService.GetTrips();
                var allUsers = await userService.GetUsers();
                var allFellows = await fellowtravelerService.GetFellowTravelers();

                // Мои поездки
                var historyTrips = new List<TripModel>();

                foreach (var trip in allTrips.Where(t => t.UserId.Equals(myId, StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    // Берем только данные пользователя
                    var user = allUsers.FirstOrDefault(u => u.Id?.Equals(trip.UserId, StringComparison.OrdinalIgnoreCase) == true);

                    // Данные пользователя 
                    trip.UserName = user?.Name ?? "Неизвестный";
                    trip.UserAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                    trip.UserRating = user?.Rating ?? 0.00;

                    // Места 
                    trip.DeparturePlaceName = trip.DeparturePlaceId ?? "Ошибка загрузки";
                    trip.ArrivePlaceName = trip.ArrivePlaceId ?? "Ошибка загрузки";

                    trip.Role = trip.IsDriver ? "Водитель" : "Пассажир";

                    historyTrips.Add(trip);
                }

                // Все поездки, в которых у пользователя есть заявка
                var myFellowTrips = new List<TripModel>();

                // Берем ВСЕ поездки статусов "1","2","3"
                var relevantTrips = allTrips.Where(t => t.StatusId == "1" || t.StatusId == "2" || t.StatusId == "3").ToList();

                foreach (var trip in relevantTrips)
                {
                    // Проверяем, была ли у меня заявка (любого статуса)
                    var myBooking = allFellows.FirstOrDefault(f => f.FellowUserId.Equals(myId, StringComparison.OrdinalIgnoreCase) && f.TripId == trip.Id);

                    if (myBooking == null) continue; // Нет моей брони -> пропускаем
                    // Это моя собственная поездка -> пропускаем (дубль)
                    if (trip.UserId.Equals(myId, StringComparison.OrdinalIgnoreCase)) continue;

                    // Берем только данные пользователя (ВОДИТЕЛЯ этой поездки)
                    var user = allUsers.FirstOrDefault(u => u.Id?.Equals(trip.UserId, StringComparison.OrdinalIgnoreCase) == true);

                    // Данные пользователя 
                    trip.UserName = user?.Name ?? "Неизвестный";
                    trip.UserAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                    trip.UserRating = user?.Rating ?? 0.00;

                    // Места 
                    trip.DeparturePlaceName = trip.DeparturePlaceId ?? "Ошибка загрузки"; // !!!!
                    trip.ArrivePlaceName = trip.ArrivePlaceId ?? "Ошибка загрузки";

                    trip.Role = trip.IsDriver ? "Водитель" : "Пассажир";


                    // Определние статуса запроса
                    SetPassengerStatusMessage(trip, myBooking);

                    myFellowTrips.Add(trip);
                }

                historyTrips.AddRange(myFellowTrips);

                // Обновление списка
                UpdateHistoryTrips(historyTrips);

                AtLeastOneTrip = AllHistoryTrips.Any();
                IsTripsEmpty = !AtLeastOneTrip;

                if (IsFirstSort == true)
                {
                    AllHistoryTrips = new ObservableCollection<TripModel>(SortHistoryTrips(AllHistoryTrips.ToList()));
                    IsFirstSort = false;
                }

                // Каждые 10 сек сортирует список по дате
                if (SortCounter >= 5)
                {
                    var sorted = SortHistoryTrips(AllHistoryTrips.ToList());
                    AllHistoryTrips.Clear();
                    foreach (var t in sorted) AllHistoryTrips.Add(t);
                }

                StartReloadTimer();  // Запуск таймеров
            }
            catch (Exception ex)
            {
                await Shell.Current.GoToAsync("//SearchPage");
                await Shell.Current.DisplayAlertAsync("Внимание", "Время ожидания истекло или возникли неполадки", "OK");
            }
        }

        private void StartReloadTimer()
        {
            _reloadTimer?.Dispose();

            if (SortCounter == 5) SortCounter = 0;

            _reloadTimer = new System.Threading.Timer(async _ =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    SortCounter++;

                    // Перезагружаем каждые 2 секунды
                    await LoadTripCommand.ExecuteAsync(null);
                });
            }, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
        }

        private void StopReloadTimer()
        {
            _reloadTimer?.Dispose();
            _reloadTimer = null;
        }

        private void UpdateHistoryTrips(List<TripModel> newTrips)
        {
            foreach (var newTrip in newTrips)
            {
                var existing = AllHistoryTrips.FirstOrDefault(t => t.Id == newTrip.Id);
                if (existing != null)
                {
                    existing.StatusId = newTrip.StatusId;
                    existing.SeatsQuentity = newTrip.SeatsQuentity;
                    existing.UserName = newTrip.UserName;
                    existing.UserAvatar = newTrip.UserAvatar;
                    existing.UserRating = newTrip.UserRating;
                    if (!string.IsNullOrEmpty(newTrip.StatusMessage))
                        existing.StatusMessage = newTrip.StatusMessage;
                    existing.UserStatus = newTrip.UserStatus;
                    existing.DeparturePlaceName = newTrip.DeparturePlaceName;
                    existing.ArrivePlaceName = newTrip.ArrivePlaceName;
                    existing.Role = newTrip.Role;
                }
                else
                {
                    // Добавляем новые
                    AllHistoryTrips.Add(newTrip);;
                }

                var currentIds = newTrips.Select(nt => nt.Id).ToHashSet();  // HashSet для скорости
                var toRemove = AllHistoryTrips.Where(t => !currentIds.Contains(t.Id)).ToList();

                foreach (var trip in toRemove)
                    AllHistoryTrips.Remove(trip);
            }
        }

        // сортировка по времени поездок
        private List<TripModel> SortHistoryTrips(List<TripModel> trips)
        {
            var now = DateTime.Now;
            return trips
                .OrderBy(trip => trip.StatusId == "2" ? 0 : (trip.StatusId != "1" ? 1 : 2))
                .ThenBy(trip =>
                {
                    if (string.IsNullOrEmpty(trip.Date) || string.IsNullOrEmpty(trip.Time)) return long.MaxValue;
                    string dateTimeStr = $"{trip.Date} {trip.Time}";
                    return DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDate)
                        ? Math.Abs((tripDate - now).Ticks)
                        : long.MaxValue;
                })
                .ToList();

        }

        // Статус текста для определения
        private void SetPassengerStatusMessage(TripModel trip, FellowTravelerModel myBooking)
        {
            bool myRequestAccepted = myBooking?.StatusId == "6"; // "6" = "Едет"
            bool myRequestWaiting = myBooking?.StatusId == "5";  // "5" = Ожидает
            bool hasSeats = trip.SeatsLabelVisible; // true = места есть

            if (trip.StatusId == "3") // Запланирована
            {
                if (myRequestAccepted)
                {
                    trip.StatusMessage = "Ваш запрос одобрен. Ожидайте отправления(см.поездку выше)";
                }
                else if (myRequestWaiting)
                {
                    trip.StatusMessage = "Ваш запрос на рассмотрении. Ожидайте решение попутчика(см.поездку выше)";
                }
                else if (!hasSeats)
                {
                    trip.StatusMessage = "Места закончились, но ваш запрос всё ещё на рассмотрении(см.поездку выше)";
                }
                else
                {
                    trip.StatusMessage = ""; // fallback
                }
            }
            else if (trip.StatusId == "2" && myRequestAccepted)
            {
                trip.StatusMessage = "Вы в пути! Приятной поездки!(см.поездку выше)";
            }
            else if (trip.StatusId == "1" && myRequestAccepted)
            {
                trip.StatusMessage = "Вы приехали!(см.поездку выше)";
            }
            else
            {
                trip.StatusMessage = "";
            }

            // Для совместимости
            trip.UserStatus = myRequestAccepted;
        }

        // К деталям поездки
        [RelayCommand]
        private async Task GoTripDetails(TripModel trip)
        {
            StopReloadTimer();
            Preferences.Set("SelectedTripId", trip.Id);
            Preferences.Set("PreviousPage", "TravelHistoryPage");
            await Shell.Current.GoToAsync("//TripDetailsPage");
        }

        [RelayCommand]
        public async Task OnMainPage()
        {
            StopReloadTimer();
            await Shell.Current.GoToAsync("//SearchPage");
        }
    }
}
