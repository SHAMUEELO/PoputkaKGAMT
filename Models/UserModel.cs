using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace PoputkaKGAMT.Models
{

    public class UserModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("profile_photo")]
        public string ProfilePhoto { get; set; } = "";

        [JsonPropertyName("isdriver")]
        public int Isdriver { get; set; }

        [JsonPropertyName("ispassenger")]
        public int Ispassenger { get; set; }

        [JsonPropertyName("rating_core")]
        public double Rating { get; set; } 

        [JsonPropertyName("createdAt")]
        public string Registration { get; set; } = "";

        [JsonPropertyName("one_star")]
        public int OneStar { get; set; }
        [JsonPropertyName("two_star")]
        public int TwoStar { get; set; }
        [JsonPropertyName("three_star")]
        public int ThreeStar { get; set; }
        [JsonPropertyName("four_star")]
        public int FourStar { get; set; }
        [JsonPropertyName("five_star")]
        public int FiveStar { get; set; }

        [JsonPropertyName("total_ratings")]
        private int TotalRatings;

        public double avgRating
        {
            get
            {
                int total = OneStar + TwoStar + ThreeStar + FourStar + FiveStar;
                return total > 0
                    ? Math.Round((1 * OneStar + 2 * TwoStar + 3 * ThreeStar + 4 * FourStar + 5 * FiveStar) / (double)total, 1)
                    : 0;
            }
        }
    }
}
    