using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Collections.ObjectModel;
using System.Globalization;
namespace PoputkaKGAMT.ViewModel
{

    public partial class SearchResult_ViewModel : ObservableObject
    {
        private readonly IBackgroundUpdateService backgroundUpdateService;
        private readonly TripService tripService;
        private readonly UserService userService;
        private readonly PlaceService placeService;

        public SearchResult_ViewModel(IBackgroundUpdateService backgroundUpdateService,TripService tripService,UserService userService, PlaceService placeService)
        {
            this.backgroundUpdateService = backgroundUpdateService;
            this.tripService = tripService;
            this.userService = userService;
            this.placeService = placeService;

            // 
            backgroundUpdateService.OnTripsUpdated += () =>
            {
                if (MainThread.IsMainThread)
                    LoadData();
                else
                    MainThread.BeginInvokeOnMainThread(LoadData);
            };

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

        // Постоянное обновление актуального списка
        public void OnNavigatedTo()
        {
            backgroundUpdateService.Start(10); 
        }

        public void OnNavigatedFrom()
        {
            backgroundUpdateService.Stop();
        }

        [RelayCommand]
        public async void LoadData()
        {
            
            var allTrips = await tripService.GetTrips();
            var allUsers = await userService.GetUsers();
            var allPlaces = await placeService.GetPlaces();


            var allDrivers = new List<TripModel>();
            var allPassengers = new List<TripModel>();
            var searchResults = new List<TripModel>();

            try 
            {
                foreach (var trip in allTrips)
                {
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
                    var departurePlace = allPlaces.FirstOrDefault(p => p.Id == trip.DeparturePlaceId);
                    var arrivePlace = allPlaces.FirstOrDefault(p => p.Id == trip.ArrivePlaceId);
                    trip.DeparturePlaceName = departurePlace?.Name ?? "Неизвестно";
                    trip.ArrivePlaceName = arrivePlace?.Name ?? "Неизвестно";

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

                // Сортровка поездок по реальной дате у DriverAllTrips
                var nowForDriverAllTrips = DateTime.Now;
                DriverAllTrips = new ObservableCollection<TripModel>(
                    allDrivers
                        .OrderBy(trip => {
                            if (string.IsNullOrEmpty(trip.Date) || string.IsNullOrEmpty(trip.Time))
                                return long.MaxValue;  // Пустые даты - в конец

                            string dateTimeStr = $"{trip.Date} {trip.Time}";
                            if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDate))
                                return Math.Abs((tripDate - nowForDriverAllTrips).Ticks);

                            return long.MaxValue;  // Если не распарсилось - в конец
                        })
                        .ToList()
                );

                // Сортровка поездок по реальной дате у PassengerAllTrips
                var nowForPassengerAllTrips = DateTime.Now;
                PassengerAllTrips = new ObservableCollection<TripModel>(
                    allPassengers
                        .OrderBy(trip => {
                            if (string.IsNullOrEmpty(trip.Date) || string.IsNullOrEmpty(trip.Time))
                                return long.MaxValue;  // Пустые даты - в конец

                            string dateTimeStr = $"{trip.Date} {trip.Time}";
                            if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDate))
                                return Math.Abs((tripDate - nowForPassengerAllTrips).Ticks);

                            return long.MaxValue;  // Если не распарсилось - в конец
                        })
                        .ToList()
                );


                if (App.Parameters)
                {
                    // Сортровка поездок по реальной дате у SearchTrips(Список поездок соответсвуюущие поиску)
                    var nowForSearchTrips = DateTime.Now;
                    SearchTrips = new ObservableCollection<TripModel>(
                        searchResults
                            .OrderBy(trip => {
                                if (string.IsNullOrEmpty(trip.Date) || string.IsNullOrEmpty(trip.Time))
                                    return long.MaxValue;  // Пустые даты - в конец

                                string dateTimeStr = $"{trip.Date} {trip.Time}";
                                if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDate))
                                    return Math.Abs((tripDate - nowForSearchTrips).Ticks);

                                return long.MaxValue;  // Если не распарсилось - в конец
                            })
                            .ToList()
                    );

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
            }
            catch(Exception ex) { await Shell.Current.DisplayAlertAsync("Ошибка", "Не загрузить данные\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK"); }
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
            Preferences.Set("SelectedTripId", trip.Id);
            Preferences.Set("PreviousPage", "SearchResultPage");
            await Shell.Current.GoToAsync("//TripDetailsPage");
            
        }

        // Назад
        [RelayCommand]
        public async Task GoSearch()
        {
            await Shell.Current.GoToAsync("//SearchPage");

        }


    }
}
