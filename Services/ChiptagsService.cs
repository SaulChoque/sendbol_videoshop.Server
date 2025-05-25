using sendbol_videoshop.Server.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json;

namespace sendbol_videoshop.Server.Services
{
    public class ChiptagsService
    {
        private readonly IMongoCollection<Chiptags> _chiptagsCollection;
        private readonly IDatabase _redisDatabase;

        public ChiptagsService(
            IOptions<MongoVideoshopDatabaseSettings> videoshopDatabaseSettings,
            IOptions<RedisVideoshopDatabaseSettings> redisVideoshopDatabaseSettings
        )
        {
            var mongoClient = new MongoClient(
                videoshopDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                videoshopDatabaseSettings.Value.DatabaseName);

            _chiptagsCollection = mongoDatabase.GetCollection<Chiptags>(
                videoshopDatabaseSettings.Value.ChiptagsCollectionName);

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                redisVideoshopDatabaseSettings.Value.ConnectionString
            );
            _redisDatabase = redis.GetDatabase();
        }

        /// <summary>
        /// Obtiene todos los chiptags usando Redis como caché.
        /// Si no existen en Redis, los clona desde MongoDB.
        /// </summary>
        public async Task<List<Chiptags>> GetAllAsync()
        {
            var chiptagsRedis = await GetAllChiptagsFromRedisAsync();
            if (chiptagsRedis.Count != 0)
                return chiptagsRedis;

            await ClonarChiptagsMongoARedisAsync();
            chiptagsRedis = await GetAllChiptagsFromRedisAsync();
            return chiptagsRedis;
        }

        /// <summary>
        /// Clona todos los chiptags de MongoDB a Redis.
        /// </summary>
        public async Task ClonarChiptagsMongoARedisAsync()
        {
            var chiptags = await _chiptagsCollection.Find(_ => true).ToListAsync();

            foreach (var chiptag in chiptags)
            {
                var key = $"chiptags:{chiptag.Id}";
                var dict = new Dictionary<string, string>
                {
                    { "Id", chiptag.Id },
                    { "Tag", chiptag.Tag }
                };

                var hashEntries = dict.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
                await _redisDatabase.HashSetAsync(key, hashEntries);
            }
        }

        /// <summary>
        /// Obtiene todos los chiptags desde Redis.
        /// </summary>
        public async Task<List<Chiptags>> GetAllChiptagsFromRedisAsync()
        {
            var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "chiptags:*");

            var chiptagsList = new List<Chiptags>();
            foreach (var key in keys)
            {
                var hashEntries = await _redisDatabase.HashGetAllAsync(key);
                if (hashEntries.Length > 0)
                {
                    var dict = hashEntries.ToDictionary(
                        entry => entry.Name.ToString(),
                        entry => entry.Value.ToString()
                    );
                    var chiptag = new Chiptags
                    {
                        Id = dict.TryGetValue("Id", out string? idValue) ? idValue : "",
                        Tag = dict.TryGetValue("Tag", out string? tagValue) ? tagValue : ""
                    };

                    chiptagsList.Add(chiptag);
                }
            }
            return chiptagsList;
        }

        /// <summary>
        /// Obtiene un chiptags por id usando Redis.
        /// </summary>
        public async Task<Chiptags?> GetByIdAsync(string id)
        {
            var key = $"chiptags:{id}";
            var hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize<Chiptags>(json);
            }
            /*
            // Si no está en Redis, intenta clonar y buscar de nuevo
            await ClonarChiptagsMongoARedisAsync();
            hashEntries = await _redisDatabase.HashGetAllAsync(key);
            if (hashEntries.Length > 0)
            {
                var dict = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize<Chiptags>(json);
            }
            */
            return null;
        }

        /// <summary>
        /// Busca chiptags por tag usando Redis.
        /// </summary>
        public async Task<List<Chiptags>> SearchByTagAsync(string search)
        {
            var chiptagsRedis = await GetAllChiptagsFromRedisAsync();
            return [.. chiptagsRedis.Where(c => c.Tag != null && c.Tag.Contains(search, StringComparison.CurrentCultureIgnoreCase))];
        }
    }
}