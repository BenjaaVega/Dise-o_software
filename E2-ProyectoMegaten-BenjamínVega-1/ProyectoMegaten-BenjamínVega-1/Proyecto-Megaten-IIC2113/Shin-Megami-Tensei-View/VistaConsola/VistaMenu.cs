using System.Reflection;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using Shin_Megami_Tensei_Model.Presentacion.ViewModels;

namespace Shin_Megami_Tensei_View
{
    internal sealed class VistaMenu
    {
        private readonly ConsoleIO _io;

        public VistaMenu(ConsoleIO io) => _io = io;

        public int LeerAccion(string unidadNombre, MenuAcciones menu)
        {
            _io.SepLine();
            _io.WL($"Seleccione una acción para {unidadNombre}");
            foreach (var op in menu.Opciones) _io.WL(op);
            return _io.ReadIntOrDefault(-1);
        }

        public int LeerObjetivo(string atacante, MenuObjetivo menu)
        {
            _io.SepLine();
            _io.WL($"Seleccione un objetivo para {atacante}");
            for (int i = 0; i < menu.Items.Count; i++)
            {
                var it = menu.Items[i];
                _io.WL($"{i + 1}-{it.Nombre} HP:{it.Hp}/{it.HpMax} MP:{it.Mp}/{it.MpMax}");
            }
            _io.WL($"{menu.CancelIndex + 1}-Cancelar");
            return _io.ReadIntOrDefault(menu.CancelIndex + 1);
        }

        public int LeerHabilidad(string unidadNombre, SkillMenu menu)
        {
            _io.SepLine();
            _io.WL($"Seleccione una habilidad para que {unidadNombre} use");
            for (int i = 0; i < menu.Items.Count; i++)
            {
                var it = menu.Items[i];
                var costo = ObtenerCostoMp(it);
                if (costo.HasValue) _io.WL($"{i + 1}-{it.Nombre} MP:{costo.Value}");
                else _io.WL($"{i + 1}-{it.Nombre}");
            }
            _io.WL($"{menu.CancelIndex + 1}-Cancelar");
            return _io.ReadIntOrDefault(menu.CancelIndex + 1);
        }

        public int LeerRandom(string header, IReadOnlyList<string> opciones)
        {
            _io.SepLine();
            _io.WL(header);
            var xs = opciones ?? Array.Empty<string>();
            foreach (var line in xs) _io.WL(line);
            return _io.ReadIntOrDefault(xs.Count);
        }
        
        private static int? ObtenerCostoMp(object item)
        {
            if (item == null) return null;

            var t = item.GetType();
            var p = t.GetProperty("Costo",   BindingFlags.Public | BindingFlags.Instance)
                    ?? t.GetProperty("CostoMp", BindingFlags.Public | BindingFlags.Instance)
                    ?? t.GetProperty("Cost",    BindingFlags.Public | BindingFlags.Instance);
            if (p == null) return null;

            var v = p.GetValue(item, null);
            if (v == null) return null;
            
            if (v is int vi) return vi;
            
            if (v is string s && int.TryParse(s, out var parsed)) return parsed;
            
            try { return System.Convert.ToInt32(v); }
            catch { return null; }
        }

    }
}
