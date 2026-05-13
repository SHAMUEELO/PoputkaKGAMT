using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;

namespace PoputkaKGAMT.ViewModel
{
    partial class CheckProfile_ViewModel : ObservableObject
    {
        private readonly UserService userService;

        public CheckProfile_ViewModel()
        {
            userService = new UserService();
        }

        [ObservableProperty]
        private UserModel сheckSelectedUser;

        [RelayCommand]
        public async void UserProfileLoad()
        {
            try
            {
                var allUsers = await userService.GetUsers();
                
                string userId = Preferences.Get("SelectedUserIdForCheckProfile", "");

                Console.WriteLine($"🔥 Ищем пользователя ID: '{userId}'");
                СheckSelectedUser = allUsers.FirstOrDefault(u => u.Id == userId);
                if (СheckSelectedUser != null)
                {
                    Console.WriteLine($"✅ НАЙДЕН: {СheckSelectedUser.Name}");
                    Console.WriteLine($"   ID: {СheckSelectedUser.Id}");
                    Console.WriteLine($"   Регистрация: {СheckSelectedUser.Registration}");
                    Console.WriteLine($"   Рейтинг: {СheckSelectedUser.Rating}");
                    Console.WriteLine($"   Водитель: {СheckSelectedUser.Isdriver}");
                    Console.WriteLine($"   Пассажир: {СheckSelectedUser.Ispassenger}");
                }
                else
                {
                    Console.WriteLine($"❌ ПОЛЬЗОВАТЕЛЬ НЕ НАЙДЕН по ID: '{userId}'");
                    Console.WriteLine($"   Всего пользователей в БД: {allUsers.Count}");
                }

            }
            catch (Exception ex)
            {
                await Shell.Current.GoToAsync("//SearchPage");
                await Shell.Current.DisplayAlertAsync("Внимание", "Время ожидания истекло или возникли неполадки", "OK");
            }
        }


        [RelayCommand]
        public async Task GoBack()
        {
            Preferences.Remove("SelectedUserIdForCheckProfile");
            string PreviousPageCheckProfile = Preferences.Get("PreviousPageCheckProfile", "");
            // Если ссылка пуста, то по умолчанию SearchResultPage
            if (string.IsNullOrEmpty(PreviousPageCheckProfile))
                PreviousPageCheckProfile = "SearchResultPage";
            await Shell.Current.GoToAsync($"//{PreviousPageCheckProfile}");
        }
    }
}
