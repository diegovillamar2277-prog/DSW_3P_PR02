using System.ComponentModel.DataAnnotations;
using DSW_3P_PR02.Models;
using DSW_3P_PR02.Repositories;

namespace DSW_3P_PR02.Services;

public class PersonajeService(IPersonajeRepository repository) : IPersonajeService
{
    private static Personaje ValidarYCrear(Personaje request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ValidationException("El nombre del personaje es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Videojuego))
            throw new ValidationException("El videojuego es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Clase))
            throw new ValidationException("La clase es obligatoria.");
        if (request.Nivel is < 1 or > 100)
            throw new ValidationException("El nivel debe estar entre 1 y 100.");
        if (request.Poder <= 0)
            throw new ValidationException("El poder debe ser mayor que cero.");
        if (!Uri.TryCreate(request.ImagenUrl?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ValidationException("ImagenUrl debe ser una URL absoluta HTTP o HTTPS válida.");
        return new Personaje
        {
            Nombre = request.Nombre.Trim(),
            Videojuego = request.Videojuego.Trim(),
            Clase = request.Clase.Trim(),
            Nivel = request.Nivel,
            Poder = request.Poder,
            Disponible = request.Disponible,
            ImagenUrl = uri.ToString()
        };
    }

    public List<Personaje> ObtenerTodos() => repository.ObtenerTodos();

    public Personaje? ObtenerPorId(int id) => repository.ObtenerPorId(id);

    public List<Personaje> ObtenerPorVideojuego(string nombre) =>
        repository.ObtenerPorVideojuego(nombre.Trim());

    public List<Personaje> ObtenerPorNivelMinimo(int nivel) =>
        repository.ObtenerPorNivelMinimo(nivel);

    public Personaje Crear(Personaje request)
    {
        var personaje = ValidarYCrear(request);
        personaje.Disponible = true;
        return repository.Agregar(personaje);
    }

    public void Actualizar(int id, Personaje request)
    {
        if (repository.ObtenerPorId(id) is null)
            throw new KeyNotFoundException("El personaje no existe.");
        var personaje = ValidarYCrear(request);
        if (!repository.Actualizar(id, personaje))
            throw new KeyNotFoundException("El personaje no existe.");
    }

    public void Eliminar(int id)
    {
        if (!repository.Eliminar(id))
            throw new KeyNotFoundException("El personaje no existe.");
    }
}
