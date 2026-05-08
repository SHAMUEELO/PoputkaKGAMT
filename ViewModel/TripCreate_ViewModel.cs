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
   

    public partial class TripCreate_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        private readonly PlaceService placeService;
        private readonly UserService userService;

        public TripCreate_ViewModel()
        {
            placeService = new PlaceService();
            userService = new UserService();
            LoadData();

            selectedTime = new TimeSpan(15, 45, 0);
            SelectedDate = DateTime.Today;            
        }

        [ObservableProperty]
        private ObservableCollection<PlaceModel> places = new();

        [ObservableProperty]
        private string userId;

        [RelayCommand]
        public async void LoadData()
        {
            var allPlaces = await placeService.GetPlaces();
            Places = new ObservableCollection<PlaceModel>(allPlaces);

            UserId = Preferences.Get("CurrentUserKey", "");

            var allUsers = await userService.GetUsers();
            var userData = allUsers.FirstOrDefault(u => u.Id == UserId);

            if (!string.IsNullOrWhiteSpace(userData?.ModelOfCar) && userData.ModelOfCar != "Не указано")
            {
                CarDescription = userData.ModelOfCar;
            }
        }

        [ObservableProperty]
        private bool isDriver;

        // Форма заполнения //
        // 1. Откуда - Куда 
        [ObservableProperty]
        private string departurePlace;

        [ObservableProperty]
        private string arrivePlace;

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

        // 5. Модель машины
        // Выбор из списков - 1. Модель
        [ObservableProperty]
        private string[] carrModel = new[]
        {
            "Lada Granta",
            "Vesta",
            "Niva",
            "Kia Rio",
            "Hyundai Solaris",
            "Volkswagen Polo",
            "Renault Sandero",
        };
        [ObservableProperty]
        private string selectedCarrModel;

        // Выбор из списков - 2. Номер
        [ObservableProperty]
        private string carNumber;

        // Выбор из списков - 3. Цвет
        [ObservableProperty]
        private string[] carrColor = new[]
        {
            "Белый",
            "Черный",
            "Серый",
            "Синий",
            "Коричневый",
            "Красный",
            "Жёлтый",
        };
        [ObservableProperty]
        private string selectedCarrColor;

        // Ручной ввод
        [ObservableProperty]
        private string carDescription;


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

        [ObservableProperty]
        public bool navigateToPlacePage;

        [RelayCommand]
        private async Task GoPublish()
        {
            IsDriver = !App.IsPassenger;
            CarNumber = CarNumber?.Trim();

            if (DeparturePlace == null || ArrivePlace == null ||
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
            if (!int.TryParse(Price, out int price) || price <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Цена не может быть отрицательной!", "OK");
                Price = "0"; return;
            }
            // проверка корректности использования мест отьезда
            if (DeparturePlace == ArrivePlace)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Место отъезда и прибытия не могут быть одинаковыми!", "OK");
                return;
            }
            else if ((DeparturePlace == "КГАМТ" || DeparturePlace == "ост. Автостанция") &&
                    (ArrivePlace == "КГАМТ" || ArrivePlace == "ост. Автостанция"))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Нельзя ехать между КГАМТ и Автостанцией!", "OK");
                return;
            }
            else if ((ArrivePlace != "КГАМТ" && ArrivePlace != "ост. Автостанция") &&
                    (DeparturePlace != "КГАМТ" && DeparturePlace != "ост. Автостанция"))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Выезд или приезд возможен только через КГАМТ или о.Автостанция", "OK");
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
                Price = Price?.Trim();

                // ===== Проверка и выбор автомобиля =====
                string manualCarDescription = CarDescription?.Trim() ?? "";

                // Проверяем, заполнена ли каждая из 3 переменных из Picker:
                bool modelFilled = !string.IsNullOrWhiteSpace(SelectedCarrModel);
                bool numberFilled = !string.IsNullOrWhiteSpace(CarNumber);
                bool colorFilled = !string.IsNullOrWhiteSpace(SelectedCarrColor);

                // Считаем, сколько из трёх полей заполнено: 0, 1, 2 или 3
                int filledCount = (modelFilled ? 1 : 0) + (numberFilled ? 1 : 0) + (colorFilled ? 1 : 0);
                bool hasAnyPickerCar = filledCount > 0; // true, если заполнено хотя бы одно поле Picker
                bool hasPickerCar = filledCount == 3; // true, если заполнены все 3 поля Picker
                bool hasManualCar = !string.IsNullOrWhiteSpace(manualCarDescription); // true, если заполнен ручной ввод CarDescription.

                if (hasAnyPickerCar)
                {
                    if (!Regex.IsMatch(CarNumber ?? "", @"^\d{3}$"))
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание", "Номер автомобиля должен состоять ровно из 3 цифр!", "OK");
                        CarNumber = "";
                        return;
                    }

                    if (!int.TryParse(CarNumber, out int number) || number <= 0)
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание", "Номер автомобиля должен быть положительным!", "OK");
                        CarNumber = "";
                        return;
                    }
                }

                // 1) Если заполнено 1 или 2 поля picker, а CarDescription пустой
                if (hasAnyPickerCar && !hasPickerCar && !hasManualCar)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание","Если используете поля Модель, Номер и Цвет, заполните их все,\nлибо вручную напишите автомобиль","OK");
                    return;
                }

                // 2) Если заполнено 1 или 2 поля picker, а CarDescription заполнен
                if (hasAnyPickerCar && !hasPickerCar && hasManualCar)
                {
                    if (!Regex.IsMatch(manualCarDescription, @"^.+, \d{3}, .+$")) // Проверяем, что ручной ввод соответствует формату
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание","Ручной ввод автомобиля должен быть строго в формате: Лада Гранта, 123, белый","OK");
                        return;
                    }

                    // Спрашиваем пользователя: оставить ручной ввод или нет
                    bool useManualCar = await Shell.Current.DisplayAlertAsync("Выбор автомобиля","Вы указали свой автомобиль, но поля Модель / Номер / Цвет заполнены не полностью.\n\nИспользовать ручной ввод?","Да","Нет");

                    if (useManualCar == true)
                    {
                        CarDescription = manualCarDescription;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание", "Заполните все три поля: Модель, Номер и Цвет","OK");
                        return;
                    }
                }

                // 3) Если ничего не заполнено вообще
                if (!hasAnyPickerCar && !hasManualCar)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание","Укажите автомобиль: либо заполните Модель, Номер и Цвет, либо укажите вручную","OK");
                    return;
                }

                // 4) Если CarDescription заполнен и picker заполнен полностью
                string fullCarDescription = hasPickerCar ? $"{SelectedCarrModel}, {CarNumber}, {SelectedCarrColor}" : ""; // Собираем строку вида: "Модель, Номер, Цвет"

                string carForTrip; // Итоговая строка автомобиля, которую будем отправлять дальше

                // Если заполнены и Picker-поля, и CarDescription
                if (hasPickerCar && hasManualCar)
                {
                    // Спрашиваем, какой вариант оставить: Picker или ручной ввод
                    bool usePickerCar = await Shell.Current.DisplayAlertAsync("Выбор автомобиля","Заполнены и Модель/Номер/Цвет, и поле с ручным вводом\n\nИспользовать автомобиль составленный из списков?","Да","Нет");

                    // Если выбрали Picker — берём fullCarDescription, а если нет — берём ручной ввод.
                    carForTrip = usePickerCar ? fullCarDescription : manualCarDescription;

                    // Если выбрали ручной ввод, но он не соответствует формату — ошибка
                    if (!usePickerCar && !Regex.IsMatch(manualCarDescription, @"^.+, \d{3}, .+$"))
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание", "Ручной ввод автомобиля должен быть строго в формате: Лада Гранта, 123, белый","OK");
                        return;
                    }
                }
                else if (hasPickerCar) // Если заполнены только 3 поля Picker
                {
                    carForTrip = fullCarDescription;
                }
                else // Иначе остаётся только ручной ввод
                {
                    if (!Regex.IsMatch(manualCarDescription, @"^.+, \d{3}, .+$"))
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание","Ручной ввод автомобиля должен быть строго в формате: Лада Гранта, 123, белый","OK");
                        return;
                    }

                    carForTrip = manualCarDescription;
                }

                // Убираем лишние пробелы вокруг запятых, а затем меняем формат для БД 
                string normalizedCar = Regex.Replace(carForTrip.Trim(), @"\s*,\s*", ", ").Trim();
                string newCarDescription = Regex.Replace(normalizedCar, ",", " -");

                try
                {
                    // сохранение пользователя в FirebaseЫ
                    var trip = new
                    {
                        id = "",
                        user_id = UserId,
                        departure = DeparturePlace, //departure_id
                        arrive = ArrivePlace, // arrive_id
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

                    NavigateToPlacePage = false;

                    DeparturePlace = null;
                    ArrivePlace = null;
                    Price = "";
                    CarDescription = "";
                    SelectedCarrModel = "";
                    CarNumber = "";
                    SelectedCarrColor = "";
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

        // Места
        [RelayCommand]
        private async Task GoPlacePage(string target)
        {
            NavigateToPlacePage = true;
            Preferences.Set("PlaceTarget", target);
            Preferences.Set("PreviousPage", "TripCreatePage");
            await Shell.Current.GoToAsync("//PlacePage");
        }

        [RelayCommand]
        public async Task OnMainPage()
        {
            await Shell.Current.GoToAsync("//SearchPage");
        }
    }
}
