using sendbol_videoshop.Server.Models; // Importa los modelos definidos en la carpeta Models.
using Microsoft.Extensions.Options; // Proporciona acceso a configuraciones fuertemente tipadas.
using MongoDB.Driver; // Biblioteca para interactuar con MongoDB.
using StackExchange.Redis;
using System.Text.Json; // Agrega esta línea para usar Redis


namespace sendbol_videoshop.Server.Services
{
    public class ProductosService
    {
        // Colección de MongoDB que almacena documentos del tipo Usuario.
        private readonly IMongoCollection<Producto> _productosCollection;
        private readonly IDatabase _redisDatabase;

        // Constructor de la clase ProductosService.
        // Recibe configuraciones de la base de datos a través de IOptions.
        public ProductosService(
            IOptions<MongoVideoshopDatabaseSettings> VideoshopDatabaseSettings,
            IOptions<RedisVideoshopDatabaseSettings> redisVideoshopDatabaseSettings
            )
        {
            // Crea un cliente de MongoDB utilizando la cadena de conexión proporcionada.
            var mongoClient = new MongoClient(
                VideoshopDatabaseSettings.Value.ConnectionString);

            // Obtiene la base de datos especificada en las configuraciones.
            var mongoDatabase = mongoClient.GetDatabase(
                VideoshopDatabaseSettings.Value.DatabaseName);

            // Crea una conexión a Redis utilizando la cadena de conexión proporcionada
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                redisVideoshopDatabaseSettings.Value.ConnectionString
            );

            // Obtiene la colección de Productos dentro de la base de datos.
            _productosCollection = mongoDatabase.GetCollection<Producto>(
                VideoshopDatabaseSettings.Value.ProductosCollectionName);

            // Obtiene la base de datos Redis
            _redisDatabase = redis.GetDatabase();

        }




    
        /// <summary>
        /// Método para obtener todos los productos de la colección.
        /// Si no existen en Redis, los obtiene de MongoDB y los guarda en Redis.
        /// </summary>
        /// <returns>
        /// Lista de productos obtenidos, ya sea de Redis o MongoDB.
        /// </returns>
        public async Task<List<Producto>> GetAllAsync() =>

            await GetOrFetchAllProductsAsync();

        /// <summary>
        /// Método para obtener todos los productos de la colección.
        /// Si no existen en Redis, los obtiene de MongoDB y los guarda en Redis.
        /// </summary>
        /// <returns>
        /// Lista de productos obtenidos, ya sea de Redis o MongoDB.
        /// </returns>
        public async Task<List<Producto>> GetOrFetchAllProductsAsync()
        {
            var cachedCountries = await GetAllProductsFromRedisAsync();

            if (cachedCountries.Any())
            {
                return cachedCountries.ToList();
            }

            // Si no existen datos, llamar a mongoDB
            await ClonarProductosMongoARedisAsync();
            return _productosCollection.Find(_ => true).ToList();

        }
        
        /// <summary>
        /// Clona todos los productos de MongoDB a Redis.
        /// </summary>
        /// <returns>
        /// Tarea que representa la operación asincrónica.
        /// </returns>
        public async Task ClonarProductosMongoARedisAsync()
        {
        // Obtiene todos los productos de MongoDB
        var productos = await _productosCollection.Find(_ => true).ToListAsync();
    
        // Por cada producto, crea un hash en Redis con la clave "productos:{id}"
        foreach (var producto in productos)
        {
            var key = $"productos:{producto.Id}";
    
            // Convierte el producto a un diccionario de campos
            var dict = new Dictionary<string, string>
            {
                { "Id", producto.Id },
                { "Titulo", producto.Titulo },
                { "Precio", producto.Precio.ToString() },
                { "Cantidad", producto.Cantidad.ToString() },
                { "Descripcion", producto.Descripcion },
                { "Stock", producto.Stock.ToString() },
                { "Fecha", producto.Fecha.ToString("o") },
                { "Rating", producto.Rating.ToString() },
                { "Likes", producto.Likes.ToString() },
                { "Dislikes", producto.Dislikes.ToString() },
                // Serializa listas y ObjectId a JSON para almacenarlas como string
                { "Imagenes", JsonSerializer.Serialize(producto.Imagenes) },
                { "Categoria", producto.Categoria.ToString() },
                { "Plataformas", JsonSerializer.Serialize(producto.Plataformas) },
                { "Etiquetas", JsonSerializer.Serialize(producto.Etiquetas) }
            };
    
            // Convierte el diccionario a HashEntry[]
            var hashEntries = dict.Select(kv => new StackExchange.Redis.HashEntry(kv.Key, kv.Value)).ToArray();
    
            // Guarda el hash en Redis
            await _redisDatabase.HashSetAsync(key, hashEntries);
        }
    }
             
