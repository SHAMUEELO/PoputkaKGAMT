using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Collections.ObjectModel;


namespace PoputkaKGAMT.ViewModel
{
    
    internal partial class Profile_ViewModel : ObservableObject
    {
        private readonly UserService userService;
        public Profile_ViewModel()
        {
            userService = new UserService();
            LoadProfilInfo();
        }

        [ObservableProperty]
        private ObservableCollection<UserModel> users;

        [RelayCommand]
        public async void LoadProfilInfo()
        {
            // Загружаем все данные о пользователях в список users
            var allUsers = await userService.GetUsers(); 
            Users = new ObservableCollection<UserModel>(allUsers);
            
            // Загрузка данных
            string userKey = Preferences.Get("CurrentUserKey", "");
            var userData = Users.FirstOrDefault(u => u.Id == userKey);
                
            UserName = userData.Name;
            IsDriverProfile = userData.Isdriver;
            IsPassengerProfile = userData.Ispassenger;
            RatingCore = userData.Rating;

        }

        [ObservableProperty]
        public string userName;

        [ObservableProperty]
        public int isDriverProfile, isPassengerProfile;

        [ObservableProperty]
        public double ratingCore;



        // Мои поездки
        [RelayCommand]
        private async Task GoMyTravelButton()
        {
            await Shell.Current.GoToAsync("//MyTravelPage");
        }

        // Рейтинг
        [RelayCommand]
        private async Task GoMyRating()
        {
            await Shell.Current.GoToAsync("//RatingPage");
        }


        // Найстройки
        [RelayCommand]
        private async Task GoSetting()
        {
            await Shell.Current.GoToAsync("//SettingPage");
        }


        // Выйти
        [RelayCommand]
        private async Task GoOutUser()
        {
            Preferences.Clear();
            App.IsPassenger = false;

            await Shell.Current.GoToAsync("//EntrancePage", true);
        }


        

        
    }
}
