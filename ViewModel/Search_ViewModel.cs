using Firebase.Database;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoputkaKGAMT.Models;
using System.Collections.ObjectModel;
using PoputkaKGAMT.Services;

namespace PoputkaKGAMT.ViewModel
{
    public partial class Search_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        public Search_ViewModel()
        {
            SelectedDate = DateTime.Today;
        }



        [RelayCommand]
        private async Task GoSite()
        {
            await Browser.OpenAsync("https://www.auto-meh.ru/", BrowserLaunchMode.SystemPreferred);
        }

        // 1. Откуда - Куда 
        [ObservableProperty]
        private string departurePlaceSearchPage;

        [ObservableProperty]
        private string arrivePlaceSearchPage;

        // 2. Время и дата
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
                if (DeparturePlaceSearchPage == null || ArrivePlaceSearchPage == null)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Заполните поля!", "OK");
                    return;
                }

                if (DeparturePlaceSearchPage == ArrivePlaceSearchPage)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Место отъезда и прибытия не могут быть одинаковыми!", "OK");
                    return;
                }
                else if ((DeparturePlaceSearchPage == "КГАМТ" || DeparturePlaceSearchPage == "ост. Автостанция") &&
                        (ArrivePlaceSearchPage == "КГАМТ" || ArrivePlaceSearchPage == "ост. Автостанция"))
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Нельзя ехать между КГАМТ и Автостанцией!", "OK");
                    return;
                }
                else if ((ArrivePlaceSearchPage != "КГАМТ" && ArrivePlaceSearchPage != "ост. Автостанция") &&
                        (DeparturePlaceSearchPage != "КГАМТ" && DeparturePlaceSearchPage != "ост. Автостанция"))
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Выезд или приезд возможен только через КГАМТ или о.Автостанция", "OK");
                    return;
                }
                else
                {
                    SearchDeparturePlace = DeparturePlaceSearchPage;
                    SearchArrivePlace = ArrivePlaceSearchPage;
                    SearchDate = SelectedDate;
                    SearchPassengerCount = SelectedPessengerQuentity;
                    Preferences.Set("SearchDeparturePlace", DeparturePlaceSearchPage);
                    Preferences.Set("SearchArrivePlace", ArrivePlaceSearchPage);
                    Preferences.Set("SearchDate", SelectedDate.Ticks);
                    Preferences.Set("SearchPassengerCount", SelectedPessengerQuentity);

                    App.Parameters = true;
                    await Shell.Current.GoToAsync("//SearchResultPage");

                    DeparturePlaceSearchPage = null;
                    ArrivePlaceSearchPage = null;
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

        // Места
        [RelayCommand]
        private async Task GoPlacePage(string target)
        {
            Preferences.Set("PlaceTarget", target);
            Preferences.Set("PreviousPage", "SearchPage");
            await Shell.Current.GoToAsync("//PlacePage");
        }

    }
}
