using System.Text.Json;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Repositorios.DTO;
using Shin_Megami_Tensei_Model.Repositorios.Ayudantes;

namespace Shin_Megami_Tensei_Model.Repositorios
{
    public sealed class JsonSkills : ISkillsLookup
    {
        private readonly HashSet<string> _habilidades;
        private readonly Dictionary<string, int?> _costos;

        private readonly Dictionary<string, Elemento> _elementos;
        private readonly Dictionary<string, int> _powers;
        private readonly Dictionary<string, string> _hits;
        private readonly Dictionary<string, string> _effect;

        public JsonSkills(string? rutaExplicita = null)
        {
            var ruta = ResolucionRutas.ResolverRutaSkills(rutaExplicita);
            var contenidoJson = File.Exists(ruta) ? File.ReadAllText(ruta) : "[]";
            var registros = JsonSerializer.Deserialize<List<DtoHabilidad>>(contenidoJson, ConfiguracionJson.Opciones) ?? new();

            (_habilidades, _costos, _elementos, _powers, _hits, _effect) = ConstruirColecciones(registros);
        }

        public bool Existe(string nombre) => _habilidades.Contains(nombre);

        public int? CostoMp(string nombre) =>
            _costos.TryGetValue(nombre, out var c) ? c : (int?)null;

        public IEnumerable<string> Todas() => _habilidades;

        public Elemento? ElementoDe(string nombre) =>
            _elementos.TryGetValue(nombre, out var e) ? e : (Elemento?)null;

        public int? PowerDe(string nombre) =>
            _powers.TryGetValue(nombre, out var p) ? p : (int?)null;

        public string? HitsDe(string nombre) =>
            _hits.TryGetValue(nombre, out var h) ? h : null;

        public string? EffectDe(string nombre) =>
            _effect.TryGetValue(nombre, out var e) ? e : null;

        // ===== Internos =====

        private static (
            HashSet<string> habilidades,
            Dictionary<string,int?> costos,
            Dictionary<string,Elemento> elementos,
            Dictionary<string,int> poderes,
            Dictionary<string,string> hits,
            Dictionary<string,string> effect
        ) ConstruirColecciones(IEnumerable<DtoHabilidad> items)
        {
            var habilidades = new HashSet<string>(
                items.Select(i => i.Nombre ?? i.Name ?? string.Empty)
                     .Where(s => !string.IsNullOrWhiteSpace(s)),
                StringComparer.OrdinalIgnoreCase);

            var costos = items
                .Select(i => new { Nombre = i.Nombre ?? i.Name, Costo = i.CostoMp ?? i.Cost })
                .Where(x => !string.IsNullOrWhiteSpace(x.Nombre))
                .ToDictionary(x => x.Nombre!, x => x.Costo, StringComparer.OrdinalIgnoreCase);

            var elementos = new Dictionary<string, Elemento>(StringComparer.OrdinalIgnoreCase);
            var poderes   = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var hits      = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var effect    = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var it in items)
            {
                var nombre = it.Nombre ?? it.Name;
                if (string.IsNullOrWhiteSpace(nombre)) continue;

                if (MapeadorElementos.IntentarMapear(it.Type, out var e))
                    elementos[nombre] = e;

                if (it.Power.HasValue) poderes[nombre] = it.Power.Value;

                if (!string.IsNullOrWhiteSpace(it.Hits))
                    hits[nombre] = it.Hits!.Trim();

                if (!string.IsNullOrWhiteSpace(it.Effect))
                    effect[nombre] = it.Effect!.Trim();
            }

            return (habilidades, costos, elementos, poderes, hits, effect);
        }
    }
}
