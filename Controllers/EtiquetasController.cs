using sendbol_videoshop.Server.Models;
using sendbol_videoshop.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EtiquetasController : ControllerBase
    {
        private readonly EtiquetasService _EtiquetasService;

        public EtiquetasController(EtiquetasService EtiquetasService) =>
            _EtiquetasService = EtiquetasService;

        // 1. Devolver todos los etiquetas
        [HttpGet]
        public async Task<List<Etiquetas>> GetAll() =>
            await _EtiquetasService.GetAllAsync();

        // 2. Buscar coincidencias de una cadena en el tag
        [HttpGet("search")]
        public async Task<List<Etiquetas>> SearchByTag([FromQuery] string q) =>
            await _EtiquetasService.SearchByTagAsync(q);

        // 3. Obtener un etiquetas por id
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Etiquetas>> GetById(string id)
        {
            var etiqueta = await _EtiquetasService.GetByIdAsync(id);
            if (etiqueta is null)
                return NotFound();
            return etiqueta;
        }
    }
}