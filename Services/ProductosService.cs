using sendbol_videoshop.Server.Models; // Importa los modelos definidos en la carpeta Models.
using Microsoft.Extensions.Options; // Proporciona acceso a configuraciones fuertemente tipadas.
using MongoDB.Driver; // Biblioteca para interactuar con MongoDB.
using StackExchange.Redis;
using System.Text.Json; // Agrega esta línea para usar Redis


namespace sendbol_videoshop.Server.Services
{
    // ...existing code...

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

            // Imprime la configuración de Redis para depuración
            Console.WriteLine("Configuración de Redis:");
            Console.WriteLine(redisVideoshopDatabaseSettings.Value.Endpoints);
            Console.WriteLine(redisVideoshopDatabaseSettings.Value.Port);
            Console.WriteLine(redisVideoshopDatabaseSettings.Value.User);
            Console.WriteLine(redisVideoshopDatabaseSettings.Value.Password);

            // Crea una conexión a Redis utilizando la cadena de conexión proporcionada
            var config = new ConfigurationOptions
            {
                User = redisVideoshopDatabaseSettings.Value.User,
                Password = redisVideoshopDatabaseSettings.Value.Password
            };
            config.EndPoints.Add(redisVideoshopDatabaseSettings.Value.Endpoints, int.Parse(redisVideoshopDatabaseSettings.Value.Port));
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(config);


            /*
             *             ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { { "redis-17335.c336.samerica-east1-1.gce.redns.redis-cloud.com", 17335 } },
                    User = "default",
                    Password = "qE6qc3mD0cMQKqg7co34HoBvUCvYDQTi"
                }
            );
             * 
             Configuración de Redis:
redis-17335.c336.samerica-east1-1.gce.redns.redis-cloud.com
17335
default
qE6qc3mD0cMQKqg7co34HoBvUCvYDQTi
             */

            // Obtiene la colección de Productos dentro de la base de datos.
            _productosCollection = mongoDatabase.GetCollection<Producto>(
                VideoshopDatabaseSettings.Value.ProductosCollectionName);

