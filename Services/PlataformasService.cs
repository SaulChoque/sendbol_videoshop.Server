using sendbol_videoshop.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json;

namespace sendbol_videoshop.Server.Services
{
    public class PlataformasService
    {
        private readonly IMongoCollection<Plataforma> _plataformasCollection;
        private readonly IDatabase _redisDatabase;

        public PlataformasService(
            IOptions<MongoVideoshopDatabaseSettings> videoshopDatabaseSettings,
            IOptions<RedisVideoshopDatabaseSettings> redisVideoshopDatabaseSettings
        )
        {
            var mongoClient = new MongoClient(
                videoshopDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                videoshopDatabaseSettings.Value.DatabaseName);

            _plataformasCollection = mongoDatabase.GetCollection<Plataforma>(
                videoshopDatabaseSettings.Value.PlataformasCollectionName);

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                redisVideoshopDatabaseSettings.Value.ConnectionString
            );
            _redisDatabase = redis.GetDatabase();
        }

        /// <summary>
        /// Obtiene todas las plataformas usando Redis como caché.
        /// Si no existen en Redis, las clona desde MongoDB.
        /// </summary>
        public async Task<List<Plataforma>> GetAllAsync()
        {
            var plataformasRedis = await GetAllPlataformasFromRedisAsync();
            if (plataformasRedis.Count != 0)
                return plataformasRedis;

            await ClonarPlataformasMongoARedisAsync();
            plataformasRedis = await GetAllPlataformasFromRedisAsync();
            return plataformasRedis;
        }

        /// <summary>
        /// Clona todas las plataformas de MongoDB a Redis.
        /// </summary>
        public async Task ClonarPlataformasMongoARedisAsync()
        {
            var plataformas = await _plataformasCollection.Find(_ => true).ToListAsync();

            foreach (var plataforma in plataformas)
            {
                var key = $"plataformas:{plataforma.Id}";
                var dict = new Dictionary<string, string>
                {
                    { "Id", plataforma.Id },
                    { "Nombre", plataforma.Nombre },
                    { "Icon", plataforma.Icon }
                };

                var hashEntries = dict.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
                await _redisDatabase.HashSetAsync(key, hashEntries);
            }
        }

        /// <summary>
        /// Obtiene todas las plataformas desde Redis.
        /// </summary>
        public async Task<List<Plataforma>> GetAllPlataformasFromRedisAsync()
        {
            var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "plataformas:*");

            var plataformasList = new List<Plataforma>();
            foreach (var key in keys)
            {
                var hashEntries = await _redisDatabase.HashGetAllAsync(key);
                if (hashEntries.Length > 0)
                {
                    var dict = hashEntries.ToDictionary(
                        entry => entry.Name.ToString(),
                        entry => entry.Value.ToString()
                    );
                    var plataforma = new Plataforma
                    {
                        Id = dict.TryGetValue("Id", out string? idValue) ? idValue : "",
                        Nombre = dict.TryGetValue("Nombre", out string? tagValue) ? tagValue : "",
                        Icon = dict.TryGetValue("Icon", out string? iconValue) ? iconValue : ""
                    };

                    plataformasList.Add(plataforma);
                }
            }
            return plataformasList;
        }

        /// <summary>
        /// Obtiene una plataforma por id usando Redis.
        /// </summary>
        public async Task<Plataforma?> GetByIdAsync(string id)
        {
            var key = $"plataformas:{id}";
            var hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var plataforma = new Plataforma
                {
                    Id = dict.TryGetValue("Id", out string? idValue) ? idValue : "",
                    Nombre = dict.TryGetValue("Nombre", out string? tagValue) ? tagValue : "",
                    Icon = dict.TryGetValue("Icon", out string? iconValue) ? iconValue : ""
                };
                return plataforma;
            }


            /*
            // Si no está en Redis, intenta clonar y buscar de nuevo
            await ClonarPlataformasMongoARedisAsync();
            hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize<Plataforma>(json);
            }
            */
            return null;
        }

        /// <summary>
        /// Busca plataformas por nombre usando Redis.
        /// </summary>
        public async Task<List<Plataforma>> SearchByNombreAsync(string search)
        {
            var plataformasRedis = await GetAllPlataformasFromRedisAsync();
            return [.. plataformasRedis.Where(p => p.Nombre != null && p.Nombre.Contains(search, StringComparison.CurrentCultureIgnoreCase))];
        }
    }
}