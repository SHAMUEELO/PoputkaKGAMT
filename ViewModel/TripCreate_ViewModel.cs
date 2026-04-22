using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Storage;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace PoputkaKGAMT.ViewModel
{
   

    partial class TripCreate_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        private readonly PlaceService placeService;

        public TripCreate_ViewModel()
        {
            placeService = new PlaceService();
            LoadData();

            selectedTime = new TimeSpan(15, 45, 0);
            SelectedDate = DateTime.Today;            
        }

        [ObservableProperty]
        private ObservableCollection<PlaceModel> places = new();

        [ObservableProperty]
        private string userId;

        public async void LoadData()
        {
            var allPlaces = await placeService.GetPlaces();
            Places = new ObservableCollection<PlaceModel>(allPlaces);

            UserId = Preferences.Get("CurrentUserKey", "");
        }

        [ObservableProperty]
        private bool isDriver;
        

        // Форма заполнения //
        // 1. Откуда - Куда 
        [ObservableProperty]
        private PlaceModel selectedDeparturePlace, selectedArrivePlace;

        // 2. Время и дата
        [ObservableProperty]
        private TimeSpan selectedTime;

        [ObservableProperty]
        private DateTime selectedDate;
        public DateTime MinDate => DateTime.Today;

        // 3. Количество пассажиров
        [ObservableProperty]
        private int[] pessengerQuentity = { 1, 2, 3, 4 };

        [ObservableProperty]
        private int selectedPessengerQuentity = 1;

        // 4. Цена
        [ObservableProperty]
        private string price;

        // 5. Модель машины*
        [ObservableProperty]
        private string carDescription = "";

        // 6. Дополнительное
        [ObservableProperty]
        private bool maxBack = false;

        [ObservableProperty]
        private bool noSmoking = false;

        [ObservableProperty]
        private bool onTime = false;

        [ObservableProperty]
        private bool airConditioing = true;

        // 7. Описание
        [ObservableProperty]
        private string description;

        [RelayCommand]
        private async Task GoPublish()
        {
            IsDriver = !App.IsPassenger;

            if (SelectedDeparturePlace == null || SelectedArrivePlace == null ||
                string.IsNullOrWhiteSpace(Price))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Заполните поля!", "OK");
                return;
            }
            if (Price.Length > 3) 
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Допустимая стоимость поездки — не более 999 рублей", "OK");
                Price = "";
                return;
            }
            if (Price.Length <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Стоимость поездки не может быть отрицательным", "OK");
                Price = "";
                return;
            }
            // проверка корректности использования мест отьезда
            if (SelectedDeparturePlace.Name == SelectedArrivePlace.Name)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Место отъезда и прибытия не могут быть одинаковыми!", "OK");
                return;
            }
            else if ((SelectedDeparturePlace.Name == "КГАМТ" || SelectedDeparturePlace.Name == "Автостанция ост.") &&
                    (SelectedArrivePlace.Name == "КГАМТ" || SelectedArrivePlace.Name == "Автостанция ост."))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Нельзя ехать между КГАМТ и Автостанцией!", "OK");
                return;
            }
            else if ((SelectedArrivePlace.Name != "КГАМТ" && SelectedArrivePlace.Name != "Автостанция ост.") &&
                    (SelectedDeparturePlace.Name != "КГАМТ" && SelectedDeparturePlace.Name != "Автостанция ост."))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Выезд или приезд возможен только через КГАМТ или о.Автостанция", "OK");
                return;
            }
            // Проверка корректности написания модели машиины
            else if (!Regex.IsMatch(CarDescription, @"^.+, \d{3}, .+$") && IsDriver == true)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Требуется формат: \"Лада Гранта, 123, белый\"", "OK");
                return;
            }
            else if (string.IsNullOrWhiteSpace(Description))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Обязательно заполните описание!\nУкажите точное место отъезда и/или прибытия", "Ок");
                return;
            }
            else if (string.IsNullOrWhiteSpace(Price) || !int.TryParse(Price.Trim(), out _))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Укажите стоимость поездки или введите корректный формат", "Ок");
                return;
            }
            else if (MaxBack == true && SelectedPessengerQuentity == 4)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "«Максимум двое сзади» не позволяет публиковать поездку с 4 пассажирами", "Ок");
                return;
            }
            //else if (IsTripTimeValid() == false)
            //{
            //    await Shell.Current.DisplayAlertAsync("Внимание", "Размещение поездки возможно не ранее чем за 10 минут до времени отъезда!", "Ок");
            //    return;
            //}
            else
            {
                // Меняем на нужный формат 
                Description = Description?.Trim();
                CarDescription = CarDescription?.Trim();
                Price = Price?.Trim();

                string normalizedCar = Regex.Replace(CarDescription.Trim(), @"\s*,\s*", ", ").Trim();

                string newCarDescription = Regex.Replace(normalizedCar, ",", " -");

                try
                {
                    // сохранение пользователя в FirebaseЫ
                    var trip = new
                    {
                        id = "",
                        user_id = UserId,
                        departure_id = SelectedDeparturePlace.Id,
                        arrive_id = SelectedArrivePlace.Id,
                        status_id = "3",
                        is_driver = IsDriver,
                        time = $"{SelectedTime.Hours:D2}:{SelectedTime.Minutes:D2}",  // "15:45"
                        date = SelectedDate.ToString("dd.MM.yyyy"),
                        seats_quentity = SelectedPessengerQuentity,
                        original_seats_quentity = SelectedPessengerQuentity,
                        price = int.Parse(Price),
                        car_model = newCarDescription,
                        max_back = MaxBack,
                        no_smoking = NoSmoking,
                        on_time = OnTime,
                        air_conditioing = AirConditioing,
                        description = Description,
                        createdAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                    };

                    // Сохраняем в trips
                    var result = await firebase.Child("trips").PostAsync(trip);
                    await firebase.Child("trips").Child(result.Key).PatchAsync(new { id = result.Key });

                    Preferences.Set("CurrentTripKey", result.Key); // Хзхз, пользователь может создать много поездок, и тут будет id Только последнего, поэтому смотрим по id пользоваетля

                    await Shell.Current.DisplayAlertAsync("Успешно", "Поездка создана", "OK"); //добавить картинку с галочкой . Вопрос, как оформить такие DisplayAlertAsync
                    await Shell.Current.GoToAsync("//SearchResultPage");



                    SelectedDeparturePlace = null;
                    SelectedArrivePlace = null;
                    Price = "";
                    CarDescription = "";
                    Description = "";
                    SelectedTime = new TimeSpan(15, 45, 0);
                    SelectedDate = DateTime.Today;
                    SelectedPessengerQuentity = 1;
                    MaxBack = false;
                    NoSmoking = false;
                    OnTime = false;
                    AirConditioing = true;

                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось опубликовать\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
                }
            }
            
        }
        private bool IsTripTimeValid()
        {
            DateTime tripDateTime = SelectedDate.Date + SelectedTime;
            DateTime now = DateTime.Now;

            // Проверяем, если дата поездки сегодня
            if (tripDateTime.Date == now.Date)
            {
                TimeSpan timeDifference = tripDateTime.TimeOfDay - now.TimeOfDay;

                if (timeDifference.TotalMinutes <= 10) // Если разница между текщим временем и временем,установленным для поездки, то false
                {
                    return false;
                }
            }
            return true;
        }

        [RelayCommand]
        public async Task OnMainPage()
        {
            await Shell.Current.GoToAsync("//SearchPage");
        }
    }
}
