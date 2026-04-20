using Firebase.Database;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoputkaKGAMT.Models;
using System.Collections.ObjectModel;
using PoputkaKGAMT.Services;

namespace PoputkaKGAMT.ViewModel
{
    internal partial class Search_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        private readonly PlaceService placeService;
        public Search_ViewModel()
        {
            placeService = new PlaceService();
            LoadPlace();

            SelectedDate = DateTime.Today;
        }

        [ObservableProperty]
        private ObservableCollection<PlaceModel> places = new();

        public async void LoadPlace() // Загружаем списки мест при запуске экрана
        {
            var allPlaces = await placeService.GetPlaces();
            Places = new ObservableCollection<PlaceModel>(allPlaces);
        }


        [RelayCommand]
        private async Task GoSite()
        {
            await Browser.OpenAsync("https://www.auto-meh.ru/", BrowserLaunchMode.SystemPreferred);
        }

        [ObservableProperty]
        PlaceModel selectedDeparturePlace, selectedArrivePlace;

        // Время и дата
        [ObservableProperty]
        private DateTime selectedDate;
        public DateTime MinDate => DateTime.Today;

        // 3. Количество пассажиров
        [ObservableProperty]
        private int[] pessengerQuentity = { 1, 2, 3, 4 };

        [ObservableProperty]
        private int selectedPessengerQuentity = 1;

        // Параметры поиска
        [ObservableProperty] 
        private string searchDeparturePlace;

        [ObservableProperty] 
        private string searchArrivePlace;

        [ObservableProperty] 
        private DateTime searchDate;

        [ObservableProperty] 
        private int searchPassengerCount;

        // Найти
        [RelayCommand]
        private async Task GoSearchResultFind()
        {
            try
            {
                if (SelectedDeparturePlace == null || SelectedArrivePlace == null)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Заполните поля!", "OK");
                    return;
                }

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
                else
                {
                    SearchDeparturePlace = SelectedDeparturePlace.Name;
                    SearchArrivePlace = SelectedArrivePlace.Name;
                    SearchDate = SelectedDate;
                    SearchPassengerCount = SelectedPessengerQuentity;
                    Preferences.Set("SearchDeparturePlace", SelectedDeparturePlace.Name);
                    Preferences.Set("SearchArrivePlace", SelectedArrivePlace.Name);
                    Preferences.Set("SearchDate", SelectedDate.Ticks);
                    Preferences.Set("SearchPassengerCount", SelectedPessengerQuentity);

                    App.Parameters = true;
                    await Shell.Current.GoToAsync("//SearchResultPage");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Заполните поля", "Ок");
                return;
            }
        }



        // Найти все
        [RelayCommand]
        private async Task GoSearchResultAll()
        {
            Preferences.Remove("SearchDeparturePlace");
            Preferences.Remove("SearchArrivePlace");
            Preferences.Remove("SearchDate");
            Preferences.Remove("SearchPassengerCount");

            App.Parameters = false;
            await Shell.Current.GoToAsync("//SearchResultPage");
        }
        
    }
}
