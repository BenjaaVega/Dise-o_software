using System.Text;

namespace Shin_Megami_Tensei_Model.Entrada;

public sealed class ParserArchivoEquipos : IEquipoParser
{
    private const string EncabezadoJugador1 = "Player 1 Team";
    private const string EncabezadoJugador2 = "Player 2 Team";
    private const string SamuraiTagA = "[Samurai]";
    private const string SamuraiTagB = "[ Samurai ]";

    public EquiposCrudos Parse(string pathArchivo)
    {
        var lineas = LeerLineasLimpias(pathArchivo);

        var jugador1 = new List<IntentoUnidad>();
        var jugador2 = new List<IntentoUnidad>();
        List<IntentoUnidad>? equipoActual = null;

        foreach (var linea in lineas)
        {
            if (EsEncabezado(linea, EncabezadoJugador1)) { equipoActual = jugador1; continue; }
            if (EsEncabezado(linea, EncabezadoJugador2)) { equipoActual = jugador2; continue; }
            if (equipoActual is null) continue; 
            
            var esSamurai = EsLineaSamurai(linea);
            
            var resto = esSamurai ? TextoDespuesDeCierreDeCorchete(linea) : linea;
            
            var (nombre, skills) = ParsearNombreYSkills(resto);

            equipoActual.Add(new IntentoUnidad(nombre, esSamurai, skills));
        }

        return new EquiposCrudos(jugador1, jugador2);
    }
    
    private static List<string> LeerLineasLimpias(string rutaArchivo) =>
        File.ReadAllLines(rutaArchivo, Encoding.UTF8)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

    private static bool EsEncabezado(string linea, string encabezado) =>
        linea.StartsWith(encabezado, StringComparison.OrdinalIgnoreCase);

    private static bool EsLineaSamurai(string linea) =>
        linea.StartsWith(SamuraiTagA, StringComparison.OrdinalIgnoreCase)
        || linea.StartsWith(SamuraiTagB, StringComparison.OrdinalIgnoreCase);

    private static string TextoDespuesDeCierreDeCorchete(string linea)
    {
        var idx = linea.IndexOf(']');
        return idx >= 0 && idx < linea.Length - 1
            ? linea[(idx + 1)..].Trim()
            : linea.Trim();
    }

    private static (string nombre, List<string> skills) ParsearNombreYSkills(string texto)
    {
        var parentesisApertura = texto.IndexOf('(');
        var terminaConCierre = texto.EndsWith(")", StringComparison.Ordinal);

        if (parentesisApertura >= 0 && terminaConCierre)
        {
            var nombre = texto[..parentesisApertura].Trim();
            var dentro = texto[(parentesisApertura + 1)..^1];
            var habilidades = dentro
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            return (nombre, habilidades);
        }

        return (texto.Trim(), new List<string>());
    }
}