            // Obtiene la base de datos Redis
            _redisDatabase = redis.GetDatabase();

        }


        // ...existing code...

        /// <summary>
        /// Agrega un nuevo producto a la colección de MongoDB.
        /// </summary>
        /// <param name="producto">El producto a agregar.</param>
        /// <returns>El producto agregado.</returns>
        public async Task<Producto> AddProductoAsync(Producto producto)
        {
            await _productosCollection.InsertOneAsync(producto);
            return producto;
        }

        // ...existing code...





        /// <summary>
        /// Método para obtener todos los productos de la colección.
        /// Solo consulta MongoDB, no Redis.
        /// </summary>
        /// <returns>
        /// Lista de productos obtenidos de MongoDB.
        /// </returns>
        public async Task<List<Producto>> GetAllAsync() =>
            await _productosCollection.Find(_ => true).ToListAsync();

        /// <summary>
        /// Método para obtener todos los productos de la colección.
        /// Solo consulta MongoDB, no Redis.
        /// </summary>
        /// <returns>
        /// Lista de productos obtenidos de MongoDB.
        /// </returns>
        public async Task<List<Producto>> GetOrFetchAllProductsAsync()
        {
            return await _productosCollection.Find(_ => true).ToListAsync();
        }

        // Elimina la función de clonar productos a Redis
        // public async Task ClonarProductosMongoARedisAsync() { ... } // ELIMINADA

        // Elimina la función de obtener productos desde Redis
        // public async Task<List<Producto>> GetAllProductsFromRedisAsync() { ... } // ELIMINADA

        /// <summary>
        /// Busca coincidencias de una cadena en el título usando MongoDB.
        /// </summary>
        public async Task<List<Producto>> SearchByTituloAsync(string search)
        {
            var filter = Builders<Producto>.Filter.Regex(p => p.Titulo, new MongoDB.Bson.BsonRegularExpression(search, "i"));
            return await _productosCollection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Obtiene productos por un array de ids usando MongoDB.
        /// </summary>
        public async Task<List<Producto>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var filter = Builders<Producto>.Filter.In(p => p.Id, ids);
            return await _productosCollection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Obtiene productos por categoría usando MongoDB.
        /// </summary>
        public async Task<List<Producto>> GetByCategoriaAsync(string categoria)
        {
            var filter = Builders<Producto>.Filter.Eq(p => p.Categoria, categoria);
            return await _productosCollection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Obtiene productos por plataforma usando MongoDB.
        /// </summary>
        public async Task<List<Producto>> GetByPlataformaAsync(string plataforma)
        {
            var filter = Builders<Producto>.Filter.AnyEq(p => p.Plataformas, plataforma);
            return await _productosCollection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Obtiene productos por rango de precio usando MongoDB.
        /// </summary>
        public async Task<List<Producto>> GetByRangoPrecioAsync(decimal min, decimal max)
        {
            var filter = Builders<Producto>.Filter.Gte(p => p.Precio, min) &
                         Builders<Producto>.Filter.Lte(p => p.Precio, max);
            return await _productosCollection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Obtiene un producto por id usando MongoDB.
        /// </summary>
        public async Task<Producto> GetByIdAsync(string id)
        {
            var filter = Builders<Producto>.Filter.Eq(p => p.Id, id);
            return await _productosCollection.Find(filter).FirstOrDefaultAsync();
        }

        // ...existing code for ZSETs de productosMetricas...

        // El resto de la lógica de métricas (likes, dislikes, ranking) sigue usando Redis ZSETs
        // ...existing code...



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
        public async Task<List<Producto>> GetProductosPorRatingDescAsync(int cantidad, bool order)
        {
            Order ordenRedis = Order.Descending;
            if (!order) ordenRedis = Order.Ascending;

            await SincronizarRatingRedisAsync(); // Asegura que Redis tenga los datos
                                                 // Obtiene los IDs de los productos ordenados por rating descendente desde Redis


            var ids = await _redisDatabase.SortedSetRangeByScoreAsync(
                "productosMetricas:ranking",
                order: ordenRedis,
                take: cantidad
            );

            // Convierte los IDs a string
            var idList = ids.Select(
                id => id.ToString())
                .Where(id => !string.IsNullOrEmpty(id) && id != "undefined")
                .ToList();


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
        public async Task<List<Producto>> GetProductosPorLikesDescAsync(int cantidad, bool order)
        {
            Order ordenRedis = Order.Descending;
            if (!order) ordenRedis = Order.Ascending;

            await SincronizarLikesDislikesRedisAsync(); // Asegura que Redis tenga los datos

            // Obtiene los IDs de los productos ordenados por likes descendente desde Redis
            // Primero sincroniza un ZSET temporal con la diferencia likes - dislikes
            // NOTA: Redis no soporta operaciones aritméticas directas en ZSET, así que lo calculamos en C#

            // Obtiene todos los IDs y scores de likes y dislikes
            var likes = await _redisDatabase.SortedSetRangeByScoreWithScoresAsync(
                "productosMetricas:likes",
                order: ordenRedis,
                take: cantidad
                );
            var dislikes = await _redisDatabase.SortedSetRangeByScoreWithScoresAsync(
                "productosMetricas:dislikes",
                order: ordenRedis,
                take: cantidad
                );

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

            // ...existing code...
            // ...existing code...
            var idList = productosDiff
                .Select(x => x.Id)
                .Where(id => !string.IsNullOrEmpty(id) && id != "undefined")
                .ToList();

            var productos = await _productosCollection.Find(p => idList.Contains(p.Id)).ToListAsync();
            // ...existing code...
            // ...existing code...

            // Ordena los productos según el orden de los IDs obtenidos
            var productosOrdenados = idList
                .Select(id => productos.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .Select(p => p!)
                .ToList();

            return productosOrdenados;
        }






        public async Task<List<Producto>> FiltrarProductosAsync(
            string? categoria,
            string? plataforma,
            decimal? min,
            decimal? max,
            string? sortBy,
            bool? sortOrder
            )
        {
            var filterBuilder = Builders<Producto>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(categoria))
                filter &= filterBuilder.Eq(p => p.Categoria, categoria);

            if (!string.IsNullOrEmpty(plataforma))
                filter &= filterBuilder.AnyEq(p => p.Plataformas, plataforma);

            if (min.HasValue)
                filter &= filterBuilder.Gte(p => p.Precio, min.Value);

            if (max.HasValue)
                filter &= filterBuilder.Lte(p => p.Precio, max.Value);

            // Si el ordenamiento es por ranking o likes, usa Redis para obtener el orden
            if (!string.IsNullOrEmpty(sortBy) && (sortBy.ToLower() == "ranking" || sortBy.ToLower() == "likes"))
            {
                //bool desc = sortOrder?.ToLower() == "desc";
                List<Producto> productosOrdenados;

                if (sortBy.Equals("ranking", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Puedes ajustar la cantidad máxima si lo deseas
                    var productosRanking = await GetProductosPorRatingDescAsync(5000, sortOrder ?? false);
                    productosOrdenados = productosRanking;
                }
                else // likes
                {
                    var productosLikes = await GetProductosPorLikesDescAsync(5000, sortOrder ?? false);
                    productosOrdenados = productosLikes;
                }

                // Aplica los filtros sobre la lista ordenada
                var productosFiltrados = productosOrdenados
                    .Where(p =>
                        (string.IsNullOrEmpty(categoria) || p.Categoria == categoria) &&
                        (string.IsNullOrEmpty(plataforma) || (p.Plataformas != null && p.Plataformas.Contains(plataforma))) &&
                        (!min.HasValue || p.Precio >= min.Value) &&
                        (!max.HasValue || p.Precio <= max.Value)
                    )
                    .ToList();


                return productosFiltrados;
            }
            else
            {
                // Ordenamiento tradicional en memoria para otros campos
                var productos = await _productosCollection.Find(filter).ToListAsync();

                if (!string.IsNullOrEmpty(sortBy))
                {

                    productos = sortBy.ToLower() switch
                    {
                        "precio" => sortOrder.GetValueOrDefault() ? productos.OrderByDescending(p => p.Precio).ToList() : productos.OrderBy(p => p.Precio).ToList(),
                        _ => productos
                    };
                }

                return productos;
            }
        }




    }
}
