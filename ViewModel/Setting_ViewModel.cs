using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using PoputkaKGAMT.Models;


namespace PoputkaKGAMT.ViewModel
{

    internal partial class Setting_ViewModel : ObservableObject
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");


        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("//ProfilePage");
        }



        // Изменение аватара
        [RelayCommand]
        private async Task ChangeAvatar() { 
        
            
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


        // Изменение имени
        [RelayCommand]
        private async Task ChangeUserName() {

            string newName = await Shell.Current.DisplayPromptAsync("Измение имени", "Введите новое имя", "Сохранить", "Отмена", "Имя пользователя", keyboard: Keyboard.Text);
            string userKey = Preferences.Get("CurrentUserKey", "");

            if (string.IsNullOrEmpty(newName)) return;

            if (newName.Length > 48)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Длина имени не должна превышать 50 символов", "OK");
                return;
            }

            try
            {
                var updateName = new Dictionary<string, object>
                {
                    ["name"] = newName
                }; 
                await firebase.Child("users").Child(userKey).PatchAsync(updateName);
                await Shell.Current.DisplayAlertAsync("Успешно!", $"Предыдущая имя изменена на {newName}", "OK");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось изменить имя\n" + ex.Message ,"Ок");  }
            
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
    }
}
