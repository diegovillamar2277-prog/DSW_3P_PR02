using DSW_3P_PR02.Models;
namespace DSW_3P_PR02.Services;

public interface IPersonajeService
{
    List<Personaje> ObtenerTodos();
    Personaje? ObtenerPorId(int id);
    List<Personaje> ObtenerPorVideojuego(string nombre);
    List<Personaje> ObtenerPorNivelMinimo(int nivel);
    Personaje Crear(Personaje request);
    void Actualizar(int id, Personaje request);
    void Eliminar(int id);
}
