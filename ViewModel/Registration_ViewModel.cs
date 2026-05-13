using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PoputkaKGAMT.ViewModel
{
    partial class Registration_ViewModel : ObservableObject
    {

        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        // Загрузка только почты
        private readonly UserService userService;
        
        [ObservableProperty]
        private string[] allEmails = [];
        public Registration_ViewModel()
        {
            userService = new UserService();
            LoadUserEmail();
        }

        private async void LoadUserEmail() 
        { 
            //var emailList = await userService.GetEmailsOnly();
            //AllEmails = emailList.ToArray();
        }

        // Проверка корректной формы почты
        private bool CorrectEmail(string Email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        [ObservableProperty]
        private string email = "";

        [ObservableProperty]
        private string password = ""; // Не менее 8  символов

        [ObservableProperty]
        private string userName = ""; 
        
        [RelayCommand]
        private async Task GoSearch()
        {
            try
            {
                 
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(UserName))
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Заполните поля!", "OK");
                    return;
                }

                if (Password.Length < 6)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Пароль не менее 6 символов", "OK");
                    return;
                }

                
                foreach (var emailOnDB in AllEmails)
                { 
                    if (Email == emailOnDB)
                    {
                        await Shell.Current.DisplayAlertAsync("Внимание", "Данная почта уже привязана!", "OK");
                        return;
                    }
                }

                if (!CorrectEmail(Email))
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Не корректный формат почты", "OK");
                    return;
                }

                if (UserName.Length > 48)
                {
                    await Shell.Current.DisplayAlertAsync("Внимание", "Длина имени не должна превышать 50 символов", "OK");
                    return;
                }

                // сохранение пользователя в Firebase
                var user = new
                {
                    id = "",
                    email = Email,
                    password = Password,
                    name = UserName,
                    profile_photo = "", 
                    isdriver = 0,    
                    ispassenger = 0,
                    rating_core = 0.00,
                    user_car = "Не указано",
                    one_star = 0,
                    two_star = 0,
                    three_star = 0,
                    four_star = 0,
                    five_star = 0,
                    total_ratings = 0.00,
                    createdAt = DateTime.Now.ToString("dd.MM.yyyy")
                };

                // Сохраняем в users
                var result = await firebase.Child("users").PostAsync(user);
                await firebase.Child("users").Child(result.Key).PatchAsync(new { id = result.Key });

                Preferences.Set("CurrentUserKey", result.Key);
                Preferences.Set("CurrentUserPassword", Password);
                Preferences.Set("CurrentUserEmail", Email);
                Preferences.Set("CurrentUserName", UserName);
                

                await Shell.Current.GoToAsync("//SearchPage");
                await Shell.Current.DisplayAlertAsync("Успешно", $"Добро пожаловать, {UserName}!", "OK");

                Email = "";
                Password = "";
                UserName = "";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось зарегистрироваться\nВозможно проблемы с интернетом\nОшибка:\n" + ex.Message, "OK");
            }
        }
       

        // Назад
        [RelayCommand]
        public async Task GoEntrace()
        {
            await Shell.Current.GoToAsync("//EntrancePage");
        }

    }
}
