using sendbol_videoshop.Server.Models; // Importa los modelos definidos en la carpeta Models
using Microsoft.Extensions.Options;
using NRedisStack;
using StackExchange.Redis;

namespace sendbol_videoshop.Server.Services
{
    public class CountriesService
    {
        private readonly IDatabase _redisDatabase;

        // Constructor de la clase CountriesService
        public CountriesService(
            IOptions<RedisVideoshopDatabaseSettings> redisVideoshopDatabaseSettings
        )
        {
            // Crea una conexión a Redis utilizando la cadena de conexión proporcionada
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                redisVideoshopDatabaseSettings.Value.ConnectionString
            );

            // Obtiene la base de datos Redis
            _redisDatabase = redis.GetDatabase();
        }

        // Método para agregar un país al hash utilizando el modelo Country
        public async Task AddCountryAsync(Country country)
        {
            // Clave del hash para el país
            var hashKey = $"country:{country.CCA2}";

            // Crear el hash en Redis
            await _redisDatabase.HashSetAsync(hashKey, new HashEntry[]
            {
                new HashEntry("commonName", country.CommonName),
                new HashEntry("officialName", country.OfficialName),
                new HashEntry("currency", country.CurrencyName),
                new HashEntry("currencySymbol", country.CurrencySymbol),
                new HashEntry("language", string.Join(", ", country.Languages.Values)),
                new HashEntry("region", country.Region),
                new HashEntry("subregion", country.Subregion),

            });
        }

        // Método para obtener los datos de un país
        public async Task<Dictionary<string, string>> GetCountryAsync(string countryCode)
        {
            var hashKey = $"country:{countryCode}";
            var hashEntries = await _redisDatabase.HashGetAllAsync(hashKey);

            if (hashEntries.Length == 0)
                return null; // Si no existe el país, retorna null

            return hashEntries.ToDictionary(
                entry => entry.Name.ToString(),
                entry => entry.Value.ToString()
            );
        }

        // Método para listar todos los países (solo nombres)
        public async Task<List<string>> GetAllCountriesAsync()
        {
            var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "country:*");

            var countryNames = new List<string>();
            foreach (var key in keys)
            {
                var name = await _redisDatabase.HashGetAsync(key, "commonName");
                if (!name.IsNullOrEmpty)
                {
                    countryNames.Add(name.ToString());
                }
            }

            return countryNames;
        }
    }
}
