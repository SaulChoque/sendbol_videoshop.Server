using sendbol_videoshop.Server.Models; // Importa los modelos definidos en la carpeta Models.
using Microsoft.Extensions.Options; // Proporciona acceso a configuraciones fuertemente tipadas.
using MongoDB.Driver; // Biblioteca para interactuar con MongoDB.

namespace sendbol_videoshop.Server.Services
{
    public class UsuariosService
    {
        // Colección de MongoDB que almacena documentos del tipo Usuario.
        private readonly IMongoCollection<Usuario> _usuariosCollection;

        // Constructor de la clase UsuariosService.
        // Recibe configuraciones de la base de datos a través de IOptions.
        public UsuariosService(
            IOptions<MongoVideoshopDatabaseSettings> VideoshopDatabaseSettings
            )
        {
            // Crea un cliente de MongoDB utilizando la cadena de conexión proporcionada.
            var mongoClient = new MongoClient(
                VideoshopDatabaseSettings.Value.ConnectionString);

            // Obtiene la base de datos especificada en las configuraciones.
            var mongoDatabase = mongoClient.GetDatabase(
                VideoshopDatabaseSettings.Value.DatabaseName);

            // Obtiene la colección de usuarios dentro de la base de datos.
            _usuariosCollection = mongoDatabase.GetCollection<Usuario>(
                VideoshopDatabaseSettings.Value.UsuariosCollectionName);
        }

        // Método para obtener todos los usuarios de la colección.
        public async Task<List<Usuario>> GetAsync() =>
            await _usuariosCollection.Find(_ => true).ToListAsync(); // Devuelve todos los documentos.

        // Método para obtener un usuario específico por su ID.
        public async Task<Usuario?> GetAsync(string id) =>
            await _usuariosCollection.Find(x => x.Id == id).FirstOrDefaultAsync(); // Busca el primer documento que coincida con el ID.        

        // Método para verificar si existe un usuario con el correo proporcionado.
        public async Task<bool> ExistsByCorreoAsync(string correo) =>
            await _usuariosCollection.Find(x => x.Correo == correo).AnyAsync();


        // Método para crear un nuevo usuario en la colección.
        public async Task CreateAsync(Usuario newUsuario) =>
            await _usuariosCollection.InsertOneAsync(newUsuario); // Inserta un nuevo documento.

        // Método para actualizar un usuario existente por su ID.
        public async Task UpdateAsync(string id, Usuario updatedUsuario) =>
            await _usuariosCollection.ReplaceOneAsync(x => x.Id == id, updatedUsuario); // Reemplaza el documento que coincida con el ID.

        // Método para eliminar un usuario de la colección por su ID.
        public async Task RemoveAsync(string id) =>
            await _usuariosCollection.DeleteOneAsync(x => x.Id == id); // Elimina el documento que coincida con el ID.

        // Método para obtener un usuario por correo y contraseña
        public async Task<Usuario?> GetByCorreoYPasswordAsync(string correo, string contrasena)
        {
            return await _usuariosCollection
                .Find(x => x.Correo == correo && x.Contrasena == contrasena)
                .FirstOrDefaultAsync();
        }

// ...existing code...


    }
}
