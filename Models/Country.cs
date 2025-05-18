using System.Text.Json.Serialization;

namespace sendbol_videoshop.Server.Models
{
    public class Country
    {
        [JsonPropertyName("name")]
        public Name Name { get; set; } = new();

        [JsonIgnore]
        public string CommonName => Name.Common;

        [JsonIgnore]
        public string OfficialName => Name.Official;

        [JsonPropertyName("cca2")]
        public string CCA2 { get; set; } = string.Empty;

        [JsonPropertyName("cca3")]
        public string CCA3 { get; set; } = string.Empty;

        [JsonPropertyName("region")]
        public string Region { get; set; } = string.Empty;

        [JsonPropertyName("subregion")]
        public string? Subregion { get; set; } = string.Empty; // Ahora es opcional

        [JsonPropertyName("currencies")]
        public Dictionary<string, Currency> Currencies { get; set; } = new();

        [JsonIgnore]
        public string CurrencyName => Currencies.Values.FirstOrDefault()?.Name ?? string.Empty;

        [JsonIgnore]
        public string CurrencySymbol => Currencies.Values.FirstOrDefault()?.Symbol ?? string.Empty;

        [JsonPropertyName("languages")]
        public Dictionary<string, string> Languages { get; set; } = new();

        [JsonPropertyName("latlng")]
        public List<double> LatLng { get; set; } = new();

        [JsonPropertyName("area")]
        public double Area { get; set; }

        [JsonPropertyName("landlocked")]
        public bool Landlocked { get; set; }
    }

    public class Name
    {
        [JsonPropertyName("common")]
        public string Common { get; set; } = string.Empty;

        [JsonPropertyName("official")]
        public string Official { get; set; } = string.Empty;
    }

    public class Currency
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }
}
