using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;


namespace PoputkaKGAMT.ViewModel
{

    public partial class Setting_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");
        private readonly UserService userService;
        public Setting_ViewModel()
        {
            userService = new UserService();
        }

        [ObservableProperty]
        private string carModellText;

        [RelayCommand]
        public async void LoadData()
        {
            try
            {
                string userKey = Preferences.Get("CurrentUserKey", "");

                var allUsers = await userService.GetUsers();
                var userData = allUsers.FirstOrDefault(u => u.Id == userKey);

                CarModellText = string.IsNullOrWhiteSpace(userData?.ModelOfCar) ? "Не загружена с БД" : userData.ModelOfCar;
            }
            catch (Exception ex)
            {
                CarModellText = "Не загружена с БД";
            }
        }


        // Изменение аватара
        [RelayCommand]
        private async Task ChangeAvatar() 
        { 
            string userKey = Preferences.Get("CurrentUserKey", "");
            
            try
            {
                var results = await MediaPicker.Default.PickPhotosAsync(); 
                var photo = results?.FirstOrDefault();

                if (photo == null) return;
                
                using var stream = await photo.OpenReadAsync();


                // 2. Загрузка в Storage → получаем downloadUrl
                //string downloadUrl = await UploadAvatarToFirebaseStorageAsync(stream, userKey);

                // 3. Обновляем поле avatar у пользователя в Realtime Database
                //var updateAvatar = new Dictionary<string, object>
                //{
                //["avatar"] = downloadUrl
                //};

                //await firebase.Child("users").Child(userKey).PatchAsync(updateAvatar);

                //await Shell.Current.DisplayAlertAsync("Успешно!", "Фото профиля обновлено", "Ок");


            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось изменить фото профиля\n" + ex.Message, "Ок"); }
        }


        // Изменение Модели машины
        [RelayCommand]
        private async Task ChangeCar() {

            string newCar = await Shell.Current.DisplayPromptAsync("Укажите детали автомобиля", "Введите детали строго по формату:\nмодель машины, три номера, цвет", "Сохранить", "Отмена", "Лада Гранта, 123, белый", keyboard: Keyboard.Text);
            string userKey = Preferences.Get("CurrentUserKey", "");

            if (newCar == null)
            {
                if (CarModellText != "Не указано")
                {
                    bool deleteCar = await Shell.Current.DisplayAlertAsync("Удаление автомобиля","Хотите-ли удалить сохранённый автомобиль?","Да","Нет");

                    if (deleteCar == true)
                    {
                        var updateCar = new Dictionary<string, object>
                        {
                            ["user_car"] = "Не указано"
                        };

                        await firebase.Child("users").Child(userKey).PatchAsync(updateCar);
                        CarModellText = "Не указано";
                    }
                }

                return;
            }

            // Проверка корректности написания модели машиины
            if (!Regex.IsMatch(newCar, @"^.+, \d{3}, .+$"))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Требуется формат: \"Модель машины, три номера, цвет\"", "OK");
                return;
            }
            
            newCar = newCar?.Trim();

            string normalizedCar = Regex.Replace(newCar.Trim(), @"\s*,\s*", ", ").Trim();

            try
            {
                var updateCar = new Dictionary<string, object>
                {
                    ["user_car"] = normalizedCar
                }; 
                await firebase.Child("users").Child(userKey).PatchAsync(updateCar);
                await Shell.Current.DisplayAlertAsync("Успешно!", $"Автомобиль {normalizedCar}", "OK");
                LoadData();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось изменить/указать автомобиль\n" + ex.Message ,"Ок");  }
            
        }


        private bool CorrectEmail(string Email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        // Изменение почты
        [RelayCommand]
        private async Task ChangeEmail()
        {
            string oldEmail = Preferences.Get("CurrentUserEmail", "");
            string newEmail = await Shell.Current.DisplayPromptAsync("Изменение логина", "Ваша текущая почта " + oldEmail, "Изменить", "Отмена", "Новая почта", keyboard: Keyboard.Text);

            string userKey = Preferences.Get("CurrentUserKey", "");


            if(string.IsNullOrEmpty(newEmail)) return;

            if (!CorrectEmail(newEmail))
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не корректный формат почты", "Oк");
                return;
            }

            try
            {
                var updateEmail = new Dictionary<string, object>
                {
                    ["email"] = newEmail
                };

                await firebase.Child("users").Child(userKey).PatchAsync(updateEmail);
                await Shell.Current.DisplayAlertAsync("Успешно!", "Предыдущая почта была изменена на " + newEmail, "Ок");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось изменить почту\n" + ex.Message, "Ок"); }

        }

        // Изменение Пароля
        [RelayCommand]
        private async Task ChangePassword()
        {
            string oldPassword = Preferences.Get("CurrentUserPassword", "");
            string newPassword = await Shell.Current.DisplayPromptAsync("Изменение пароля", "Ваш текущий пароль " + oldPassword, "Изменить", "Отмена", "Новый пароль", keyboard: Keyboard.Text);

            string userKey = Preferences.Get("CurrentUserKey", "");



            if (string.IsNullOrEmpty(newPassword))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Введите пароль", "Oк");
                return; 
            }
            else if (newPassword.Length < 6)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Пароль не менее 6 символов", "Oк");
                return;
            }
                

            try
                {
                    var updatePassword = new Dictionary<string, object>
                    {
                        ["password"] = newPassword
                    };

                    await firebase.Child("users").Child(userKey).PatchAsync(updatePassword);
                    await Shell.Current.DisplayAlertAsync("Успешно!", "Предыдущий пароль была изменен!", "Ок");
                }
                catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Внимание", "Не удалось изменить пароль\n" + ex.Message, "Ок"); }
        }



        // Удаление аккаунта
        [RelayCommand]
        private async Task GoEntrance()
        {
            bool deleteAccount = await Shell.Current.DisplayAlertAsync("Внимание", "Вы точно хотите удалит аккаунт?", "Да", "Нет");
            if (!deleteAccount) return;

            try
            {    
                // Удаление поездок (Добавить удалить все поедзки от польщователя и его историю)
                var userKey = Preferences.Get("CurrentUserKey", "");

                var allTrips = await firebase.Child("trips").OnceAsync<TripModel>();
                var userTrips = allTrips.Where(trip => trip.Object.UserId == userKey);

                foreach (var trip in userTrips)
                {
                    await firebase.Child("trips").Child(trip.Key).DeleteAsync();
                }

                // Удалит отзывы

                // Удаление по почте
                string userEmail = Preferences.Get("CurrentUserEmail", "");

                var allusers = await firebase.Child("users").OnceAsync<Object>();

                var userToDelete = allusers.First(u => ((dynamic)u.Object).email.ToString() == userEmail);
                await firebase.Child("users").Child(userToDelete.Key).DeleteAsync();

                // Очистка всех ссылок
                Preferences.Clear();
                await Shell.Current.GoToAsync("//EntrancePage");
                await Shell.Current.DisplayAlertAsync("Успешно!", "Аккаунт удалён!", "OK");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Ошибка", ex.Message, "OK");  }

        }

        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("//ProfilePage");
        }
    }
}
