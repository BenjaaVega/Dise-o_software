using System;
using System.IO;

namespace Shin_Megami_Tensei_Model.Repositorios.Ayudantes
{
    internal static class ResolucionRutas
    {
        private static readonly string[] Bases =
        {
            AppContext.BaseDirectory,
            Directory.GetCurrentDirectory(),
            Path.Combine(AppContext.BaseDirectory, "..")
        };

        public static string ResolverRuta(string ruta)
        {
            if (File.Exists(ruta)) return ruta;

            foreach (var b in Bases)
            {
                var p = Path.Combine(b, ruta);
                if (File.Exists(p)) return p;
                
                var pdata = Path.Combine(b, "data", Path.GetFileName(ruta));
                if (File.Exists(pdata)) return pdata;
            }
            return ruta; 
        }

        public static string ResolverRutaSkills(string? rutaExplicita)
        {
            if (!string.IsNullOrWhiteSpace(rutaExplicita) && File.Exists(rutaExplicita))
                return rutaExplicita;

            foreach (var b in Bases)
            {
                var p1 = Path.Combine(b, "data", "skills.json");
                if (File.Exists(p1)) return p1;

                var p2 = Path.Combine(b, "skills.json");
                if (File.Exists(p2)) return p2;
            }
            return Path.Combine("data", "skills.json");
        }
    }
}