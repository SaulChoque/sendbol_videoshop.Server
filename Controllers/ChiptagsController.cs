using sendbol_videoshop.Server.Models;
using sendbol_videoshop.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChiptagsController : ControllerBase
    {
        private readonly ChiptagsService _chiptagsService;

        public ChiptagsController(ChiptagsService chiptagsService) =>
            _chiptagsService = chiptagsService;

        // 1. Devolver todos los chiptags
        [HttpGet]
        public async Task<List<Chiptags>> GetAll() =>
            await _chiptagsService.GetAllAsync();

        // 2. Buscar coincidencias de una cadena en el tag
        [HttpGet("search")]
        public async Task<List<Chiptags>> SearchByTag([FromQuery] string q) =>
            await _chiptagsService.SearchByTagAsync(q);

        // 3. Obtener un chiptags por id
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Chiptags>> GetById(string id)
        {
            var chiptag = await _chiptagsService.GetByIdAsync(id);
            if (chiptag is null)
                return NotFound();
            return chiptag;
        }
    }
}