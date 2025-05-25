using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace sendbol_videoshop.Server.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("nombre")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [BsonElement("apellido")]
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; }

        [BsonElement("correo")]
        [JsonPropertyName("correo")]
        public string Correo { get; set; }

        [BsonElement("contrasena")]
        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; }

        [BsonElement("telefono")]
        [JsonPropertyName("telefono")]
        public string Telefono { get; set; }

        [BsonElement("fechaNacimiento")]
        [JsonPropertyName("fechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }


        [BsonElement("fechaCreacion")]
        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [BsonElement("pais")]
        [JsonPropertyName("pais")]
        public string Pais { get; set; }

        [BsonElement("prodInfo")]
        [JsonPropertyName("prodInfo")]
        public List<ProdInfoItem> ProdInfo { get; set; } = new List<ProdInfoItem>();

        // Constructor sin argumentos
        public Usuario()
        {
            Id = ObjectId.GenerateNewId().ToString();
            Nombre = "";
            Apellido = "";
            Correo = "";
            Contrasena = "";
            Telefono = "";
            Pais = "";
            FechaNacimiento = DateTime.MinValue;
            FechaCreacion = DateTime.Now;
            ProdInfo = new List<ProdInfoItem>();
        }

        // Constructor con argumentos
        public Usuario(
            string id,
            string nombre,
            string apellido,
            string correo,
            string contrasena,
            string telefono,
            string pais,
            DateTime fechaNacimiento,
            DateTime fechaCreacion,
            List<ProdInfoItem> prodInfo
        )
        {
            Id = id;
            Nombre = nombre;
            Apellido = apellido;
            Correo = correo;
            Contrasena = contrasena;
            Telefono = telefono;
            Pais = pais;
            FechaNacimiento = fechaNacimiento;
            FechaCreacion = fechaCreacion;
            ProdInfo = prodInfo ?? new List<ProdInfoItem>();
        }
    }

    public class ProdInfoItem
    {
        [BsonElement("idProd")]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("idProd")]
        public required string IdProd { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [BsonElement("ranking")]
        [JsonPropertyName("ranking")]
        public int Ranking { get; set; }
    }

    public class UsuarioLogin
    {
        public required string Correo { get; set; }
        public required string Contrasena { get; set; }
    }
}