        public async Task<List<Producto>> GetAllProductsFromRedisAsync()
        {
            var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "productos:*");

            var productos = new List<Producto>();
            foreach (var key in keys)
            {
                // Obtiene todos los campos del hash del producto
                var hashEntries = await _redisDatabase.HashGetAllAsync(key);
                if (hashEntries.Length > 0)
                {
                    // Convierte el hash a un diccionario
                    var dict = hashEntries.ToDictionary(
                        entry => entry.Name.ToString(),
                        entry => entry.Value.ToString()
                    );

                    var producto = new Producto
                    {
                        Id = dict.TryGetValue("Id", out string? idValue) ? idValue : "",
                        Titulo = dict.TryGetValue("Titulo", out string? tituloValue) ? tituloValue : "",
                        Precio = dict.TryGetValue("Precio", out string? precioStr) && decimal.TryParse(precioStr, out decimal precioValue) ? precioValue : 0,
                        Cantidad = dict.TryGetValue("Cantidad", out string? cantidadStr) && int.TryParse(cantidadStr, out int cantidadValue) ? cantidadValue : 0,
                        Descripcion = dict.TryGetValue("Descripcion", out string? descripcionValue) ? descripcionValue : "",
                        Imagenes = dict.TryGetValue("Imagenes", out string? imagenesValue) && !string.IsNullOrEmpty(imagenesValue)
                            ? JsonSerializer.Deserialize<List<string>>(imagenesValue) ?? new List<string>()
                            : new List<string>(),
                        Categoria = dict.TryGetValue("Categoria", out string? categoriaValue) ? categoriaValue : "",
                        Plataformas = dict.TryGetValue("Plataformas", out string? plataformasValue) && !string.IsNullOrEmpty(plataformasValue)
                            ? JsonSerializer.Deserialize<List<string>>(plataformasValue) ?? []
                            : [],
                        Stock = dict.TryGetValue("Stock", out string? stockStr) && int.TryParse(stockStr, out int stockValue) ? stockValue : 0,
                        Fecha = dict.TryGetValue("Fecha", out string? fechaStr) && DateTime.TryParse(fechaStr, out DateTime fechaValue) ? fechaValue : DateTime.MinValue,
                        Rating = dict.TryGetValue("Rating", out string? ratingStr) && int.TryParse(ratingStr, out int ratingValue) ? ratingValue : 0,
                        Likes = dict.TryGetValue("Likes", out string? likesStr) && int.TryParse(likesStr, out int likesValue) ? likesValue : 0,
                        Dislikes = dict.TryGetValue("Dislikes", out string? dislikesStr) && int.TryParse(dislikesStr, out int dislikesValue) ? dislikesValue : 0,
                        Etiquetas = dict.TryGetValue("Etiquetas", out string? etiquetasValue) && !string.IsNullOrEmpty(etiquetasValue)
                            ? JsonSerializer.Deserialize<List<string>>(etiquetasValue) ?? []
                            : []
                    };
            
                    productos.Add(producto);
                }
            }

