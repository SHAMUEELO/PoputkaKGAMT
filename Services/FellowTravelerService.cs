using Firebase.Database;
using PoputkaKGAMT.Models;

namespace PoputkaKGAMT.Services
{
    public class FellowTravelerService
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        public async Task<List<FellowTravelerModel>> GetFellowTravelers()
        {
            try
            {
                var allFellowTravelers = await firebase.Child("fellow_travelers").OnceAsync<Dictionary<string, object>>();

                var result = new List<FellowTravelerModel>();
                foreach (var kvp in allFellowTravelers)
                {
                    if (kvp.Object is IDictionary<string, object> data)
                    {

                        var FellowTraveler = new FellowTravelerModel
                        {
                            Id = kvp.Key,
                            TripUserId = data["trip_user_id"]?.ToString() ?? "",
                            FellowUserId = data["fellow_user_id"]?.ToString() ?? "",
                            TripId = data["trip_id"]?.ToString() ?? "",
                            StatusId = data["status_id"]?.ToString() ?? "",
                            FellowUserIsDriver = bool.TryParse(data["fellow_user_is_driver"]?.ToString(), out bool fid) ? fid : false,
                            TripUserIsDriver = bool.TryParse(data["trip_user_is_driver"]?.ToString(), out bool tuid) ? tuid : false,
                            UserCar = data["car_description"]?.ToString() ?? "",
                        };

                        result.Add(FellowTraveler);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить\n" + ex.Message, "OK");
                return new List<FellowTravelerModel>();
            }
        }
    }
}
