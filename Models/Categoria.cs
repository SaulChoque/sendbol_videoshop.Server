using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace sendbol_videoshop.Server.Models
{
    public class Categoria
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("titulo")]
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; }


        [BsonElement("chiptags")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Chiptags { get; set; }

        public Categoria()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Titulo = "";
            Chiptags = new List<string>();
        }
    }
}