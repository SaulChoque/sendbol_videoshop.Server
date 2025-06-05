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


        [BsonElement("etiquetas")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Etiquetas { get; set; }

        public Categoria()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Titulo = "";
            Etiquetas = new List<string>();
        }
    }
}