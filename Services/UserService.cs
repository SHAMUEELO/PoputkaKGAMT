
using PoputkaKGAMT.Models;
using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task<List<UserModel>> GetUsers()
        {
            var firebase_url = "https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/users.json";
            try
            {
                var response = await _httpClient.GetStringAsync(firebase_url);
                var dict = JsonSerializer.Deserialize<Dictionary<string, UserModel>>(response); // Обрабатываем Json формат из firebase_url

                return dict.Values.ToList(); // Преобразуем Dictionary, полученный из firebase, в List 
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
