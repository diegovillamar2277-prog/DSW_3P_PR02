using Microsoft.AspNetCore.Mvc;
using DSW_3P_PR02.Models;
using DSW_3P_PR02.Services;

namespace DSW_3P_PR02.Controllers;

[ApiController]
[Route("api/personajes")]
public class PersonajesController(IPersonajeService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Personaje>> ObtenerTodos() => Ok(service.ObtenerTodos());

    [HttpGet("{id:int}")]
    public ActionResult<Personaje> ObtenerPorId(int id)
    {
        var personaje = service.ObtenerPorId(id);
        if (personaje is null) return NotFound(new ProblemDetails
        {
            Status = 404,
            Title = "Personaje no encontrado",
            Detail = "El personaje no existe."
        });
        return Ok(personaje);
    }

    [HttpGet("videojuego/{nombre}")]
    public ActionResult<List<Personaje>> ObtenerPorVideojuego(string nombre) =>
        Ok(service.ObtenerPorVideojuego(nombre));

    [HttpGet("nivel/{nivel:int}")]
    public ActionResult<List<Personaje>> ObtenerPorNivel(int nivel) =>
        Ok(service.ObtenerPorNivelMinimo(nivel));

    [HttpPost]
    public ActionResult<Personaje> Crear([FromBody] Personaje request)
    {
        var personaje = service.Crear(request);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = personaje.Id }, personaje);
    }

    [HttpPut("{id:int}")]
    public IActionResult Actualizar(int id, [FromBody] Personaje request)
    {
        service.Actualizar(id, request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        service.Eliminar(id);
        return NoContent();
    }
}
