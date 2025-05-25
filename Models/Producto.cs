using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace sendbol_videoshop.Server.Models
{
    public class Producto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("titulo")]
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; }

        [BsonElement("precio")]
        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        [BsonElement("cantidad")]
        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [BsonElement("descripcion")]
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("imagenes")]
        [JsonPropertyName("imagenes")]
        public List<string> Imagenes { get; set; }

        [BsonElement("categoria")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Categoria { get; set; }

        [BsonElement("plataformas")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Plataformas { get; set; }


        [BsonElement("stock")]
        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [BsonElement("fecha")]
        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        [BsonElement("rating")]
        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [BsonElement("likes")]
        [JsonPropertyName("likes")]
        public int Likes { get; set; }

        [BsonElement("dislikes")]
        [JsonPropertyName("dislikes")]
        public int Dislikes { get; set; }

        [BsonElement("etiquetas")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Etiquetas { get; set; }

        public Producto()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Titulo = "";
            Precio = 0;
            Cantidad = 0;
            Descripcion = "";
            Imagenes = new List<string>();
            Categoria = "";
            Plataformas = new List<string>();
            Stock = 0;
            Fecha = DateTime.MinValue;
            Rating = 0;
            Likes = 0;
            Dislikes = 0;
            Etiquetas = new List<string>();
        }
    }
}