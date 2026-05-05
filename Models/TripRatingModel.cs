using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PoputkaKGAMT.Models
{
    public class TripRatingModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("trip_id")]
        public string TripId { get; set; } = "";

        [JsonPropertyName("appraiser_user_id")]
        public string AppraiserUserId { get; set; } = "";

        [JsonPropertyName("recipient_user_id")]
        public string RecipientUserId { get; set; } = "";

        [JsonPropertyName("estimate")]
        public int Estimate { get; set; }

        [JsonPropertyName("review")]
        public string Review { get; set; } = "";

        [JsonPropertyName("createdAt")]
        public string Created { get; set; } = "";

        [JsonIgnore]
        public string UserName { get; set; } = "";

        [JsonIgnore]
        public double UserRating { get; set; }

        [JsonIgnore]
        public string EstimateTitle
        {
            get
            {
                return Estimate switch
                {
                    1 => "Не понравилось",
                    2 => "Не понравилось",
                    3 => "Удовлетворительно",
                    4 => "Понравилась",
                    5 => "Отлично",
                    _ => "Запланирован"
                };
            }
        }

        [JsonIgnore]
        public bool UserHaveReview { get; set; }

        [JsonIgnore]
        public bool CanReview => !UserHaveReview; // Можно ли оставить отзыв

        [JsonIgnore]
        public bool IsUserHaveReviewForShow => !string.IsNullOrEmpty(Review); //  Есть отзыв для отображения
    }
}
