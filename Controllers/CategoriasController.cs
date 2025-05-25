using sendbol_videoshop.Server.Models;
using sendbol_videoshop.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly CategoriasService _categoriasService;

        public CategoriasController(CategoriasService categoriasService) =>
            _categoriasService = categoriasService;

        // 1. Devolver todas las categorías
        [HttpGet]
        public async Task<List<Categoria>> GetAll() =>
            await _categoriasService.GetAllAsync();

        // 2. Buscar coincidencias de una cadena en el título
        [HttpGet("search")]
        public async Task<List<Categoria>> SearchByTitulo([FromQuery] string q) =>
            await _categoriasService.SearchByTituloAsync(q);

        // 3. Obtener una categoría por id
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Categoria>> GetById(string id)
        {
            var categoria = await _categoriasService.GetByIdAsync(id);
            if (categoria is null)
                return NotFound();
            return categoria;
        }
    }
}