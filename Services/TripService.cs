using Firebase.Database;
using PoputkaKGAMT.Models;

namespace PoputkaKGAMT.Services
{
    public class TripService
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        public async Task<List<TripModel>> GetTrips()
        {
            try
            {
                var allTrips = await firebase.Child("trips").OnceAsync<Dictionary<string, object>>();

                var result = new List<TripModel>();
                foreach (var kvp in allTrips)
                {
                    if (kvp.Object is IDictionary<string, object> data)
                    {

                        var trip = new TripModel
                        {
                            Id = kvp.Key,
                            UserId = data["user_id"]?.ToString() ?? "",
                            DeparturePlaceId = data["departure"]?.ToString() ?? "",
                            ArrivePlaceId = data["arrive"]?.ToString() ?? "",
                            StatusId = data["status_id"]?.ToString() ?? "",
                            IsDriver = bool.TryParse(data["is_driver"]?.ToString(), out bool d) ? d : false,
                            Time = data["time"]?.ToString() ?? "",
                            Date = data["date"]?.ToString() ?? "",
                            SeatsQuentity = int.TryParse(data["seats_quentity"]?.ToString(), out int s) ? s : 0,
                            OriginalSeatsQuentity = int.TryParse(data["original_seats_quentity"]?.ToString(), out int orig_S) ? orig_S : 0,
                            Price = int.TryParse(data["price"]?.ToString(), out int p) ? p : 0,
                            CarDescrtiption = data["car_model"]?.ToString(),
                            MaxBack = bool.TryParse(data["max_back"]?.ToString(), out bool mb) ? mb : false,
                            NoSmoking = bool.TryParse(data["no_smoking"]?.ToString(), out bool ns) ? ns : false,
                            OnTime = bool.TryParse(data["on_time"]?.ToString(), out bool ot) ? ot : false,
                            AirConditioing = bool.TryParse(data["air_conditioing"]?.ToString(), out bool ac) ? ac : false,
                            Description = data["description"]?.ToString() ?? ""
                        };

                        result.Add(trip);
                    }
                }
                return result;
            }
            catch (Exception ex) 
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить\n" + ex.Message, "OK");
                return new List<TripModel>();
            }
        }
    }
}
