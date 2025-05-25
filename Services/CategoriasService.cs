using sendbol_videoshop.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json;

namespace sendbol_videoshop.Server.Services
{
    public class CategoriasService
    {
        private readonly IMongoCollection<Categoria> _categoriasCollection;
        private readonly IDatabase _redisDatabase;
        private bool _redisExists;

        public CategoriasService(
            IOptions<MongoVideoshopDatabaseSettings> videoshopDatabaseSettings,
            IOptions<RedisVideoshopDatabaseSettings> redisVideoshopDatabaseSettings
        )
        {


            var mongoClient = new MongoClient(
                videoshopDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                videoshopDatabaseSettings.Value.DatabaseName);

            _categoriasCollection = mongoDatabase.GetCollection<Categoria>(
                videoshopDatabaseSettings.Value.CategoriasCollectionName);

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                redisVideoshopDatabaseSettings.Value.ConnectionString
            );
            _redisDatabase = redis.GetDatabase();
        }




        /// <summary>
        /// Obtiene todas las categorías usando Redis como caché.
        /// Si no existen en Redis, las clona desde MongoDB.
        /// </summary>
        public async Task<List<Categoria>> GetAllAsync()
        {
            var categoriasRedis = await GetAllCategoriasFromRedisAsync();
            if (categoriasRedis.Count != 0)
                return categoriasRedis;

            await ClonarCategoriasMongoARedisAsync();
            categoriasRedis = await GetAllCategoriasFromRedisAsync();
            return categoriasRedis;
        }




        /// <summary>
        /// Clona todas las categorías de MongoDB a Redis.
        /// </summary>
        public async Task ClonarCategoriasMongoARedisAsync()
        {
            var categorias = await _categoriasCollection.Find(_ => true).ToListAsync();

            foreach (var categoria in categorias)
            {
                var key = $"categorias:{categoria.Id}";
                var dict = new Dictionary<string, string>
                {
                    { "Id", categoria.Id },
                    { "Titulo", categoria.Titulo }, // Asegúrate que el modelo usa "Titulo"
                    { "Chiptags", JsonSerializer.Serialize(categoria.Chiptags) } // Usa "Chiptags" igual que en el modelo
                };

                var hashEntries = dict.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
                await _redisDatabase.HashSetAsync(key, hashEntries);
            }
            _redisExists = true;
        }

        /// <summary>
        /// Obtiene todas las categorías desde Redis.
        /// </summary>
        public async Task<List<Categoria>> GetAllCategoriasFromRedisAsync()
        {
            var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "categorias:*");

            var categorias = new List<Categoria>();
            // ...existing code...
            foreach (var key in keys)
            {
                var hashEntries = await _redisDatabase.HashGetAllAsync(key);
                if (hashEntries.Length > 0)
                {
                    var dict = hashEntries.ToDictionary(
                        entry => entry.Name.ToString(),
                        entry => entry.Value.ToString()
                    );
            
                    var categoria = new Categoria
                    {
                        Id = dict.TryGetValue("Id", out string? idValue) ? idValue : "",
                        Titulo = dict.TryGetValue("Titulo", out string? tituloValue) ? tituloValue : "",
                        Chiptags = dict.TryGetValue("Chiptags", out string? chiptagsValue) && !string.IsNullOrEmpty(chiptagsValue)
                            ? JsonSerializer.Deserialize<List<string>>(chiptagsValue) ?? []
                            : []
                    };
            
                    categorias.Add(categoria);
                }
            }
            // ...existing code...
            return categorias;
        }

        /// <summary>
        /// Obtiene una categoría por id usando Redis.
        /// </summary>
        public async Task<Categoria?> GetByIdAsync(string id)
        {
            var key = $"categorias:{id}";
            var hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var categoria = new Categoria
                {
                    Id = dict.TryGetValue("Id", out string? idValue) ? idValue : "",
                    Titulo = dict.TryGetValue("Titulo", out string? tituloValue) ? tituloValue : "",
                    Chiptags = dict.TryGetValue("Chiptags", out string? chiptagsValue) && !string.IsNullOrEmpty(chiptagsValue)
                        ? JsonSerializer.Deserialize<List<string>>(chiptagsValue) ?? []
                        : []
                };
                return categoria;
            }


            /*
            // Si no está en Redis, intenta clonar y buscar de nuevo
            await ClonarCategoriasMongoARedisAsync();
            hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize<Categoria>(json);
            }
            */
            return null;
        }

        /// <summary>
        /// Busca categorías por título usando Redis.
        /// </summary>
        public async Task<List<Categoria>> SearchByTituloAsync(string search)
        {
            var categoriasRedis = await GetAllAsync();
            return [.. categoriasRedis.Where(c => c.Titulo != null && c.Titulo.Contains(search, StringComparison.CurrentCultureIgnoreCase))];
        }
    }
}