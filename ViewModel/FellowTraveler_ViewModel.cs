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
        private readonly TripService tripService;

        public FellowTraveler_ViewModel()
        {
            userService = new UserService();
            fellowTravelerService = new FellowTravelerService();
            tripService = new TripService();
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

        [ObservableProperty]
        private string tripStatusId = ""; // статус поездки

        // Таймер
        private System.Threading.Timer? _reloadTimer;

        [RelayCommand]
        public async Task LoadFellowTravelers()
        {
            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                string currentUserId = Preferences.Get("CurrentUserKey", "");

                var allUsers = await userService.GetUsers();
                var allFellowTravelers = await fellowTravelerService.GetFellowTravelers();
                var allTrips = await tripService.GetTrips();

                var trip = allTrips.FirstOrDefault(t => t.Id == tripId);
                TripStatusId = trip?.StatusId ?? "";

                if (trip?.StatusId == "2" || trip?.StatusId == "1")
                {
                    allFellowTravelers = allFellowTravelers.Where(f => f.TripId != tripId || f.StatusId != "5").ToList();
                }

                var tripFellowTravelers = allFellowTravelers.Where(f => f.TripId == tripId).ToList();
                var currentTrip = allTrips.FirstOrDefault(t => t.Id == tripId);

                // Подготовка списка
                TripStatusId = trip?.StatusId ?? "";

                // Удаляем из списка тех, кого нет в Firebase
                var toRemove = FellowUsers.Where(f => !tripFellowTravelers.Any(ft => ft.Id == f.Id)).ToList();
                foreach (var f in toRemove)
                    FellowUsers.Remove(f);

                // Обновляем уже есть
                var toUpdate = tripFellowTravelers.Where(ft => FellowUsers.Any(f => f.Id == ft.Id)).ToList();
                foreach (var ft in toUpdate)
                {
                    var existing = FellowUsers.FirstOrDefault(f => f.Id == ft.Id);
                    if (existing != null)
                    {
                        existing.StatusId = ft.StatusId;
                        existing.IsCurrentUserVisible = !IsTripOwner && existing.IsCurrentUser && TripStatusId == "3";
                        existing.IsCreatorVisible = IsTripOwner && TripStatusId == "3";
                    }
                }

                // Добавляем новые
                var toAdd = tripFellowTravelers.Where(ft => !FellowUsers.Any(f => f.Id == ft.Id)).ToList();
                foreach (var ft in toAdd)
                {
                    var user = allUsers.FirstOrDefault(u =>
                        u.Id?.Equals(ft.FellowUserId, StringComparison.OrdinalIgnoreCase) == true
                    );

                    ft.UserFellowName = user?.Name ?? "Неизвестный";
                    ft.UserFellowAvatar = user?.ProfilePhoto ?? "defoltavataricon.png";
                    ft.UserFellowRating = user?.Rating ?? 0.00;

                    ft.IsCurrentUser = ft.FellowUserId == currentUserId;
                    ft.MyFellowBackgroundColor = ft.IsCurrentUser ? Color.FromArgb("#EFF4FF") : Colors.White;

                    ft.IsCurrentUserVisible = !IsTripOwner && ft.IsCurrentUser && TripStatusId == "3";
                    ft.IsCreatorVisible = IsTripOwner && TripStatusId == "3";

                    FellowUsers.Add(ft);
                }

                foreach (var ft in FellowUsers)
                {
                    ft.IsCreatorVisible = IsTripOwner && TripStatusId == "3"; 
                }


                AtLeastOneFellowUser = FellowUsers.Any();
                IsFellowTravelerEmpty = !AtLeastOneFellowUser;
                IsMyBooking = FellowUsers.Any(f => f.FellowUserId == currentUserId);
                IsTripOwner = trip?.UserId == currentUserId;
                CanAcceptPassenger = trip?.SeatsQuentity > 0;
                AcceptButtonColor = CanAcceptPassenger ? Color.FromArgb("#21842C") : Color.FromArgb("#808080");
            
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
                    // Перезагружаем список попутчиков каждые 2 секунды
                    await LoadFellowTravelersCommand.ExecuteAsync(null);
                });
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        private void StopReloadTimer()
        {
            _reloadTimer?.Dispose();
            _reloadTimer = null;
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
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось принять пользователя!\nВозможно проблемы с интернетом", "Ок"); }
        }



        [RelayCommand]
        public async Task GoBackTripsDetails()
        {
            try
            {
                StopReloadTimer();
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
            StopReloadTimer();
            Preferences.Remove("SelectedTripId");
            await Shell.Current.GoToAsync("//SearchResultPage");
        }
    }
}
