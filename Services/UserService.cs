
using Firebase.Database;
using PoputkaKGAMT.Models;
using System.Diagnostics;

using System.Text.Json;

namespace PoputkaKGAMT.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService()
        {
            _httpClient = new HttpClient();
        }

        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        public async Task<List<UserModel>> GetUsers()
        {
            try
            {
                var allUsers = await firebase.Child("users").OnceAsync<Dictionary<string, object>>();

                var result = new List<UserModel>();

                foreach (var kvp in allUsers)
                {
                    if (kvp.Object is IDictionary<string, object> data)
                    {
                        var user = new UserModel
                        {
                            Id = kvp.Key,
                            Email = data["email"]?.ToString() ?? "",
                            Password = data["password"]?.ToString() ?? "",
                            Name = data["name"]?.ToString() ?? "",
                            ProfilePhoto = data["profile_photo"]?.ToString() ?? "",
                            Isdriver = int.TryParse(data["isdriver"]?.ToString(), out int d) ? d : 0,
                            Ispassenger = int.TryParse(data["ispassenger"]?.ToString(), out int p) ? p : 0,
                            Rating = double.TryParse(data["rating_core"]?.ToString(), out double r) ? r : 0,
                            ModelOfCar = data["user_car"]?.ToString() ?? "",
                            Registration = data["createdAt"]?.ToString() ?? "",
                        };

                        result.Add(user);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить\n" + ex.Message, "OK");
                return new List<UserModel>();
            }
        }


        public async Task<List<string>> GetEmailsOnly()
        {
            var firebase_url = "https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/users.json";
            try
            {
                var response = await _httpClient.GetStringAsync(firebase_url);
                var dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(response);

                return dict.Values.Where(u => u.ContainsKey("email")).Select(u => u["email"]?.ToString() ?? "").ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

    }
}