            return productos;
        }







        /// <summary>
        /// Busca coincidencias de una cadena en el título usando Redis.
        /// </summary>
        public async Task<List<Producto>> SearchByTituloAsync(string search)
        {
            var productosRedis = await GetAllProductsFromRedisAsync();
            return [.. productosRedis.Where(p => p.Titulo != null && p.Titulo.ToLower().Contains(search.ToLower()))];
        }

        /// <summary>
        /// Obtiene productos por un array de ids usando Redis.
        /// </summary>
        public async Task<List<Producto>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var productosRedis = await GetAllProductsFromRedisAsync();
            return [.. productosRedis.Where(p => ids.Contains(p.Id))];
        }
        
        /// <summary>
        /// Obtiene productos por categoría usando Redis.
        /// </summary>
        public async Task<List<Producto>> GetByCategoriaAsync(string categoria)
        {
            var productosRedis = await GetAllProductsFromRedisAsync();
            return [.. productosRedis.Where(p => p.Categoria == categoria)];
        }

        /// <summary>
        /// Obtiene productos por plataforma usando Redis.
        /// </summary>
        public async Task<List<Producto>> GetByPlataformaAsync(string plataforma)
        {
            var productosRedis = await GetAllProductsFromRedisAsync();
            return [.. productosRedis.Where(p => p.Plataformas != null && p.Plataformas.Contains(plataforma))];
        }
        

        /// <summary>
        /// Obtiene productos por rango de precio usando Redis.
        /// </summary>
        public async Task<List<Producto>> GetByRangoPrecioAsync(decimal min, decimal max)
        {
            var productosRedis = await GetAllProductsFromRedisAsync();
            return [.. productosRedis.Where(p => p.Precio >= min && p.Precio <= max)];
        }

        
        /// <summary>
        /// Obtiene un producto por id usando Redis.
        /// </summary>
        public async Task<Producto> GetByIdAsync(string id)
        {
            var productosRedis = await GetAllProductsFromRedisAsync();
            return productosRedis.FirstOrDefault(p => p.Id == id)!;
        }



        // 8. Actualizar likes y dislikes (mongodb)
        public async Task UpdateLikesDislikesAsync(string id, int likes, int dislikes)
        {
            var update = Builders<Producto>.Update
                .Set(p => p.Likes, likes)
                .Set(p => p.Dislikes, dislikes);

            await _productosCollection.UpdateOneAsync(p => p.Id == id, update);
        }


        // 8.1 Actualizar likes y dislikes (redis)
        public async Task UpdateLikesDislikesRedisAsync(string id, int likes, int dislikes)
        {

            await SincronizarLikesDislikesRedisAsync(); // Asegura que Redis tenga los datos
            // ZSET para likes
            await _redisDatabase.SortedSetAddAsync("productosMetricas:likes", id, likes);
            // ZSET para dislikes (opcional, si quieres también guardar dislikes)
            await _redisDatabase.SortedSetAddAsync("productosMetricas:dislikes", id, dislikes);
        }

        // 9. Actualizar rating
        public async Task UpdateRatingAsync(string id, int rating)
        {
            await SincronizarRatingRedisAsync(); // Asegura que Redis tenga los datos   
            // Actualiza el ZSET de Redis para el rating
            await _redisDatabase.SortedSetAddAsync("productosMetricas:ranking", id, rating);
        }



        // ...existing code...

        /// <summary>
        /// Verifica si existen los datos de likes y dislikes en Redis.
        /// Si no existen, los extrae de MongoDB y los copia a Redis.
        /// </summary>
        public async Task SincronizarLikesDislikesRedisAsync()
        {
            // Consulta la cantidad de elementos en los ZSET de likes y dislikes en Redis
            long likesCount = await _redisDatabase.SortedSetLengthAsync("productosMetricas:likes");
            long dislikesCount = await _redisDatabase.SortedSetLengthAsync("productosMetricas:dislikes");

            // Si ambos ZSET ya tienen datos, no es necesario sincronizar
            if (likesCount > 0 && dislikesCount > 0)
                return;

            // Si no existen datos, obtiene todos los productos desde MongoDB
            var productos = await _productosCollection.Find(_ => true).ToListAsync();

            // Crea un batch para agrupar las operaciones en Redis y hacerlas más eficientes
            var batch = _redisDatabase.CreateBatch();
            var tasks = new List<Task>();

            // Por cada producto, agrega una tarea para insertar el score de likes y dislikes en los ZSET de Redis
            foreach (var producto in productos)
            {
                // Agrega el ID del producto y su cantidad de likes al ZSET de likes
                tasks.Add(batch.SortedSetAddAsync("productosMetricas:likes", producto.Id, producto.Likes));
                // Agrega el ID del producto y su cantidad de dislikes al ZSET de dislikes
                tasks.Add(batch.SortedSetAddAsync("productosMetricas:dislikes", producto.Id, producto.Dislikes));
            }

            // Ejecuta el batch de operaciones en Redis
            batch.Execute();
            // Espera a que todas las tareas del batch finalicen antes de continuar
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Verifica si existen los datos de rating en Redis.
        /// Si no existen, los extrae de MongoDB y los copia a Redis.
        /// </summary>
        public async Task SincronizarRatingRedisAsync()
        {
            // Consulta la cantidad de elementos en el ZSET de rating en Redis
            long ratingCount = await _redisDatabase.SortedSetLengthAsync("productosMetricas:ranking");

            // Si el ZSET ya tiene datos, no es necesario sincronizar
            if (ratingCount > 0)
                return;

            // Si no existen datos, obtiene todos los productos desde MongoDB
            var productos = await _productosCollection.Find(_ => true).ToListAsync();

            // Crea un batch para agrupar las operaciones en Redis y hacerlas más eficientes
            var batch = _redisDatabase.CreateBatch();
            var tasks = new List<Task>();

            // Por cada producto, agrega una tarea para insertar el score de rating en el ZSET de Redis
            foreach (var producto in productos)
            {
                // Agrega el ID del producto y su rating al ZSET de ranking
                tasks.Add(batch.SortedSetAddAsync("productosMetricas:ranking", producto.Id, producto.Rating));
            }

            // Ejecuta el batch de operaciones en Redis
            batch.Execute();
            // Espera a que todas las tareas del batch finalicen antes de continuar
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Obtiene una lista de productos ordenados de mayor a menor rating.
        /// </summary>
        /// <param name="cantidad">Cantidad máxima de productos a devolver.</param>
        /// <returns>Lista de productos ordenados por rating descendente.</returns>
        public async Task<List<Producto>> GetProductosPorRatingDescAsync(int cantidad)
        {

            await SincronizarRatingRedisAsync(); // Asegura que Redis tenga los datos
            // Obtiene los IDs de los productos ordenados por rating descendente desde Redis
            var ids = await _redisDatabase.SortedSetRangeByScoreAsync(
                "productosMetricas:ranking",
                order: Order.Descending,
                take: cantidad
            );

            // Convierte los IDs a string
            var idList = ids.Select(id => id.ToString()).ToList();

            // Obtiene los productos desde MongoDB usando los IDs
            var productos = await _productosCollection.Find(p => idList.Contains(p.Id)).ToListAsync();

            // Ordena los productos según el orden de los IDs obtenidos de Redis
            var productosOrdenados = idList
                .Select(id => productos.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .Select(p => p!)
                .ToList();

            return productosOrdenados;
        }

        /// <summary>
        /// Obtiene una lista de productos ordenados de mayor a menor por la diferencia de likes menos dislikes.
        /// </summary>
        /// <param name="cantidad">Cantidad máxima de productos a devolver.</param>
        /// <returns>Lista de productos ordenados por (likes - dislikes) descendente.</returns>
        public async Task<List<Producto>> GetProductosPorLikesDescAsync(int cantidad)
        {
            
            await SincronizarLikesDislikesRedisAsync(); // Asegura que Redis tenga los datos
   
            // Obtiene los IDs de los productos ordenados por likes descendente desde Redis
            // Primero sincroniza un ZSET temporal con la diferencia likes - dislikes
            // NOTA: Redis no soporta operaciones aritméticas directas en ZSET, así que lo calculamos en C#

            // Obtiene todos los IDs y scores de likes y dislikes
            var likes = await _redisDatabase.SortedSetRangeByScoreWithScoresAsync("productosMetricas:likes");
            var dislikes = await _redisDatabase.SortedSetRangeByScoreWithScoresAsync("productosMetricas:dislikes");
        
            // Crea un diccionario para dislikes para acceso rápido
            var dislikesDict = dislikes.ToDictionary(x => x.Element.ToString(), x => x.Score);
        
            // Calcula la diferencia (likes - dislikes) para cada producto
            var productosDiff = likes
                .Select(l =>
                {
                    var id = l.Element.ToString();
                    var likeScore = l.Score;
                    var dislikeScore = dislikesDict.ContainsKey(id) ? dislikesDict[id] : 0;
                    return new { Id = id, Diff = likeScore - dislikeScore };
                })
                .OrderByDescending(x => x.Diff)
                .Take(cantidad)
                .ToList();
        
            var idList = productosDiff.Select(x => x.Id).ToList();
        
            // Obtiene los productos desde MongoDB usando los IDs
            var productos = await _productosCollection.Find(p => idList.Contains(p.Id)).ToListAsync();
        
            // Ordena los productos según el orden de los IDs obtenidos
            var productosOrdenados = idList
                .Select(id => productos.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .Select(p => p!)
                .ToList();
        
            return productosOrdenados;
        }
        



    }
}
