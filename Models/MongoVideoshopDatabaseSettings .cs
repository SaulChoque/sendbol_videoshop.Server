namespace sendbol_videoshop.Server.Models;

public class MongoVideoshopDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string UsuariosCollectionName { get; set; } = null!;
}

