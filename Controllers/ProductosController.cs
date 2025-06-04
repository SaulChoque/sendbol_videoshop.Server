using sendbol_videoshop.Server.Models;
using sendbol_videoshop.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ProductosService _ProductosService;

        public ProductosController(ProductosService ProductosService) =>
            _ProductosService = ProductosService;

        // 1. Devolver todos los registros
        [HttpGet]
        public async Task<List<Producto>> GetAll() =>
            await _ProductosService.GetAllAsync();

        // 2. Buscar coincidencias de una cadena en el título
        [HttpGet("search")]
        public async Task<List<Producto>> SearchByTitulo([FromQuery] string q) =>
            await _ProductosService.SearchByTituloAsync(q);

        // 3. Obtener productos por un array de ids
        [HttpPost("by-ids")]
        public async Task<List<Producto>> GetByIds([FromBody] List<string> ids) =>
            await _ProductosService.GetByIdsAsync(ids);

        // 4. Obtener productos por categoría
        [HttpGet("categoria/{categoria}")]
        public async Task<List<Producto>> GetByCategoria(string categoria) =>
            await _ProductosService.GetByCategoriaAsync(categoria);

        // 5. Obtener productos por plataforma
        [HttpGet("plataforma/{plataforma}")]
        public async Task<List<Producto>> GetByPlataforma(string plataforma) =>
            await _ProductosService.GetByPlataformaAsync(plataforma);

        // 6. Obtener productos por rango de precio
        [HttpGet("rango-precio")]
        public async Task<List<Producto>> GetByRangoPrecio([FromQuery] decimal min, [FromQuery] decimal max) =>
            await _ProductosService.GetByRangoPrecioAsync(min, max);

        // 7. Obtener un producto por id
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Producto>> GetById(string id)
        {
            var producto = await _ProductosService.GetByIdAsync(id);
            if (producto is null)
                return NotFound();
            return producto;
        }
        // ...existing code...

        /// <summary>
        /// Obtiene los productos ordenados de mayor a menor por rating.
        /// </summary>
        /// <param name="cantidad">Cantidad máxima de productos a devolver.</param>
        [HttpGet("top-ranking")]
        public async Task<List<Producto>> GetTopRanking(
            [FromQuery] int cantidad = 10,
            [FromQuery] bool sortOrder = false
            )
            => await _ProductosService.GetProductosPorRatingDescAsync(cantidad, sortOrder);

        /// <summary>
        /// Obtiene los productos ordenados de mayor a menor por (likes - dislikes).
        /// </summary>
        /// <param name="cantidad">Cantidad máxima de productos a devolver.</param>
        [HttpGet("top-likes")]


        public async Task<List<Producto>> GetTopLikes(
            [FromQuery] int cantidad = 10,
            [FromQuery] bool sortOrder = false
            )
            => await _ProductosService.GetProductosPorLikesDescAsync(cantidad, sortOrder);

        // ...existing code...

        /// <summary>
        /// Actualiza el rating de un producto en Redis.
        /// </summary>
        [HttpPost("update-rating/{id}")]
        public async Task<IActionResult> UpdateRatingRedis(string id, [FromBody] RatingDto dto)
        {
            await _ProductosService.UpdateRatingAsync(id, dto.Rating);
            return Ok(new { message = "Rating actualizado en Redis." });
        }

        /// <summary>
        /// Actualiza los likes y dislikes de un producto en Redis.
        /// </summary>
        [HttpPost("update-likes-dislikes/{id}")]
        public async Task<IActionResult> UpdateLikesDislikesRedis(string id, [FromBody] LikesDislikesDto dto)
        {
            await _ProductosService.UpdateLikesDislikesRedisAsync(id, dto.Likes, dto.Dislikes);
            return Ok(new { message = "Likes y dislikes actualizados en Redis." });
        }


        // ...existing code...
        [HttpGet("filtrar")]
        public async Task<List<Producto>> Filtrar(
            [FromQuery] string? categoria,
            [FromQuery] string? plataforma,
            [FromQuery] decimal? min,
            [FromQuery] decimal? max,
            [FromQuery] string? sortBy,
            [FromQuery] bool? sortOrder // "asc" o "desc"
        )
        {
            return await _ProductosService.FiltrarProductosAsync(categoria, plataforma, min, max, sortBy, sortOrder ?? false);
        }




        /*
        // 8. Actualizar likes y dislikes
        [HttpPut("{id:length(24)}/likes-dislikes")]
        public async Task<IActionResult> UpdateLikesDislikes(string id, [FromBody] LikesDislikesDto dto)
        {
            await _ProductosService.UpdateLikesDislikesAsync(id, dto.Likes, dto.Dislikes);
            return NoContent();
        }

        // 9. Actualizar rating
        [HttpPut("{id:length(24)}/rating")]
        public async Task<IActionResult> UpdateRating(string id, [FromBody] RatingDto dto)
        {
            await _ProductosService.UpdateRatingAsync(id, dto.Rating);
            return NoContent();
        }
        */
    }

    public class LikesDislikesDto
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }

    public class RatingDto
    {
        public int Rating { get; set; }
    }
}