using Firebase.Database;
using PoputkaKGAMT.Models;

namespace PoputkaKGAMT.Services
{
    public class StatusService
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        public async Task<List<StatusModel>> GetStatuses()
        {
            try
            {
                var allStatuses = await firebase.Child("statuses").OnceAsync<Dictionary<string, object>>();
               
                var result = new List<StatusModel>();
                foreach (var kvp in allStatuses)
                {
                    if (kvp.Object is IDictionary<string, object> data)
                    {
                        var status = new StatusModel
                        {
                            Id = kvp.Key,  
                            Name = data["name"]?.ToString() ?? ""
                        };
                        result.Add(status);
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить\n" + ex.Message, "OK");
                return new List<StatusModel>();
            }

        }
    }
}
