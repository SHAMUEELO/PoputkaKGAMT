using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Collections.ObjectModel;


namespace PoputkaKGAMT.ViewModel
{
    partial class FellowTraveler_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");
        private readonly UserService userService;
        private readonly FellowTravelerService fellowTravelerService;
        public FellowTraveler_ViewModel()
        {
            userService = new UserService();
            fellowTravelerService = new FellowTravelerService();
        }

        [ObservableProperty]
        private ObservableCollection<FellowTravelerModel> fellowUsers = new();

        [ObservableProperty]
        private bool isMyBooking;

        [ObservableProperty]
        private bool isTripOwner;

        [ObservableProperty]
        private bool atLeastOneFellowUser = false;

        [ObservableProperty]
        private bool isFellowTravelerEmpty = true; // Показывает данные при отсутствии брони выбранной поездки

        [ObservableProperty]
        private bool canAcceptPassenger = true;

        [ObservableProperty]
        private Color acceptButtonColor = Color.FromArgb("#21842C");

        [RelayCommand]
        public async Task LoadFellowTravelers()
        {
            FellowUsers.Clear();
            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                string currentUserId = Preferences.Get("CurrentUserKey", "");

                var data = await Task.Run(async () =>
                {
                    var allUsers = await userService.GetUsers();
                    var allFellowTravelers = await fellowTravelerService.GetFellowTravelers();
                    var allTrips = await new TripService().GetTrips();

                    var trip = allTrips.FirstOrDefault(t => t.Id == tripId);
                    if (trip?.StatusId == "2" || trip?.StatusId == "1")
                    {
                        allFellowTravelers = allFellowTravelers.Where(f => f.TripId != tripId || f.StatusId != "5").ToList();
                    }

                    var tripFellowTravelers = allFellowTravelers.Where(f => f.TripId == tripId).ToList();
                    var currentTrip = allTrips.FirstOrDefault(t => t.Id == tripId);

                    // Подготовка списка
                    var preparedList = new List<FellowTravelerModel>();
                    foreach (var fellowTraveler in tripFellowTravelers)
                    {
                        var user = allUsers.FirstOrDefault(u => u.Id?.Equals(fellowTraveler.FellowUserId, StringComparison.OrdinalIgnoreCase) == true);
                        fellowTraveler.UserFellowName = user?.Name ?? "Неизвестный";
                        fellowTraveler.UserFellowAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                        fellowTraveler.UserFellowRating = user?.Rating ?? 0.00;
                        // Определение цвета кнопок
                        fellowTraveler.IsCurrentUser = fellowTraveler.FellowUserId == currentUserId;
                        fellowTraveler.MyFellowBackgroundColor = fellowTraveler.IsCurrentUser ? Color.FromArgb("#EFF4FF") : Colors.White;
                        preparedList.Add(fellowTraveler);
                    }

                    return (preparedList, currentTrip, tripFellowTravelers.Any(f => f.FellowUserId == currentUserId));
                });

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FellowUsers.Clear();
                    foreach (var item in data.preparedList)
                        FellowUsers.Add(item);

                    AtLeastOneFellowUser = data.preparedList.Any();
                    IsFellowTravelerEmpty = !AtLeastOneFellowUser;
                    IsMyBooking = data.Item3;
                    IsTripOwner = data.currentTrip?.UserId == currentUserId;
                    CanAcceptPassenger = data.currentTrip?.SeatsQuentity > 0;
                    AcceptButtonColor = CanAcceptPassenger ? Color.FromArgb("#21842C") : Color.FromArgb("#808080");
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить данные!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
            }
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

        // Удаляем бронь
        [RelayCommand]
        private async Task DeleteBooking(FellowTravelerModel selectedPassenger)
        {
            bool delete = await Shell.Current.DisplayAlertAsync("Внимание", "Вы точно хотите отменить бронирование?", "Да", "Нет");
            if (!delete) return;

            try
            {
               
                string tripId = Preferences.Get("SelectedTripId", "");
                
                var tripService = new TripService();
                var allTrips = await tripService.GetTrips();  // Все поездки
                var trip = allTrips.FirstOrDefault(t => t.Id == tripId);

                // Находим поездку и меняем необходимое количество мест
                if (selectedPassenger.StatusId == "6")
                {
                    if (trip == null)
                    {
                        await Shell.Current.DisplayAlertAsync("ОШИБКА", "Поездка не найдена в БД!", "OK");
                        return;
                    }

                    if (selectedPassenger.FellowUserIsDriver == false) // Если пассажир
                    {
                        if (trip.SeatsQuentity != 4)
                        {
                            trip.SeatsQuentity += 1;
                        }

                        await firebase.Child("trips").Child(tripId).PatchAsync(new { seats_quentity = trip.SeatsQuentity });
                    }
                    
                    if(selectedPassenger.FellowUserIsDriver == true)
                    {
                        await firebase.Child("trips").Child(tripId).PatchAsync(new { seats_quentity = trip.OriginalSeatsQuentity });
                    }
                }

                await firebase.Child("fellow_travelers").Child(selectedPassenger.Id).DeleteAsync();
                FellowUsers.Remove(selectedPassenger);

                IsMyBooking = false;
                LoadFellowTravelers();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось отменить бронь!\nВозможно проблемы с интернетом", "Ок"); }
        }

        // Добавляем в итоговую поездку
        [RelayCommand]
        private async Task AddInFinalTrip(FellowTravelerModel selectedPassenger)
        {
            try
            {
                await firebase.Child("fellow_travelers").Child(selectedPassenger.Id).PatchAsync(new { status_id = "6" });

                string tripId = Preferences.Get("SelectedTripId", "");

                // Находим поездку и меняем необходимое количество мест
                var tripService = new TripService();
                var trip = (await tripService.GetTrips()).FirstOrDefault(t => t.Id == tripId);

                if (!selectedPassenger.FellowUserIsDriver) // Если ПАССАЖИР
                {
                    if (trip.SeatsQuentity > 0)
                    {
                        trip.SeatsQuentity -= 1;

                        await firebase.Child("trips").Child(tripId).PatchAsync(new { seats_quentity = trip.SeatsQuentity });
                    }
                }
                else
                {
                    trip.SeatsQuentity = 0; // Все заняты

                    await firebase.Child("trips").Child(tripId).PatchAsync(new {seats_quentity = trip.SeatsQuentity});
                }

                    // Удаляем бронь пассажира из других поездок
                    string passengerId = selectedPassenger.FellowUserId;
                    var allFellowTravelers = await fellowTravelerService.GetFellowTravelers();

                foreach (var otherBooking in allFellowTravelers)
                {
                    // Если другая бронь И НЕ текущая поездка И статус НЕ "6"
                    if (otherBooking.FellowUserId == passengerId && otherBooking.TripId != tripId)
                    {
                        await firebase.Child("fellow_travelers").Child(otherBooking.Id).DeleteAsync();
                    }
                }

                LoadFellowTravelers();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось принять пользователя!\nВозможно проблемы с интернетом", "Ок"); }
        }



        [RelayCommand]
        public async Task GoBackTripsDetails()
        {
            try
            {
                IsMyBooking = false;
                IsTripOwner = false;
                await Shell.Current.GoToAsync("//TripDetailsPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось вернуться\nВозможно проблемы с интернетом\nОшибка:" + ex.Message, "Ок");
            }
        }
   

        [RelayCommand]
        private async Task GoBackSearchResult()
        {
            Preferences.Remove("SelectedTripId");
            await Shell.Current.GoToAsync("//SearchResultPage");
        }
    }
}
