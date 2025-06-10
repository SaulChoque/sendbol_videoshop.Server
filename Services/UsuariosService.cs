using sendbol_videoshop.Server.Models; // Importa los modelos definidos en la carpeta Models.
using Microsoft.Extensions.Options; // Proporciona acceso a configuraciones fuertemente tipadas.
using MongoDB.Driver; // Biblioteca para interactuar con MongoDB.
using BCrypt.Net; // Agrega esto al inicio del archivo

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
        public async Task CreateAsync(Usuario newUsuario)
        {
            // Hashea la contraseña antes de guardar
            newUsuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(newUsuario.Contrasena);
            await _usuariosCollection.InsertOneAsync(newUsuario);
        }


        // Método para actualizar un usuario existente por su ID.
        public async Task UpdateAsync(string id, Usuario updatedUsuario) =>
            await _usuariosCollection.ReplaceOneAsync(x => x.Id == id, updatedUsuario); // Reemplaza el documento que coincida con el ID.


        // Método para eliminar un usuario de la colección por su ID.
        public async Task RemoveAsync(string id) =>
            await _usuariosCollection.DeleteOneAsync(x => x.Id == id); // Elimina el documento que coincida con el ID.

        // Método para obtener un usuario por correo y contraseña (usando hash)
        public async Task<Usuario?> GetByCorreoYPasswordAsync(string correo, string contrasena)
        {
            // Busca el usuario por correo
            var usuario = await _usuariosCollection
                .Find(x => x.Correo == correo)
                .FirstOrDefaultAsync();
        
            // Si no existe el usuario o la contraseña no coincide, retorna null
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena))
                return null;
        
            return usuario;
        }

        // ...existing code...

        // Método para actualizar o agregar un like (ProdInfoItem) en la lista de un usuario
        public async Task<bool> UpdateProdInfoAsync(string usuarioId, ProdInfoItem prodInfoItem, string accion)
        {
            // Crea un filtro para buscar el usuario por su Id en la base de datos
            var filter = Builders<Usuario>.Filter.Eq(u => u.Id, usuarioId);
        
            // Busca el usuario en la colección usando el filtro
            var usuario = await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
        
            // Si no se encuentra el usuario, retorna false
            if (usuario == null)
                return false;
        
            // Busca el índice del producto en la lista ProdInfo del usuario
            var index = usuario.ProdInfo.FindIndex(p => p.IdProd == prodInfoItem.IdProd);
        
            if (index >= 0)
            {
                // Si el producto ya existe, actualiza según la acción recibida
                switch (accion.ToLower())
                {
                    case "like":
                        // Marca el producto como 'like' (Status = 1)
                        usuario.ProdInfo[index].Status = 1;
                        break;
                    case "dislike":
                        // Marca el producto como 'dislike' (Status = 0)
                        usuario.ProdInfo[index].Status = 0;
                        break;
                    case "rating":
                        // Actualiza el ranking del producto
                        usuario.ProdInfo[index].Ranking = prodInfoItem.Ranking;
                        break;
                    default:
                        // Si la acción no es reconocida, retorna false
                        return false;
                }
            }
            else
            {
                // Si el producto no existe en la lista, lo agrega según la acción
                switch (accion.ToLower())
                {
                    case "like":
                        // Asigna Status = 1 y agrega el producto
                        prodInfoItem.Status = 1;
                        usuario.ProdInfo.Add(prodInfoItem);
                        break;
                    case "dislike":
                        // Asigna Status = 0 y agrega el producto
                        prodInfoItem.Status = 0;
                        usuario.ProdInfo.Add(prodInfoItem);
                        break;
                    case "rating":
                        // Agrega el producto con el ranking recibido
                        usuario.ProdInfo.Add(prodInfoItem);
                        break;
                    default:
                        // Si la acción no es reconocida, retorna false
                        return false;
                }
            }
        
            // Prepara la actualización para guardar la lista modificada en la base de datos
            var update = Builders<Usuario>.Update.Set(u => u.ProdInfo, usuario.ProdInfo);
        
            // Ejecuta la actualización en la base de datos
            var result = await _usuariosCollection.UpdateOneAsync(filter, update);
        
            // Retorna true si se modificó algún documento, false en caso contrario
            return result.ModifiedCount > 0;
        }
        // ...existing code...



    }
}
