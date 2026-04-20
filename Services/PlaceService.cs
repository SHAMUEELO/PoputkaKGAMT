using Firebase.Database;
using PoputkaKGAMT.Models;

namespace PoputkaKGAMT.Services
{
    public class PlaceService
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        public async Task<List<PlaceModel>> GetPlaces() 
        {   
            try
            {
                var allTrips = await firebase.Child("places").OnceAsync<PlaceModel>();

                return allTrips.Select(kvp => kvp.Object).ToList();

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить\n" + ex.Message, "OK");
                return new List<PlaceModel>();
            }

        }

    }
}
