using System.Text.Json;
using DSW_3P_PR02.Models;

namespace DSW_3P_PR02.Repositories;

public sealed class PersonajeJsonRepository : IPersonajeRepository
{
    private readonly string _ruta;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public PersonajeJsonRepository(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _ruta = Path.GetFullPath(Path.Combine(environment.ContentRootPath,
            configuration["Persistencia:RutaArchivo"] ?? "Data/personajes.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(_ruta)!);
    }

    private List<Personaje> Leer()
    {
        if (!File.Exists(_ruta)) return new List<Personaje>();
        // Un archivo corrupto no se interpreta como vacío para evitar pérdida de datos.
        return JsonSerializer.Deserialize<List<Personaje>>(File.ReadAllText(_ruta), _options)
            ?? throw new InvalidDataException("El archivo de personajes no contiene una lista JSON.");
    }

    private void Guardar(List<Personaje> personajes)
    {
        var temporal = _ruta + ".tmp";
        try
        {
            File.WriteAllText(temporal, JsonSerializer.Serialize(personajes, _options));
            File.Move(temporal, _ruta, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporal)) File.Delete(temporal);
        }
    }

    public List<Personaje> ObtenerTodos()
    {
        lock (_lock) return Leer();
    }

    public Personaje? ObtenerPorId(int id)
    {
        lock (_lock) return Leer().FirstOrDefault(p => p.Id == id);
    }

    public List<Personaje> ObtenerPorVideojuego(string nombre)
    {
        lock (_lock) return Leer()
            .Where(p => p.Videojuego.Equals(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public List<Personaje> ObtenerPorNivelMinimo(int nivel)
    {
        lock (_lock) return Leer().Where(p => p.Nivel >= nivel).ToList();
    }

    public Personaje Agregar(Personaje personaje)
    {
        lock (_lock)
        {
            var personajes = Leer();
            personaje.Id = checked(personajes.Select(p => p.Id).DefaultIfEmpty(0).Max() + 1);
            personajes.Add(personaje);
            Guardar(personajes);
            return personaje;
        }
    }

    public bool Actualizar(int id, Personaje personaje)
    {
        lock (_lock)
        {
            var personajes = Leer();
            var index = personajes.FindIndex(p => p.Id == id);
            if (index < 0) return false;
            personaje.Id = id;
            personajes[index] = personaje;
            Guardar(personajes);
            return true;
        }
    }

    public bool Eliminar(int id)
    {
        lock (_lock)
        {
            var personajes = Leer();
            if (personajes.RemoveAll(p => p.Id == id) == 0) return false;
            Guardar(personajes);
            return true;
        }
    }
}
