using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using PoputkaKGAMT.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Text;


namespace PoputkaKGAMT.ViewModel
{
    
    partial class Entrance_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        [ObservableProperty]
        private string emailcheck = "";

        [ObservableProperty]
        private string passwordcheck = "";



        
        // Вход
        [RelayCommand]
        private async Task GoSearch()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Emailcheck) || string.IsNullOrWhiteSpace(Passwordcheck))
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Заполните поля", "Ок");
                    return;
                }

               
                // Получаем данные из БД
                var alluser = await firebase.Child("users").OnceAsync<UserModel>();

                var foundUser = alluser.FirstOrDefault(u => u.Object?.Email == Emailcheck);

                if (foundUser?.Object == null)
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Пользователь не найден или неверный логин", "OK");
                    return;
                }

                if (foundUser.Object.Password != Passwordcheck)
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Неверный пароль", "OK");
                    return;
                }

                Preferences.Set("CurrentUserKey", foundUser.Key);
                Preferences.Set("CurrentUserPassword", Passwordcheck);
                Preferences.Set("CurrentUserEmail", Emailcheck);
                await Shell.Current.GoToAsync("//SearchPage");

                Emailcheck = "";
                Passwordcheck = "";

            } 
            catch (Exception ex) {

                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось войти\nВозможно отсутствует подключение к сети Интернет\nОшибка:\n" + ex.Message, "OK");
            }


        }



        // Регистрация
        [RelayCommand]
        private async Task GoRegistration()
        {
            await Shell.Current.GoToAsync("//RegistrationPage");
        }
    }
}
