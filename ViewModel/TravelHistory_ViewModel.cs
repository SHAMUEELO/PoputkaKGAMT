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
        private readonly IBackgroundUpdateService backgroundUpdateService;
        private readonly TripService tripService;
        private readonly UserService userService;
        private readonly PlaceService placeService;

        public TravelHistory_ViewModel(IBackgroundUpdateService backgroundUpdateService, TripService tripService, UserService userService, PlaceService placeService, FellowTravelerService fellowtravelerService)
        {
            this.backgroundUpdateService = backgroundUpdateService;
            this.tripService = tripService;
            this.userService = userService;
            this.placeService = placeService;
            this.fellowtravelerService = fellowtravelerService;

            backgroundUpdateService.OnTripsUpdated += () =>
            {
                if (MainThread.IsMainThread)
                    LoadTrip();
                else
                    MainThread.BeginInvokeOnMainThread(LoadTrip);
            };

        }


        [ObservableProperty]
        private ObservableCollection<TripModel> allHistoryTrips = new();

        [ObservableProperty]
        private bool atLeastOneTrip = false;

        [ObservableProperty]
        private bool isTripsEmpty = true;

        [ObservableProperty]
        private string driverNotCheckAndHaveMaxPassenger = "Ваша заявка на рассмотрении(см.поездку выше)";

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
        public async void LoadTrip()
        {
            try
            {
                string myId = Preferences.Get("CurrentUserKey", "");

                var allTrips = await tripService.GetTrips();
                var allUsers = await userService.GetUsers();
                var allPlaces = await placeService.GetPlaces();
                var allFellows = await fellowtravelerService.GetFellowTravelers();

                // Мои поездки
                var historyTrips = new List<TripModel>();
                
                foreach (var trip in allTrips)
                {
                    // Только мои поездки
                    if (!trip.UserId.Equals(myId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // если StatusId == "2" или "1" и попутчиков нет, то удаляе  поездку из БД
                    if ((trip.StatusId == "2" || trip.StatusId == "1") && trip.SeatsQuentity == trip.OriginalSeatsQuentity)
                    {
                        // Используем логику удаления с очисткой попутчиков
                        await firebase.Child("trips").Child(trip.Id).DeleteAsync();

                        // Также нужно удалить связанные с поездкой попутчиков
                        foreach (var ft in allFellows.Where(f => f.TripId == trip.Id))
                        {
                            await firebase.Child("fellow_travelers").Child(ft.Id).DeleteAsync();
                        }
                    }

                    // Берем только данные пользователя
                    var user = allUsers.FirstOrDefault(u => u.Id?.Equals(trip.UserId, StringComparison.OrdinalIgnoreCase) == true);

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

                    historyTrips.Add(trip);
                }

               
                // Все поездки, в которых у пользователя есть заявка 
                var allMyBookings = allFellows?.Where(f => f.FellowUserId.Equals(myId, StringComparison.OrdinalIgnoreCase)).ToList() ?? new List<FellowTravelerModel>();

                var myFellowTripIds = allMyBookings.Select(f => f.TripId).Distinct().ToList();

                foreach (var fellowTripId in myFellowTripIds)
                {
                    var fellowTrip = allTrips?.FirstOrDefault(t => t.Id == fellowTripId);
                    if (fellowTrip == null) continue;

                    // Только проверяем что это НЕ моя поездка (дубль)
                    if (fellowTrip.UserId.Equals(myId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Берем только данные пользователя (ВОДИТЕЛЯ этой поездки)
                    var user = allUsers.FirstOrDefault(u => u.Id?.Equals(fellowTrip.UserId, StringComparison.OrdinalIgnoreCase) == true);

                    // Данные пользователя 
                    fellowTrip.UserName = user?.Name ?? "Неизвестный";
                    fellowTrip.UserAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                    fellowTrip.UserRating = user?.Rating ?? 0.00;

                    // Места 
                    var departurePlace = allPlaces.FirstOrDefault(p => p.Id?.Equals(fellowTrip.DeparturePlaceId, StringComparison.OrdinalIgnoreCase) == true);
                    var arrivePlace = allPlaces.FirstOrDefault(p => p.Id?.Equals(fellowTrip.ArrivePlaceId, StringComparison.OrdinalIgnoreCase) == true);
                    fellowTrip.DeparturePlaceName = departurePlace?.Name ?? "Неизвестно";
                    fellowTrip.ArrivePlaceName = arrivePlace?.Name ?? "Неизвестно";

                    fellowTrip.Role = fellowTrip.IsDriver ? "Водитель" : "Пассажир";

                    // Другие поездки с моими заявками, которые еще не рассмотрели
                    var myPendingBooking = allFellows.FirstOrDefault(f =>f.FellowUserId.Equals(myId, StringComparison.OrdinalIgnoreCase) && f.TripId.Equals(fellowTrip.Id, StringComparison.OrdinalIgnoreCase) && f.StatusId == "5");
                    fellowTrip.UserStatus = myPendingBooking != null;


                    if (fellowTrip.SeatsLabelVisible == false)
                    {
                        DriverNotCheckAndHaveMaxPassenger = "Все места заняты, ваша заявка еще не рассмотрена(см.поездку выше)";
                    }
                    else { DriverNotCheckAndHaveMaxPassenger = "Ваша заявка на рассмотрении(см.поездку выше)";  }


                    if (!historyTrips.Any(t => t.Id == fellowTrip.Id))  // Без дублей
                    {
                        historyTrips.Add(fellowTrip);
                    }
                }

                // Сортровка поездок по реальной дате
                var now = DateTime.Now;
                AllHistoryTrips = new ObservableCollection<TripModel>(
                    historyTrips
                        .OrderBy(trip => {
                            // Сначала статус "2"
                            if (trip.StatusId == "2") return 0;
                            // Потом всё, что не "1" и не "2"
                            if (trip.StatusId != "1") return 1;
                            // Последними — статус "1"
                            return 2;
                        })
                        .ThenBy(trip =>
                        {
                            if (string.IsNullOrEmpty(trip.Date) || string.IsNullOrEmpty(trip.Time))
                                return long.MaxValue;  // Пустые даты - в конец

                            string dateTimeStr = $"{trip.Date} {trip.Time}";
                            if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDate))
                                return Math.Abs((tripDate - now).Ticks);

                            return long.MaxValue;  // Если не распарсилось - в конец
                        })
                        .ToList()
                );

                AtLeastOneTrip = AllHistoryTrips.Any();
                IsTripsEmpty = !AtLeastOneTrip;
                OnPropertyChanged(nameof(AllHistoryTrips));

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить данные!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
            }
        }


        // К деталям поездки
        [RelayCommand]
        private async Task GoTripDetails(TripModel trip)
        {
            Preferences.Set("SelectedTripId", trip.Id);
            Preferences.Set("PreviousPage", "TravelHistoryPage");
            await Shell.Current.GoToAsync("//TripDetailsPage");
        }

        [RelayCommand]
        public async Task OnMainPage()
        {
            await Shell.Current.GoToAsync("//SearchPage");
        }
    }
}
