using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace PoputkaKGAMT.ViewModel
{
    partial class TripDetails_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");
        private readonly TripService tripService;
        private readonly UserService userService;
        private readonly PlaceService placeService;
        private readonly StatusService statusService;
        private readonly FellowTravelerService fellowTravelerService;

        public TripDetails_ViewModel()
        {
            tripService = new TripService();
            userService = new UserService();
            placeService = new PlaceService();
            statusService = new StatusService();
            fellowTravelerService = new FellowTravelerService();
        }

        [ObservableProperty]
        private TripModel selectedTrip; // Данные выбранной поездки

        [ObservableProperty]
        private bool showUsersTrip = false; // Видимость "Отменить" для создтеля этой поездки

        [ObservableProperty]
        private bool showUsersTripForComplete = false; // Видимость "Завершить" для создтеля этой поездки
        [ObservableProperty]
        private Color completeTripButtonBackground = Color.FromArgb("#21842C"); // Зеленая для "Завершить поездку" при статусе 2 
        [ObservableProperty]
        private bool canCompleteTrip = true; 

        [ObservableProperty]
        private bool showTrip = true; // Видимость "Забронировать" для пассажира

        [ObservableProperty]
        private bool isOnlyTwo, isNotLate, isNotSmoking, isConditioner = false;

        [ObservableProperty]
        private bool isDriverTrip = true;

        [ObservableProperty]
        private bool isPassengerTrip = false;

        [ObservableProperty]
        private string roleText = "Свободных мест";

        [ObservableProperty]
        private bool canBookTrip = true;

        [ObservableProperty]
        private Color bookButtonBackground = Color.FromArgb("#214484");

        [ObservableProperty]
        private bool seatsLabelVisible = true;

        [ObservableProperty]
        private string roleBooking = "Забронировать";

        [RelayCommand]
        public async Task LoadTrip()
        {
            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");

                var allTrips = await tripService.GetTrips();
                var allUsers = await userService.GetUsers();
                var allPlaces = await placeService.GetPlaces();

                var trip = allTrips.FirstOrDefault(t => t.Id == tripId);
                

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

                // Загружаем данные от лица водителя или от лица пассажира
                trip.Role = trip.IsDriver ? "Водитель" : "Пассажир";
                if (trip.IsDriver == false)
                {
                    IsDriverTrip = false;
                    IsPassengerTrip = true;
                    RoleBooking = "Откликнуться";
                }
                else
                {
                    IsDriverTrip = true;
                    IsPassengerTrip = false;
                    RoleBooking = "Забронировать";
                }

                RoleText = trip.RoleText;
                CanBookTrip = trip.CanBookTrip;
                SeatsLabelVisible = trip.SeatsLabelVisible;
                BookButtonBackground = trip.BookButtonBackground;

                // Логика показа спец. кнопок для водителя или пассажира
                string currentUserId = Preferences.Get("CurrentUserKey", "");
                if (trip.UserId == currentUserId)
                {
                    if (trip.StatusId == "3")
                    {
                        ShowUsersTrip = true;
                        ShowTrip = false;
                        ShowUsersTripForComplete = false;
                    }
                    else if (trip.StatusId == "2" || trip.StatusId == "1")
                    {
                        ShowUsersTripForComplete = true;
                        ShowTrip = false;
                        ShowUsersTrip = false;

                        // Цвет кнопки: зелёный если 2, серый если 1
                        if (trip.StatusId == "2")
                        {
                            CompleteTripButtonBackground = Color.FromArgb("#21842C"); // зелёный
                            CanCompleteTrip = true;
                        }
                        if (trip.StatusId == "1")
                        {
                            CompleteTripButtonBackground = Color.FromArgb("#808080"); // серый
                            CanCompleteTrip = false;
                        }
                    }
                }

                if (trip.UserId != currentUserId) { ShowUsersTrip = false; ShowTrip = true; }

                // Дополнительные детали
                IsOnlyTwo = trip.MaxBack;
                IsNotLate = trip.OnTime;
                IsNotSmoking = trip.NoSmoking;
                IsConditioner = trip.AirConditioing;


                // Загружаем выбранные элементы
                SelectedTrip = trip;

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить данные!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
            }
        }


        [RelayCommand]
        private async Task DeleteTrip()
        {

            bool delete = await Shell.Current.DisplayAlertAsync("Внимание", "Вы точно хотите убрать объявление?", "Да", "Нет");
            if (!delete) return;

            try
            {
                if (delete)
                {
                    string tripId = Preferences.Get("SelectedTripId", "");
                    await firebase.Child("trips").Child(tripId).DeleteAsync();

                    var allFellowTravelers = await fellowTravelerService.GetFellowTravelers(); //Все данные из таблицы
                                                                                               // Надо по всем данным пройтись, найти TipId и как только мы найдем его, удалить Id этого FelllowTraveler

                    foreach (var fellowTraveler in allFellowTravelers)
                    {
                        if (fellowTraveler.TripId == tripId) // или fellowTraveler.TripId
                        {
                            await firebase.Child("fellow_travelers").Child(fellowTraveler.Id).DeleteAsync();
                        }
                    }

                }

                Preferences.Remove("SelectedTripId");
                SelectedTrip = null;
                ShowUsersTrip = false;
                ShowTrip = true;

                string previousPage = Preferences.Get("PreviousPage", "");
                // Если ссылка пуста, то по умолчанию SearchResultPage
                if (string.IsNullOrEmpty(previousPage))
                    previousPage = "SearchResultPage";

                await Shell.Current.GoToAsync($"//{previousPage}");

            }
            catch(Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось отменить поездку!\nВозможно проблемы с интернетом", "Ок");  } 
        }

        [RelayCommand]
        private async Task CompleteTrip()
        {
            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                await firebase.Child("trips").Child(tripId).PatchAsync(new { status_id = "1" }); // обновить в Firebase

                string currentUserId = Preferences.Get("CurrentUserKey", "");
                var allUsers = await userService.GetUsers(); 
                var currentUser = allUsers.FirstOrDefault(u => u.Id == currentUserId);

                if (currentUser != null)
                {
                    // Выбираем поле по IsDriver
                    int newCount;
                    if (SelectedTrip.IsDriver)
                    {
                        newCount = currentUser.Isdriver + 1;
                        await firebase.Child("users").Child(currentUserId).PatchAsync(new {isdriver = newCount });
                    }
                    else
                    {
                        newCount = currentUser.Ispassenger + 1;
                        await firebase.Child("users").Child(currentUserId).PatchAsync(new {ispassenger = newCount });
                    }
                }


                await Shell.Current.DisplayAlertAsync("Успешно", "Ваша поездка была завершена!", "Ок");
                SelectedTrip.StatusId = "1";
                // Кнопка теперь серая и не возможно нажать
                CompleteTripButtonBackground = Color.FromArgb("#808080");
                CanCompleteTrip = false;

                ShowUsersTripForComplete = false;
                ShowTrip = true;

                string previousPage = Preferences.Get("PreviousPage", "");
                // Если ссылка пуста, то по умолчанию SearchResultPage
                if (string.IsNullOrEmpty(previousPage))
                    previousPage = "SearchResultPage";

                await Shell.Current.GoToAsync($"//{previousPage}");

            }
            catch(Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось завершить поездку!\nВозможно проблемы с интернетом", "Ок");  } 
        }

        [RelayCommand]
        private async Task AddBookingTable() 
        {

            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                string currentUserId = Preferences.Get("CurrentUserKey", "");

                var allTrips = await tripService.GetTrips();
                var trip = allTrips.FirstOrDefault(t => t.Id == tripId);

                // Проверка на копию пользователя
                var allFellowTravelers = await fellowTravelerService.GetFellowTravelers();
                var myExistingBooking = allFellowTravelers.FirstOrDefault(f => f.TripId == tripId && f.FellowUserId == currentUserId);
                if (myExistingBooking != null)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Вы уже забронировали/откликнулись на эту поездку!", "OK");
                    return; 
                }


                // сохранение пользователя в Firebase, если он еще не бронировал
                if (RoleBooking == "Забронировать")
                {
                    var fellow_user = new
                    {
                        id = "",
                        fellow_user_id = Preferences.Get("CurrentUserKey", ""),
                        trip_user_id = trip.UserId,// Водитель или пассажир(короче создатель поездки)
                        trip_id = tripId,// Id поездки,куда отправила бронь
                        status_id = "5",
                        fellow_user_is_driver = false, // Роль того, кто отправляет(Если смотрит поездку от водителя), то пассажир
                        trip_user_is_driver = true, // Роль того,кто создл поездку
                        car_description = "",
                        createdAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                    };

                    // Сохраняем в users
                    var result = await firebase.Child("fellow_travelers").PostAsync(fellow_user);
                    await firebase.Child("fellow_travelers").Child(result.Key).PatchAsync(new { id = result.Key });

                    await Shell.Current.DisplayAlertAsync("Успешно", "Ваш запрос отправлен!\nОжидайте решение водителя\n\nВаш статус запроса можно посмотреть перейдя по «Попутный состав»", "Ок");

                }
                else if (RoleBooking == "Откликнуться")
                {
                    // Добавление машины и проверка его корректности
                    string carDescription = await Shell.Current.DisplayPromptAsync("Опишите автомобиль", "Введите в формате: Лада Гранта, 123, белый", "Отправить", "Отмена", "Ваш автомобиль", keyboard: Keyboard.Text);
                    if (!Regex.IsMatch(carDescription, @"^.+, \d{3}, .+$"))
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание", "Требуется формат: \"Лада Гранта, 123, белый\"", "OK");
                        return;
                    }
                    string normalizedCar = Regex.Replace(carDescription.Trim(), @"\s*,\s*", ", ").Trim();
                    string newCarDescription = Regex.Replace(normalizedCar, ",", " -");

                    // сохранение пользователя в Firebase
                    var fellow_user = new
                    {
                        id = "",
                        fellow_user_id = Preferences.Get("CurrentUserKey", ""),
                        trip_user_id = trip.UserId,// Водитель или пассажир(короче создатель поездки)
                        trip_id = tripId,// Id поездки,куда отправила бронь
                        status_id = "5",
                        fellow_user_is_driver = true, // Роль того, кто отправляет(Если смотрит поездку от водителя), то пассажир
                        trip_user_is_driver = false, // Роль того,кто создл поездку
                        car_description = newCarDescription,
                        createdAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                    };
                    // Сохраняем в users
                    var result = await firebase.Child("fellow_travelers").PostAsync(fellow_user);
                    await firebase.Child("fellow_travelers").Child(result.Key).PatchAsync(new { id = result.Key });

                    await Shell.Current.DisplayAlertAsync("Успешно", "Ваш запрос отправлен!\nОжидайте решение пассажира\n\nВаш статус запроса можно посмотреть перейдя по «Попутный состав»", "Ок");
                }
            }
            catch(Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось отправить бронь!\nВозможно проблемы с интернетом", "Ок"); }
            
        }

        // К попутным пассажирам
        [RelayCommand]
        private async Task GoFellowTravelerPage()
        {
            try
            {
                Preferences.Set("SelectedTripId", SelectedTrip.Id);
                await Shell.Current.GoToAsync("//FellowTravelerPage");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось открыть страницу!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "Ок");  }
        }

        // К профилю пользователя
        [RelayCommand]
        private async Task GoCheckProfile()
        {
            try
            {
                string tripId = Preferences.Get("SelectedTripId", "");
                var allTrips = await tripService.GetTrips();
                var allUsers = await userService.GetUsers();

                var trip = allTrips.FirstOrDefault(t => t.Id == tripId);
                var user = allUsers.FirstOrDefault(u => u.Id?.Equals(trip.UserId, StringComparison.OrdinalIgnoreCase) == true);

                Preferences.Set("SelectedUserIdForCheckProfile", user.Id);
                Preferences.Set("PreviousPageCheckProfile", "TripDetailsPage");
                await Shell.Current.GoToAsync("//CheckProfilePage");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось открыть страницу!\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "Ок"); }
        }

        [RelayCommand]
        public async Task GoBackSearchResult()
        {
            Preferences.Remove("SelectedTripId");
            SelectedTrip = null;
            ShowUsersTrip = false;
            ShowTrip = true;

            IsOnlyTwo = false;
            IsNotLate = false; 
            IsNotSmoking = false;
            IsConditioner = false;

            string previousPage = Preferences.Get("PreviousPage", "");
            // Если ссылка пуста, то по умолчанию SearchResultPage
            if (string.IsNullOrEmpty(previousPage))
                previousPage = "SearchResultPage";

            await Shell.Current.GoToAsync($"//{previousPage}");
        }
    }
}
