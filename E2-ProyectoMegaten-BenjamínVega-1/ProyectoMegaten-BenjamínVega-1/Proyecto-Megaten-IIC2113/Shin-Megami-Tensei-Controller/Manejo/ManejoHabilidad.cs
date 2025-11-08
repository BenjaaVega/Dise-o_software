using System.Collections.Generic;
using System.Linq;
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
            var targetKind = _habilidades.TargetDe(nombre) ?? SkillTarget.Single;

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
            int rangoGolpes    = Math.Max(1, (maxGolpes - minGolpes + 1));
            int desplazamiento = _contadorK.Get(esJ1) % rangoGolpes;
            int cantidadGolpes = Math.Max(1, minGolpes + desplazamiento);

            var drainKind = ClasificarDrain(efecto);

            if (drainKind == DrainKind.Mp && (targetKind == SkillTarget.Single || targetKind == SkillTarget.Unknown))
            {
                var enemigo = esJ1 ? tablero.J2 : tablero.J1;
                var objetivoDrain = SelectorObjetivo.ElegirEnemigoActivo(_vista, enemigo, unidadActual.Nombre);
                if (objetivoDrain is null) return IManejoAccion.ActionResult.Continuar();

                unidadActual.GastarMp(costoMp);

                var resultadoDrain = _calcularDano.DanoHabilidad(unidadActual, objetivoDrain, nombre, elemento, poder);
                var outcomeDrain = resultadoDrain.Outcome.Outcome;
                int drenado = Math.Max(0, resultadoDrain.Damage);
                if (outcomeDrain is HitOutcome.Nullified or HitOutcome.Reflected or HitOutcome.Drained)
                    drenado = 0;

                int mpReducido = Math.Min(objetivoDrain.Mp, drenado);
                if (mpReducido > 0) objetivoDrain.GastarMp(mpReducido);
                UnidadHelper.RecuperarMp(unidadActual, mpReducido);

                var lineas = new List<string>
                {
                    $"{unidadActual.Nombre} drena MP de {objetivoDrain.Nombre}",
                    mpReducido > 0
                        ? $"{objetivoDrain.Nombre} pierde {mpReducido} de MP"
                        : $"{objetivoDrain.Nombre} resiste el drenaje de MP",
                    $"{objetivoDrain.Nombre} termina con MP:{objetivoDrain.Mp}/{objetivoDrain.MpMax}",
                    $"{unidadActual.Nombre} termina con MP:{unidadActual.Mp}/{unidadActual.MpMax}"
                };
                _vista.ShowMessage(string.Join("\n", lineas));

                var outcomesDrain = new List<HitOutcome> { outcomeDrain };
                var (usarFullDrain, usarBlinkDrain, ganarBlinkDrain) = TurnosHelper.DecidirConsumo(turnos, outcomesDrain);
                TurnosHelper.AplicarConsumo(turnos, usarFullDrain, usarBlinkDrain, ganarBlinkDrain);
                _vista.ShowTurnsConsumption(usarFullDrain, usarBlinkDrain, ganarBlinkDrain);

                _contadorK.Inc(esJ1);
                return IManejoAccion.ActionResult.Continuar();
            }

            bool esSingleTarget = targetKind == SkillTarget.Single
                                   || targetKind == SkillTarget.Unknown
                                   || targetKind == SkillTarget.SingleAlly;

            if (esSingleTarget)
            {
                var enemigo = esJ1 ? tablero.J2 : tablero.J1;
                var objetivo = SelectorObjetivo.ElegirEnemigoActivo(_vista, enemigo, unidadActual.Nombre);
                if (objetivo is null) return IManejoAccion.ActionResult.Continuar();

                unidadActual.GastarMp(costoMp);

                var outcomes = new List<HitOutcome>();
                var danos = new List<int>();
                RealizarDano.HacerAtaqueMultiHit(
                    _vista, _calcularDano, tablero, esJ1, unidadActual, objetivo,
                    nombre, elemento, poder, cantidadGolpes, outcomes, danos);

                var danosPorObjetivo = new Dictionary<IUnidad, int>();
                int totalDano = danos.Sum();
                if (totalDano > 0) danosPorObjetivo[objetivo] = totalDano;
                AplicarDrain(drainKind, unidadActual, danosPorObjetivo);

                var (usarFull2, usarBlink2, ganarBlink2) = TurnosHelper.DecidirConsumo(turnos, outcomes);
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

            var ordenObjetivos = ConstruirOrdenObjetivos(tablero, esJ1, unidadActual);
            var objetivos = SeleccionarObjetivos(targetKind, ordenObjetivos);
            if (objetivos.Count == 0)
            {
                _vista.ShowMessage("No hay objetivos.");
                return IManejoAccion.ActionResult.Continuar();
            }

            var hitsPorObjetivo = DistribuirGolpes(objetivos, targetKind, cantidadGolpes, desplazamiento);
            if (!hitsPorObjetivo.Values.Any(v => v > 0))
            {
                _vista.ShowMessage("No hay objetivos.");
                return IManejoAccion.ActionResult.Continuar();
            }

            unidadActual.GastarMp(costoMp);

            var resultados = new List<HitOutcome>();
            var danosTotales = new List<int>();
            var danoPorObjetivo = new Dictionary<IUnidad, int>();
            bool esPrimero = true;
            var ultimoReflector = DeterminarUltimoReflector(objetivos, hitsPorObjetivo, elemento);

            foreach (var objetivo in objetivos)
            {
                if (!hitsPorObjetivo.TryGetValue(objetivo, out int hits) || hits <= 0) continue;

                int indiceInicio = danosTotales.Count;
                bool objetivoEsRival = PerteneceA(tablero.Rival(esJ1), objetivo);
                bool parametroEsJ1 = objetivoEsRival ? esJ1 : !esJ1;

                RealizarDano.HacerAtaqueMultiHit(
                    _vista, _calcularDano, tablero, parametroEsJ1, unidadActual, objetivo,
                    nombre, elemento, poder, hits, resultados, danosTotales,
                    showSeparator: esPrimero,
                    mostrarHpReflejo: ReferenceEquals(objetivo, ultimoReflector));

                esPrimero = false;

                int danoAcumulado = danosTotales.Skip(indiceInicio).Sum();
                if (danoAcumulado > 0)
                {
                    if (danoPorObjetivo.TryGetValue(objetivo, out var acumulado))
                        danoPorObjetivo[objetivo] = acumulado + danoAcumulado;
                    else
                        danoPorObjetivo[objetivo] = danoAcumulado;
                }
            }

            AplicarDrain(drainKind, unidadActual, danoPorObjetivo);

            var (usarFullMulti, usarBlinkMulti, ganarBlinkMulti) = TurnosHelper.DecidirConsumo(turnos, resultados);
            TurnosHelper.AplicarConsumo(turnos, usarFullMulti, usarBlinkMulti, ganarBlinkMulti);
            _vista.ShowTurnsConsumption(usarFullMulti, usarBlinkMulti, ganarBlinkMulti);

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

        private static DrainKind ClasificarDrain(string efecto)
        {
            if (string.IsNullOrWhiteSpace(efecto)) return DrainKind.None;

            var texto = efecto.ToLowerInvariant().Replace('’', '\'');
            bool incluyeHp = texto.Contains("drains the enemy's hp") || texto.Contains("hp/mp");
            bool incluyeMp = texto.Contains("drains the enemy's mp") || texto.Contains("hp/mp");

            return (incluyeHp, incluyeMp) switch
            {
                (true, true)   => DrainKind.HpMp,
                (true, false)  => DrainKind.Hp,
                (false, true)  => DrainKind.Mp,
                _              => DrainKind.None
            };
        }

        private static List<(IUnidad unidad, TargetGroup grupo)> ConstruirOrdenObjetivos(Tablero tablero, bool esJ1, IUnidad actual)
        {
            var lista = new List<(IUnidad, TargetGroup)>();
            var rival = tablero.Rival(esJ1);
            lista.AddRange(rival.Unidades.Take(Equipo.ActiveSlots)
                .Where(u => u != null && u.EstaViva)
                .Select(u => (u!, TargetGroup.EnemyField)));

            lista.AddRange(rival.Unidades.Skip(Equipo.ActiveSlots)
                .Where(u => u != null && u.EstaViva)
                .OrderBy(rival.OrdenEnArchivoDe)
                .Select(u => (u!, TargetGroup.EnemyReserve)));

            var propio = tablero.Propio(esJ1);
            lista.AddRange(propio.Unidades.Take(Equipo.ActiveSlots)
                .Where(u => u != null && !ReferenceEquals(u, actual) && u.EstaViva)
                .Select(u => (u!, TargetGroup.AllyField)));

            lista.AddRange(propio.Unidades.Skip(Equipo.ActiveSlots)
                .Where(u => u != null && u.EstaViva)
                .OrderBy(propio.OrdenEnArchivoDe)
                .Select(u => (u!, TargetGroup.AllyReserve)));

            if (actual.EstaViva)
                lista.Add((actual, TargetGroup.Self));

            return lista;
        }

        private static List<IUnidad> SeleccionarObjetivos(SkillTarget target, List<(IUnidad unidad, TargetGroup grupo)> orden)
        {
            return target switch
            {
                SkillTarget.AllEnemies => orden
                    .Where(t => t.grupo is TargetGroup.EnemyField or TargetGroup.EnemyReserve)
                    .Select(t => t.unidad)
                    .ToList(),
                SkillTarget.MultiEnemies => orden
                    .Where(t => t.grupo is TargetGroup.EnemyField or TargetGroup.EnemyReserve)
                    .Select(t => t.unidad)
                    .ToList(),
                SkillTarget.Universal => orden.Select(t => t.unidad).ToList(),
                SkillTarget.Party => orden
                    .Where(t => t.grupo is TargetGroup.AllyField or TargetGroup.AllyReserve or TargetGroup.Self)
                    .Select(t => t.unidad)
                    .ToList(),
                _ => orden
                    .Where(t => t.grupo is TargetGroup.EnemyField or TargetGroup.EnemyReserve)
                    .Select(t => t.unidad)
                    .ToList()
            };
        }

        private static Dictionary<IUnidad, int> DistribuirGolpes(IReadOnlyList<IUnidad> objetivos, SkillTarget target, int totalGolpes, int desplazamiento)
        {
            var distribucion = objetivos.ToDictionary(obj => obj, _ => 0);
            if (objetivos.Count == 0) return distribucion;

            switch (target)
            {
                case SkillTarget.MultiEnemies:
                {
                    int inicio = desplazamiento % Math.Max(1, objetivos.Count);
                    for (int i = 0; i < totalGolpes; i++)
                    {
                        var objetivo = objetivos[(inicio + i) % objetivos.Count];
                        distribucion[objetivo]++;
                    }
                    break;
                }
                case SkillTarget.AllEnemies:
                case SkillTarget.Universal:
                    foreach (var objetivo in objetivos)
                        distribucion[objetivo] = 1;
                    break;
                default:
                    foreach (var objetivo in objetivos)
                        distribucion[objetivo] = Math.Max(1, totalGolpes);
                    break;
            }

            return distribucion;
        }

        private static IUnidad? DeterminarUltimoReflector(IEnumerable<IUnidad> objetivos, IDictionary<IUnidad, int> hitsPorObjetivo, Elemento elemento)
        {
            IUnidad? ultimo = null;
            foreach (var objetivo in objetivos)
            {
                if (!hitsPorObjetivo.TryGetValue(objetivo, out var hits) || hits <= 0) continue;
                if (objetivo.Affinities.AfinidadDe(elemento) == Afinidad.Repel)
                    ultimo = objetivo;
            }
            return ultimo;
        }

        private static bool PerteneceA(Equipo equipo, IUnidad unidad) =>
            equipo.Unidades.Any(u => ReferenceEquals(u, unidad));

        private static void AplicarDrain(DrainKind drainKind, IUnidad usuario, IDictionary<IUnidad, int> danosPorObjetivo)
        {
            if (drainKind == DrainKind.None || danosPorObjetivo.Count == 0) return;

            if (drainKind is DrainKind.Hp or DrainKind.HpMp)
            {
                int total = danosPorObjetivo.Values.Sum();
                if (total > 0) UnidadHelper.CurarUnidad(usuario, total);
            }

            if (drainKind is DrainKind.Mp or DrainKind.HpMp)
            {
                int totalMp = 0;
                foreach (var kvp in danosPorObjetivo)
                {
                    int cantidad = Math.Max(0, kvp.Value);
                    int reducible = Math.Min(kvp.Key.Mp, cantidad);
                    if (reducible <= 0) continue;
                    kvp.Key.GastarMp(reducible);
                    totalMp += reducible;
                }
                if (totalMp > 0) UnidadHelper.RecuperarMp(usuario, totalMp);
            }
        }

        private enum DrainKind { None, Hp, Mp, HpMp }

        private enum TargetGroup
        {
            EnemyField,
            EnemyReserve,
            AllyField,
            AllyReserve,
            Self
        }
    }
}
