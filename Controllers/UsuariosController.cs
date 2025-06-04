using sendbol_videoshop.Server.Models; // Importa los modelos definidos en la carpeta Models.
using sendbol_videoshop.Server.Services; // Importa los modelos definidos en la carpeta Services.
using Microsoft.AspNetCore.Mvc; // Proporciona acceso a los controladores de ASP.NET Core.
using Microsoft.AspNetCore.Http; 

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase // Cambia la herencia de la clase a `ControllerBase`.
    {
        private readonly UsuariosService _usuariosService;

        public UsuariosController(UsuariosService UsuariosService) =>
            _usuariosService = UsuariosService;


        //CMMT EXPLN Get retorna TODA la informacion

        [HttpGet]
        public async Task<List<Usuario>> Get() =>
            await _usuariosService.GetAsync();



        //CMMT EXPLN Get retorna la informacion que conicide con el parametro
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Usuario>> Get(string id)
        {
            var Usuario = await _usuariosService.GetAsync(id);

            if (Usuario is null)
            {
                return NotFound();
            }

            return Usuario;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Usuario newUsuario)
        {
            Console.WriteLine("COREROOOOOOOOOOOOOOOOOOOOOO ");
            Console.WriteLine(newUsuario);
            await _usuariosService.CreateAsync(newUsuario);

            return CreatedAtAction(nameof(Get), new { id = newUsuario.Id }, newUsuario);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Usuario updatedUsuario)
        {
            var Usuario = await _usuariosService.GetAsync(id);

            if (Usuario is null)
            {
                return NotFound();
            }

            updatedUsuario.Id = Usuario.Id;

            await _usuariosService.UpdateAsync(id, updatedUsuario);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var Usuario = await _usuariosService.GetAsync(id);

            if (Usuario is null)
            {
                return NotFound();
            }

            await _usuariosService.RemoveAsync(id);

            return NoContent();
        }

        [HttpGet("exists/{correo}")]
        public async Task<IActionResult> Exists(string correo)
        {
            var exists = await _usuariosService.ExistsByCorreoAsync(correo);

            if (!exists)
            {
                return NotFound(new { message = "El usuario con el correo especificado no existe." });
            }

            return Ok(new { message = "El usuario existe." });
        }


        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLogin login)
        {
            var usuario = await _usuariosService.GetByCorreoYPasswordAsync(login.Correo, login.Contrasena);
            if (usuario == null)
                return Unauthorized(new { message = "Credenciales incorrectas" });
        
            // Guardar el ID del usuario en la sesión (almacenada en Redis)
            HttpContext.Session.SetString("UsuarioId", usuario.Id);
        
            return Ok(new { message = "Sesión iniciada", usuarioId = usuario.Id });
        }
        
        [HttpGet("session-user")]
        public async Task<IActionResult> GetUsuarioSesion()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized(new { message = "No hay usuario en sesión" });
        
            var usuario = await _usuariosService.GetAsync(usuarioId);
            if (usuario == null)
                return NotFound();
        
            return Ok(usuario);
        }


        
        
        // Endpoint para dar like a un producto
        [HttpPost("{usuarioId}/like")]
        public async Task<IActionResult> LikeProducto(string usuarioId, [FromBody] ProdInfoItem prodInfoItem)
        {
            var result = await _usuariosService.UpdateProdInfoAsync(usuarioId, prodInfoItem, "like");
            if (!result)
                return BadRequest(new { message = "No se pudo registrar el like." });
            return Ok(new { message = "Like registrado correctamente." });
        }

        // Endpoint para dar dislike a un producto
        [HttpPost("{usuarioId}/dislike")]
        public async Task<IActionResult> DislikeProducto(string usuarioId, [FromBody] ProdInfoItem prodInfoItem)
        {
            var result = await _usuariosService.UpdateProdInfoAsync(usuarioId, prodInfoItem, "dislike");
            if (!result)
                return BadRequest(new { message = "No se pudo registrar el dislike." });
            return Ok(new { message = "Dislike registrado correctamente." });
        }

        // Endpoint para calificar (rating) un producto
        [HttpPost("{usuarioId}/rating")]
        public async Task<IActionResult> RatingProducto(string usuarioId, [FromBody] ProdInfoItem prodInfoItem)
        {
            var result = await _usuariosService.UpdateProdInfoAsync(usuarioId, prodInfoItem, "rating");
            if (!result)
                return BadRequest(new { message = "No se pudo registrar el rating." });
            return Ok(new { message = "Rating registrado correctamente." });
        }




    }

}
