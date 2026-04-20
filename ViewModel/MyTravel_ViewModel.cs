using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PoputkaKGAMT.ViewModel
{
    partial class MyTravel_ViewModel : ObservableObject
    {
        private readonly TripService tripService;
        private readonly UserService userService;
        private readonly PlaceService placeService;

        public MyTravel_ViewModel()
        {
            tripService = new TripService();
            userService = new UserService();
            placeService = new PlaceService();
            LoadMyTripData();
        }

        [ObservableProperty]
        private ObservableCollection<TripModel> myTrips = new();

        [ObservableProperty]
        private bool atLeastOneTrip = false; 

        [ObservableProperty]
        private bool isTripsEmpty = true; 

        [RelayCommand]
        public async void LoadMyTripData()
        {
            try
            {
                var allTrips = await tripService.GetTrips();
                var allUsers = await userService.GetUsers();
                var allPlaces = await placeService.GetPlaces();

                var myTripsOnly = new List<TripModel>();
            
                string myUserId = Preferences.Get("CurrentUserKey", "");

                foreach (var trip in allTrips)
                {
                    // Только мои поездки
                    if (!trip.UserId.Equals(myUserId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Берем только данные пользователя
                    var user = allUsers.FirstOrDefault(u => u.Id?.Equals(trip.UserId, StringComparison.OrdinalIgnoreCase) == true);

                    // Данные пользователя УБРАТЬ???
                    trip.UserName = user?.Name ?? "Неизвестный";
                    trip.UserAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                    trip.UserRating = user?.Rating ?? 0.00;

                    // Места 
                    var departurePlace = allPlaces.FirstOrDefault(p => p.Id == trip.DeparturePlaceId);
                    var arrivePlace = allPlaces.FirstOrDefault(p => p.Id == trip.ArrivePlaceId);
                    trip.DeparturePlaceName = departurePlace?.Name ?? "Неизвестно";
                    trip.ArrivePlaceName = arrivePlace?.Name ?? "Неизвестно";

                    trip.Role = trip.IsDriver ? "Водитель" : "Пассажир";

                    myTripsOnly.Add(trip);
                }

                
                // Сортровка поездок по реальной дате
                var now = DateTime.Now;
                MyTrips = new ObservableCollection<TripModel>(
                    myTripsOnly
                        .OrderBy(trip => {
                            if (string.IsNullOrEmpty(trip.Date) || string.IsNullOrEmpty(trip.Time))
                                return long.MaxValue;  // Пустые даты - в конец

                            string dateTimeStr = $"{trip.Date} {trip.Time}";
                            if (DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDate))
                                return Math.Abs((tripDate - now).Ticks);

                            return long.MaxValue;  // Если не распарсилось - в конец
                        })
                        .ToList()
                );

                IsTripsEmpty = !MyTrips.Any();
                AtLeastOneTrip = !IsTripsEmpty;

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
            Preferences.Set("PreviousPage", "MyTravelPage");
            await Shell.Current.GoToAsync("//TripDetailsPage");
        }

        [RelayCommand]
        public async Task GoBackProfileButton()
        {

            await Shell.Current.GoToAsync("//ProfilePage");
        }
    }
}
