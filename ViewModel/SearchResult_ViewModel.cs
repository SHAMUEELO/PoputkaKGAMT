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

    public partial class SearchResult_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        private readonly TripService tripService;
        private readonly UserService userService;
        private readonly FellowTravelerService fellowTravelerService;

        public SearchResult_ViewModel()
        {
            fellowTravelerService = new FellowTravelerService();
            tripService = new TripService();
            userService = new UserService();
        }

        [ObservableProperty]
        private ObservableCollection<TripModel> driverAllTrips = new();  // Список поездок только водителей 
        [ObservableProperty]
        ObservableCollection<TripModel> searchTrips = new();

        [ObservableProperty]
        private ObservableCollection<TripModel> passengerAllTrips = new();  // Список поездок только пассажиры 

        [ObservableProperty] 
        private bool showSearchResults;

        [ObservableProperty] 
        private bool isSearchEmpty;   
        
        [ObservableProperty] 
        private bool isAllEmptyDriver;

        [ObservableProperty] 
        private bool isAllEmptyPassenger;

        [ObservableProperty] 
        private bool isFirstSort = true;

        [ObservableProperty] 
        private int sortCounter = 0;  // Счетчик

        // Таймер для обновления списка
        private System.Threading.Timer? _reloadTimer;

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                // Изменяем статус, если реальное время соответсвует времени поездки
                var allFellowTravelersCheck = await fellowTravelerService.GetFellowTravelers();
                var allTripsCheck = await tripService.GetTrips();
                var now = DateTime.Now;  //  Текущее время

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

                            // Скрываем с поиска
                            await firebase.Child("trips").Child(trip.Id).PatchAsync(new { is_visible_on_searchresult_page = false });

                            if (trip.SeatsQuentity == trip.OriginalSeatsQuentity)
                            {
                                // Удаляем поездку
                                await firebase.Child("trips").Child(trip.Id).DeleteAsync();
                            }
                            else await firebase.Child("trips").Child(trip.Id).PatchAsync(new { status_id = "2" });

                        }
                    }
                }

                var allTrips = await tripService.GetTrips();
                var allUsers = await userService.GetUsers();
                var allFellowTravelers = await fellowTravelerService.GetFellowTravelers();

                var allDrivers = new List<TripModel>();
                var allPassengers = new List<TripModel>();
                var searchResults = new List<TripModel>();


                // Удаляем завершенные поездки без попутчиков
                foreach (var trip in allTrips.ToList())
                {
                    // если StatusId == "2" или "1" и попутчиков нет, то удаляе  поещдку из БД
                    if ((trip.StatusId == "2" || trip.StatusId == "1") && trip.SeatsQuentity == trip.OriginalSeatsQuentity)
                    {
                        // Используем логику удаления с очисткой попутчиков
                        await firebase.Child("trips").Child(trip.Id).DeleteAsync();

                        // Также нужно удалить связанные с поездкой попутчиков
                        foreach (var ft in allFellowTravelers.Where(f => f.TripId == trip.Id))
                        {
                            await firebase.Child("fellow_travelers").Child(ft.Id).DeleteAsync();
                        }
                        continue;
                    }
                    // если StatusId == "2" или "1", то не показываем в SearchResult
                    if (trip.StatusId == "2" || trip.StatusId == "1")
                        continue;


                    var user = allUsers.FirstOrDefault(u => u.Id?.Equals(trip.UserId, StringComparison.OrdinalIgnoreCase) == true);

                    // Проверяем, есть ли пользователь
                    if (user == null) continue;

                    // Данные пользователя
                    trip.UserName = user?.Name ?? "Неизвестный";
                    trip.UserAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                    trip.UserRating = user?.Rating ?? 0.00;

                    // Места 
                    trip.DeparturePlaceName = trip.DeparturePlaceId ?? "Ошибка загрузки";
                    trip.ArrivePlaceName = trip.ArrivePlaceId ?? "Ошибка загрузки";

                    trip.Role = trip.IsDriver ? "Водитель" : "Пассажир";
                    if (trip.IsDriver)
                    {
                        allDrivers.Add(trip);
                        if (MatchesSearchCriteria(trip)) searchResults.Add(trip);
                    }
                    else
                    {
                        allPassengers.Add(trip);
                        if (MatchesSearchCriteria(trip)) searchResults.Add(trip);
                    }
                }

                // Обновление спсика при изменении
                UpdateTripsCollection(DriverAllTrips, allDrivers);
                UpdateTripsCollection(PassengerAllTrips, allPassengers);
                UpdateTripsCollection(SearchTrips, searchResults);

                if(IsFirstSort == true)
                {
                    DriverAllTrips = new ObservableCollection<TripModel>(SortTripsByDate(DriverAllTrips.ToList()));
                    PassengerAllTrips = new ObservableCollection<TripModel>(SortTripsByDate(PassengerAllTrips.ToList()));
                    SearchTrips = new ObservableCollection<TripModel>(SortTripsByDate(SearchTrips.ToList()));
                    IsFirstSort = false;
                }

                // Каждые 10 сек сортирует список по дате
                if (SortCounter >= 5)
                {
                    DriverAllTrips = new ObservableCollection<TripModel>(SortTripsByDate(DriverAllTrips.ToList()));
                    PassengerAllTrips = new ObservableCollection<TripModel>(SortTripsByDate(PassengerAllTrips.ToList()));
                    SearchTrips = new ObservableCollection<TripModel>(SortTripsByDate(SearchTrips.ToList()));
                }
                    

                if (App.Parameters)
                {
                    ShowSearchResults = true;
                    IsSearchEmpty = !searchResults.Any();
                }
                else
                {
                    SearchTrips.Clear();
                    ShowSearchResults = false;
                }
                IsAllEmptyDriver = !allDrivers.Any();
                IsAllEmptyPassenger = !allPassengers.Any();

                // Запуск таймера
                StartReloadTimer();
            }
            catch(Exception ex) {
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
                    await LoadDataCommand.ExecuteAsync(null);
                });
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));

        }

        private void StopReloadTimer()
        {
            _reloadTimer?.Dispose();
            _reloadTimer = null;
        }


        // Обновление спсиков через сарвнение
        private void UpdateTripsCollection(ObservableCollection<TripModel> collection, List<TripModel> newTrips)
        {
            // 1. удаляем отсутствующие (удалена поездка из БД)
            var toRemove = collection.Where(existing => !newTrips.Any(newTrip => newTrip.Id == existing.Id)).ToList();
            foreach (var trip in toRemove)
                collection.Remove(trip);

            // 2. обновляем существующие (статус/места изменились)
            foreach (var newTrip in newTrips)
            {
                var existing = collection.FirstOrDefault(t => t.Id == newTrip.Id);
                if (existing != null)
                {
                    // Обновляем ключевые свойства
                    existing.StatusId = newTrip.StatusId;
                    existing.SeatsQuentity = newTrip.SeatsQuentity;
                    existing.UserName = newTrip.UserName;
                    existing.UserAvatar = newTrip.UserAvatar;
                    existing.UserRating = newTrip.UserRating;
                }
            }

            // 3. Добавляем новые (добавилась поездка)
            var toAdd = newTrips.Where(newTrip => !collection.Any(t => t.Id == newTrip.Id)).ToList();
            foreach (var trip in toAdd)
                collection.Add(trip);
        }

        // Сортировка по вермени поездок
        private List<TripModel> SortTripsByDate(List<TripModel> trips)
        {
            var now = DateTime.Now;
            return trips.OrderBy(trip =>
            {
                if (string.IsNullOrEmpty(trip.Date) || string.IsNullOrEmpty(trip.Time))
                    return long.MaxValue;

                string dateTimeStr = $"{trip.Date} {trip.Time}";
                if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDate))
                    return Math.Abs((tripDate - now).Ticks);

                return long.MaxValue;
            }).ToList();
        }
        

        private bool MatchesSearchCriteria(TripModel trip)
        {
            if (!App.Parameters) return true; 

            // 1. Место отъезда
            var departurePlace = Preferences.Get("SearchDeparturePlace", "");
            if (!string.IsNullOrEmpty(departurePlace) && trip.DeparturePlaceName != departurePlace)
                return false;

            // 2. Место прибытия
            var arrivePlace = Preferences.Get("SearchArrivePlace", "");
            if (!string.IsNullOrEmpty(arrivePlace) && trip.ArrivePlaceName != arrivePlace)
                return false;

            // 3. Дата 
            var searchDateTicks = Preferences.Get("SearchDate", 0L);
            if (searchDateTicks > 0 && DateTime.TryParse(trip.Date, out var tripDate))
            {
                var searchDate = new DateTime(searchDateTicks).Date;
                if (tripDate.Date != searchDate)
                    return false;
            }

            // 4. Места (доступно больше чем ищут)
            var passengerCount = Preferences.Get("SearchPassengerCount", 1);
            if (trip.SeatsQuentity < passengerCount)
                return false;

            return true; 
        }

        // К деталям поездки
        [RelayCommand]
        private async Task GoTripDetails(TripModel trip)
        {
            StopReloadTimer();
            Preferences.Set("SelectedTripId", trip.Id);
            Preferences.Set("PreviousPage", "SearchResultPage");
            await Shell.Current.GoToAsync("//TripDetailsPage");
        }

        // Назад
        [RelayCommand]
        public async Task GoSearch()
        {
            StopReloadTimer();
            await Shell.Current.GoToAsync("//SearchPage");
        }


    }
}
