using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Repositorios;
using Shin_Megami_Tensei_Model.Presentacion;
using Shin_Megami_Tensei.Manejo.Helpers;
using Shin_Megami_Tensei.Manejo.Services;

namespace Shin_Megami_Tensei.Manejo
{
    public sealed class ManejoHabilidad : IManejoAccion
    {
        private readonly IVistaJuego _vista;
        private readonly ISkillsLookup _habilidades;
        private readonly CalcularDano _calcularDano = new();
        private readonly ManejoInvocar _invocar;
        private readonly ContadorK _contadorK = new();

        public ManejoHabilidad(IVistaJuego vistaJuego, ISkillsLookup habilidades, ManejoInvocar invocar)
        {
            _vista = vistaJuego;
            _habilidades = habilidades;
            _invocar = invocar;
        }

        public IManejoAccion.ActionResult Execute(Tablero tablero, IUnidad unidadActual, bool esJ1, Turnos turnos)
        {
            var nombresHabilidades = ObtenerHabilidadesDe(unidadActual);
            var disponibles = new List<(string nombre, int? costo)>();
            foreach (var habilidad in nombresHabilidades)
            {
                var costo = _habilidades.CostoMp(habilidad);
                if (costo.HasValue && unidadActual.Mp < costo.Value) continue;
                disponibles.Add((habilidad, costo));
            }

            var menu = FormateadorMenus.MenuHabilidades(disponibles);
            int indiceSeleccion = _vista.ReadSkill(unidadActual.Nombre, menu);
            if (indiceSeleccion == menu.CancelIndex + 1) return IManejoAccion.ActionResult.Continuar();
            if (indiceSeleccion < 1 || indiceSeleccion > menu.Items.Count)
            {
                _vista.ShowMessage("Selección inválida.");
                return IManejoAccion.ActionResult.Continuar();
            }

            string nombre = menu.Items[indiceSeleccion - 1].Nombre;
            if (!_habilidades.Existe(nombre))
            {
                _vista.ShowMessage($"La habilidad '{nombre}' no existe en JSON.");
                return IManejoAccion.ActionResult.Continuar();
            }

            int costoMp = _habilidades.CostoMp(nombre) ?? 0;
            if (unidadActual.Mp < costoMp)
            {
                _vista.ShowMessage("MP insuficiente.");
                return IManejoAccion.ActionResult.Continuar();
            }
            
            if (EsSabbatma(nombre))
            {
                var resultadoAccion = _invocar.ExecuteDesdeSabbatma(tablero, unidadActual, esJ1, turnos, costoMp, out bool invocacionOk);
                if (invocacionOk) _contadorK.Inc(esJ1);
                return resultadoAccion;
            }
            if (EsInvitacion(nombre))
            {
                var resultadoAccion = _invocar.ExecuteDesdeInvitation(tablero, unidadActual, esJ1, turnos, costoMp, out bool invocacionOk);
                if (invocacionOk) _contadorK.Inc(esJ1);
                return resultadoAccion;
            }
            
            var efecto = _habilidades.EffectDe(nombre) ?? string.Empty;
            var tipo = RealizarCura.Clasificar(efecto);
            if (tipo != RealizarCura.HealKind.None)
            {
                var porcentajePoderOpcional = _habilidades.PowerDe(nombre);
                if (porcentajePoderOpcional is null)
                {
                    _vista.ShowMessage($"La habilidad '{nombre}' no trae 'power' en el JSON.");
                    return IManejoAccion.ActionResult.Continuar();
                }
                int porcentajePoder = porcentajePoderOpcional.Value;
                bool recarmdra = nombre.Trim().Equals("Recarmdra", StringComparison.OrdinalIgnoreCase)
                                 || efecto.ToLowerInvariant().Contains("in exchange for all hp");

                var propio = tablero.Propio(esJ1);
                bool aplicado = tipo switch
                {
                    RealizarCura.HealKind.HealParty    => RealizarCura.CuraEquipo(_vista, propio, unidadActual, porcentajePoder, recarmdra),
                    RealizarCura.HealKind.ReviveParty  => RealizarCura.ReviveEquipo(_vista, propio, unidadActual, porcentajePoder, recarmdra),
                    RealizarCura.HealKind.HealSingle   => RealizarCura.EfectoSolo(_vista, propio, unidadActual, porcentajePoder, revive: false),
                    RealizarCura.HealKind.ReviveSingle => RealizarCura.EfectoSolo(_vista, propio, unidadActual, porcentajePoder, revive: true),
                    _ => false
                };

                if (aplicado)
                {
                    unidadActual.GastarMp(costoMp);
                    var (usarFull, usarBlink, ganarBlink) = TurnosHelper.DecidirConsumo(turnos, HitOutcome.Damage);
                    TurnosHelper.AplicarConsumo(turnos, usarFull, usarBlink, ganarBlink);
                    _vista.ShowTurnsConsumption(usarFull, usarBlink, ganarBlink);
                    _contadorK.Inc(esJ1);
                }
                return IManejoAccion.ActionResult.Continuar();
            }
            var enemigo = esJ1 ? tablero.J2 : tablero.J1;
            var objetivo = SelectorObjetivo.ElegirEnemigoActivo(_vista, enemigo, unidadActual.Nombre);
            if (objetivo is null) return IManejoAccion.ActionResult.Continuar();

            var elementoOpcional = _habilidades.ElementoDe(nombre);
            var poderOpcional    = _habilidades.PowerDe(nombre);
            if (elementoOpcional is null || poderOpcional is null)
            {
                _vista.ShowMessage($"La habilidad '{nombre}' no trae 'type' o 'power' en el JSON.");
                return IManejoAccion.ActionResult.Continuar();
            }
            var elemento = elementoOpcional.Value;
            int poder    = poderOpcional.Value;

            var (minGolpes, maxGolpes) = HitsParser.Parse(_habilidades.HitsDe(nombre) ?? _habilidades.EffectDe(nombre));
            int rangoGolpes   = Math.Max(1, (maxGolpes - minGolpes + 1));
            int desplazamiento = _contadorK.Get(esJ1) % rangoGolpes;
            int cantidadGolpes = Math.Max(1, minGolpes + desplazamiento);

            unidadActual.GastarMp(costoMp);
            var ultimoResultadoGolpe = RealizarDano.HacerAtaqueMultiHit(_vista, _calcularDano, tablero, esJ1, unidadActual, objetivo, nombre, elemento, poder, cantidadGolpes);

            var (usarFull2, usarBlink2, ganarBlink2) = TurnosHelper.DecidirConsumo(turnos, ultimoResultadoGolpe);
            TurnosHelper.AplicarConsumo(turnos, usarFull2, usarBlink2, ganarBlink2);
            _vista.ShowTurnsConsumption(usarFull2, usarBlink2, ganarBlink2);

            _contadorK.Inc(esJ1);

            if (CombateHelper.EquipoRivalDerrotado(tablero, esJ1))
            {
                var (ganador, tag) = CombateHelper.NombreEquipo(tablero, esJ1);
                return IManejoAccion.ActionResult.Fin(ganador, tag);
            }

            return IManejoAccion.ActionResult.Continuar();
        }

        private static IReadOnlyList<string> ObtenerHabilidadesDe(IUnidad unidad)
        {
            if (unidad is Samurai samurai && samurai.Habilidades is not null) return samurai.Habilidades;
            var propiedad = unidad.GetType().GetProperty("Habilidades");
            if (propiedad?.GetValue(unidad) is IEnumerable<string> lista) return lista.ToList();
            return Array.Empty<string>();
        }

        private static bool EsSabbatma(string nombreHabilidad)
        {
            var nombreNormalizado = (nombreHabilidad ?? "").Trim().ToLowerInvariant();
            return nombreNormalizado == "sabbatma" || nombreNormalizado == "sabatma" || nombreNormalizado == "sabatman";
        }

        private static bool EsInvitacion(string nombreHabilidad) =>
            (nombreHabilidad ?? "").Trim().Equals("invitation", StringComparison.OrdinalIgnoreCase);
    }
}
