using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Repositorios.DTO;
using Shin_Megami_Tensei_Model.Repositorios.Ayudantes;
using Shin_Megami_Tensei_Model.Repositorios.Modelos;

namespace Shin_Megami_Tensei_Model.Repositorios
{
    public sealed class JsonUnidades : IRepositorioUnidades
    {
        private readonly Dictionary<string, RegistroUnidad> _samurais;
        private readonly Dictionary<string, RegistroUnidad> _monstruos;

        public JsonUnidades(string samuraiJson = "data/samurai.json", string monstersJson = "data/monsters.json")
        {
            _samurais  = Cargar(ResolucionRutas.ResolverRuta(samuraiJson));
            _monstruos = Cargar(ResolucionRutas.ResolverRuta(monstersJson));
        }

        public IUnidad CrearSamurai(string nombre, IEnumerable<string> skills)
        {
            if (!_samurais.TryGetValue(nombre, out var rec))
                throw new InvalidOperationException($"Samurai no encontrado: {nombre}");

            var s = new Samurai(nombre, rec.Stats, skills);
            if (rec.Affinities is not null)
                s.SetAffinities(ConversorAfinidades.ConvertirASet(rec.Affinities));
            return s;
        }

        public IUnidad CrearMonstruo(string nombre)
        {
            if (!_monstruos.TryGetValue(nombre, out var rec))
                throw new InvalidOperationException($"Monstruo no encontrado: {nombre}");

            var m = new Monstruo(nombre, rec.Stats, (IEnumerable<string>?)rec.Skills ?? Array.Empty<string>());
            if (rec.Affinities is not null)
                m.SetAffinities(ConversorAfinidades.ConvertirASet(rec.Affinities));
            return m;
        }

        // ===== Internos =====

        private static Dictionary<string, RegistroUnidad> Cargar(string ruta)
        {
            var json = File.Exists(ruta) ? File.ReadAllText(ruta) : "[]";

            var items = JsonSerializer.Deserialize<List<DtoUnidad>>(json, ConfiguracionJson.Opciones) ?? new();
            var dict = new Dictionary<string, RegistroUnidad>(StringComparer.OrdinalIgnoreCase);

            foreach (var it in items)
            {
                var nombre = it?.Nombre ?? it?.Name;
                if (string.IsNullOrWhiteSpace(nombre) || it?.Stats is null)
                    continue;

                var stats = new Stats(
                    it.Stats.HP, it.Stats.MP, it.Stats.Str, it.Stats.Skl,
                    it.Stats.Mag, it.Stats.Spd, it.Stats.Lck);

                dict[nombre] = new RegistroUnidad
                {
                    Stats = stats,
                    Affinities = it.Affinity,
                    Skills = (it.Skills ?? new List<string>())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList()
                };
            }

            return dict;
        }
    }
}
