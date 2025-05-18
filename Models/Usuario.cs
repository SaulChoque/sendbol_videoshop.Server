using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
namespace sendbol_videoshop.Server.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
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

        [BsonElement("pais")]
        [JsonPropertyName("pais")]
        public string Pais { get; set; }

        [BsonElement("fechaNacimiento")]
        [JsonPropertyName("fechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [BsonElement("esAdmin")]
        [JsonPropertyName("esAdmin")]
        public bool EsAdmin { get; set; } = false;

        [BsonElement("fechaCreacion")]
        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;






        // Constructor sin argumentos


        public Usuario()
        {
            Id = "";
            Nombre = "";
            Apellido = "";
            Correo = "";
            Contrasena = "";
            Telefono = "";
            Pais = "";
            EsAdmin = false;
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
            bool esAdmin,
            DateTime fechaCreacion
            )
        {
            Id = "0";
            Nombre = nombre;
            Apellido = apellido;
            Correo = correo;
            Contrasena = contrasena;
            Telefono = telefono;
            Pais = pais;
            FechaNacimiento = fechaNacimiento;
            EsAdmin = esAdmin;
            FechaCreacion = fechaCreacion;
        }


    }

    public class UsuarioLogin
    {
        public required string Correo { get; set; }
        public required string Contrasena { get; set; }
    }
}

