using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace sendbol_videoshop.Server.Models
{
    public class Plataforma
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("nombre")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [BsonElement("icon")]
        [JsonPropertyName("icon")]
        public string Icon { get; set; }


        public Plataforma()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Nombre = "";
            Icon = "";
        }
    }
}