using sendbol_videoshop.Server.Models;
using sendbol_videoshop.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlataformasController : ControllerBase
    {
        private readonly PlataformasService _plataformasService;

        public PlataformasController(PlataformasService plataformasService) =>
            _plataformasService = plataformasService;

        // 1. Devolver todas las plataformas
        [HttpGet]
        public async Task<List<Plataforma>> GetAll() =>
            await _plataformasService.GetAllAsync();

        // 2. Buscar coincidencias de una cadena en el nombre
        [HttpGet("search")]
        public async Task<List<Plataforma>> SearchByNombre([FromQuery] string q) =>
            await _plataformasService.SearchByNombreAsync(q);

        // 3. Obtener una plataforma por id
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Plataforma>> GetById(string id)
        {
            var plataforma = await _plataformasService.GetByIdAsync(id);
            if (plataforma is null)
                return NotFound();
            return plataforma;
        }
    }
}