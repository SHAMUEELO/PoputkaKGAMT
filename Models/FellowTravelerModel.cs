using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PoputkaKGAMT.Models
{
    public class FellowTravelerModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("trip_user_id")]
        public string TripUserId { get; set; } = "";

        [JsonPropertyName("fellow_user_id")]
        public string FellowUserId { get; set; } = "";

        [JsonPropertyName("trip_id")]
        public string TripId { get; set; } = "";

        [JsonPropertyName("status_id")]
        public string StatusId { get; set; } = "";

        [JsonPropertyName("fellow_user_is_driver")]
        public bool FellowUserIsDriver { get; set; }

        [JsonPropertyName("trip_user_is_driver")]
        public bool TripUserIsDriver { get; set; }

        [JsonPropertyName("car_description")]
        public string UserCar { get; set; } = "";

        [JsonIgnore]
        public string UserFellowName { get; set; } = "";

        [JsonIgnore]
        public string UserFellowAvatar { get; set; } = "";

        [JsonIgnore]
        public double UserFellowRating { get; set; }

        [JsonIgnore]
        public Color MyFellowBackgroundColor { get; set; }

        [JsonIgnore]
        public bool IsCurrentUser { get; set; }

        [JsonIgnore]
        public bool IsCurrentUserVisible { get; set; }

        [JsonIgnore]
        public bool IsCreatorVisible { get; set; }

        [JsonIgnore]
        public Color StatusColor
        {
            get
            {
                return StatusId == "6" ? Color.FromArgb("#21842C"): Color.FromArgb("#214484");
            }
        }

        [JsonIgnore]
        public string StatusTitle
        {
            get
            {
                return StatusId == "6" ? "Едет" : "Ожидает";
            }
        }

        [JsonIgnore]
        public bool CanAccept => StatusId != "6";

        
    }
}
