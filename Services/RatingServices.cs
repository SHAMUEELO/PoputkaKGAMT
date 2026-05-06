using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoputkaKGAMT.Services
{
    public class RatingServices
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");

        public async Task<List<TripRatingModel>> GetRatingDates()
        {
            try
            {
                var allRating = await firebase.Child("trip_reviews").OnceAsync<Dictionary<string, object>>();

                var result = new List<TripRatingModel>();
                foreach (var kvp in allRating)
                {
                    if (kvp.Object is IDictionary<string, object> data)
                    {

                        var trip = new TripRatingModel
                        {
                            Id = kvp.Key,
                            AppraiserUserId = data["appraiser_user_id"]?.ToString() ?? "",
                            RecipientUserId = data["recipient_user_id"]?.ToString() ?? "",
                            TripId = data["trip_id"]?.ToString() ?? "",
                            Estimate = int.TryParse(data["estimate"]?.ToString(), out int s) ? s : 0,
                            Review = data["review"]?.ToString() ?? "",
                            Created = data["createdAt"]?.ToString() ?? ""
                        };

                        result.Add(trip);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить\n" + ex.Message, "OK");
                return new List<TripRatingModel>();
            }
        }
    }
}
