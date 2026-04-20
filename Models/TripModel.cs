using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PoputkaKGAMT.Models
{
    public class TripModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = "";

        [JsonPropertyName("departure_id")]  
        public string DeparturePlaceId { get; set; } = "";

        [JsonPropertyName("arrive_id")] 
        public string ArrivePlaceId { get; set; } = "";

        [JsonPropertyName("status_id")]
        public string StatusId { get; set; } = "";

        [JsonPropertyName("is_driver")]
        public bool IsDriver { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = "";

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("seats_quentity")]
        public int SeatsQuentity { get; set; }

        [JsonPropertyName("original_seats_quentity")]
        public int OriginalSeatsQuentity { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("car_model")]
        public string? CarDescrtiption { get; set; } = null;

        [JsonPropertyName("max_back")]
        public bool MaxBack { get; set; }

        [JsonPropertyName("no_smoking")]
        public bool NoSmoking { get; set; }

        [JsonPropertyName("on_time")]
        public bool OnTime { get; set; }

        [JsonPropertyName("air_conditioing")]
        public bool AirConditioing { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";


        [JsonIgnore]
        public string UserName { get; set; } = "";

        [JsonIgnore]
        public string UserAvatar { get; set; } = "";

        [JsonIgnore]
        public double UserRating { get; set; }

        [JsonIgnore]
        public string DeparturePlaceName { get; set; } = "";

        [JsonIgnore]
        public string ArrivePlaceName { get; set; } = "";

        [JsonIgnore]
        public string Role { get; set; } = "";

        [JsonIgnore]
        public Color TripBackgroundColor
        {
            get
            {
                var currentUserId = Preferences.Get("CurrentUserKey", "");
                return UserId == currentUserId ? Color.FromArgb("#EFF4FF") : Colors.White;
            }
        }

        [JsonIgnore]
        public Color BorderStroke=> SeatsQuentity > 0 ? Color.FromArgb("#214484") : Color.FromArgb("#808080");

        [JsonIgnore]
        public bool SeatsLabelVisible => SeatsQuentity > 0;

        [JsonIgnore]
        public bool NoSeatsLabelVisible => SeatsQuentity == 0;

        [JsonIgnore]
        public string RoleText
        {
            get
            {
                if (SeatsQuentity == 0) { return "Места закончились"; }

                return IsDriver ? "Свободных мест" : "Требуется мест";
            }
        }

        [JsonIgnore]
        public Color BookButtonBackground=> SeatsQuentity > 0 ? Color.FromArgb("#214484") : Color.FromArgb("#808080");

        [JsonIgnore]
        public bool CanBookTrip=> SeatsQuentity > 0;

        [JsonIgnore]
        public Color BorderStrokeForHistory
        {
            get
            {
                return StatusId switch
                {
                    "1" => Color.FromArgb("#808080"),  // Серый
                    "2" => Color.FromArgb("#21842C"),  // Зеленый  
                    "3" => Color.FromArgb("#84216B"),  // Фиолетовый
                    _ => Color.FromArgb("#84216B")
                };
            }
        }

        [JsonIgnore]
        public string StatusTitle
        {
            get
            {
                return StatusId switch
                {
                    "1" => "Завершен",
                    "2" => "Активна",
                    "3" => "Запланирован",
                    _ => "Запланирован"
                };
            }
        }

        // Для TravelHistory
        [JsonIgnore]
        public bool UserStatus { get; set; }

       
    }
}
