using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace sendbol_videoshop.Server.Models
{
    public class Etiquetas
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("tag")]
        [JsonPropertyName("tag")]
        public string Tag { get; set; }


        public Etiquetas()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Tag = "";
        }
    }
}