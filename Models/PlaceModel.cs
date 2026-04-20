using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PoputkaKGAMT.Models
{
    public class PlaceModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
