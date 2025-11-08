using Shin_Megami_Tensei_Model.Entrada;
using Shin_Megami_Tensei_Model.Repositorios;

namespace Shin_Megami_Tensei_Model.Validacion;

public sealed record ResultadoValidacion(IReadOnlyList<string> Errores)
{
    public bool EsValido => Errores.Count == 0;
}

public static class ValidacionEquipos
{
    private const int MaxUnidades = 8;
    private const int MaxHabilidadesSamurai = 8;

    public static ResultadoValidacion ValidarCrudos(IReadOnlyList<IntentoUnidad> unidades, ISkillsLookup skills)
    {
        var errores = new List<string>();
        var samurais = unidades.Where(u => u.EsSamurai).ToList();

        if (samurais.Count == 0) errores.Add("Sin samurai");
        if (samurais.Count > 1) errores.Add("Más de un samurai");
        if (unidades.Count > MaxUnidades) errores.Add("Más de 8 unidades");

        if (TieneDuplicados(unidades.Select(u => u.Nombre)))
            errores.Add("Unidades repetidas");
        
        if (samurais.Count == 1)
        {
            var samurai = samurais[0];

            if (samurai.Skills.Count > MaxHabilidadesSamurai)
                errores.Add("Samurai con más de 8 habilidades");

            if (TieneDuplicados(samurai.Skills))
                errores.Add("Habilidades repetidas");

            foreach (var habilidad in samurai.Skills)
                if (!skills.Existe(habilidad))
                    errores.Add($"Habilidad inexistente: {habilidad}");
        }

        return new ResultadoValidacion(errores);
    }

    private static bool TieneDuplicados(IEnumerable<string> valores)
    {
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var valor in valores)
        {
            if (!vistos.Add(valor)) return true;
        }
        return false;
    }
}