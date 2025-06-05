using sendbol_videoshop.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json;

namespace sendbol_videoshop.Server.Services
{
    public class EtiquetasService
    {
        private readonly IMongoCollection<Etiquetas> _etiquetasCollection;
        private readonly IDatabase _redisDatabase;

        public EtiquetasService(
            IOptions<MongoVideoshopDatabaseSettings> videoshopDatabaseSettings,
            IOptions<RedisVideoshopDatabaseSettings> redisVideoshopDatabaseSettings
        )
        {
            var mongoClient = new MongoClient(
                videoshopDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                videoshopDatabaseSettings.Value.DatabaseName);

            _etiquetasCollection = mongoDatabase.GetCollection<Etiquetas>(
                videoshopDatabaseSettings.Value.EtiquetasCollectionName);

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                redisVideoshopDatabaseSettings.Value.ConnectionString
            );
            _redisDatabase = redis.GetDatabase();
        }

        /// <summary>
        /// Obtiene todos los etiquetas usando Redis como caché.
        /// Si no existen en Redis, los clona desde MongoDB.
        /// </summary>
        public async Task<List<Etiquetas>> GetAllAsync()
        {
            var etiquetasRedis = await GetAllEtiquetasFromRedisAsync();
            if (etiquetasRedis.Count != 0)
                return etiquetasRedis;

            await ClonarEtiquetasMongoARedisAsync();
            etiquetasRedis = await GetAllEtiquetasFromRedisAsync();
            return etiquetasRedis;
        }

        /// <summary>
        /// Clona todos los etiquetas de MongoDB a Redis.
        /// </summary>
        public async Task ClonarEtiquetasMongoARedisAsync()
        {
            var etiquetas = await _etiquetasCollection.Find(_ => true).ToListAsync();

            foreach (var etiqueta in etiquetas)
            {
                var key = $"etiquetas:{etiqueta.Id}";
                var dict = new Dictionary<string, string>
                {
                    { "Id", etiqueta.Id },
                    { "Tag", etiqueta.Tag }
                };

                var hashEntries = dict.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
                await _redisDatabase.HashSetAsync(key, hashEntries);
            }
        }

        /// <summary>
        /// Obtiene todos los etiquetas desde Redis.
        /// </summary>
        public async Task<List<Etiquetas>> GetAllEtiquetasFromRedisAsync()
        {
            var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "etiquetas:*");

            var etiquetasList = new List<Etiquetas>();
            foreach (var key in keys)
            {
                var hashEntries = await _redisDatabase.HashGetAllAsync(key);
                if (hashEntries.Length > 0)
                {
                    var dict = hashEntries.ToDictionary(
                        entry => entry.Name.ToString(),
                        entry => entry.Value.ToString()
                    );
                    var etiqueta = new Etiquetas
                    {
                        Id = dict.TryGetValue("Id", out string? idValue) ? idValue : "",
                        Tag = dict.TryGetValue("Tag", out string? tagValue) ? tagValue : ""
                    };

                    etiquetasList.Add(etiqueta);
                }
            }
            return etiquetasList;
        }

        /// <summary>
        /// Obtiene un etiquetas por id usando Redis.
        /// </summary>
        public async Task<Etiquetas?> GetByIdAsync(string id)
        {
            var key = $"etiquetas:{id}";
            var hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize<Etiquetas>(json);
            }
            /*
            // Si no está en Redis, intenta clonar y buscar de nuevo
            await ClonarEtiquetasMongoARedisAsync();
            hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize<Etiquetas>(json);
            }
            */
            return null;
        }

        /// <summary>
        /// Busca etiquetas por tag usando Redis.
        /// </summary>
        public async Task<List<Etiquetas>> SearchByTagAsync(string search)
        {
            var etiquetasRedis = await GetAllEtiquetasFromRedisAsync();
            return [.. etiquetasRedis.Where(c => c.Tag != null && c.Tag.Contains(search, StringComparison.CurrentCultureIgnoreCase))];
        }
    }
}