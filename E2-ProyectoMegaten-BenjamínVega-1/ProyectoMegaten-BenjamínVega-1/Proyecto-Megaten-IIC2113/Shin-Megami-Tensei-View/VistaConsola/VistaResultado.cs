using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Combate;

namespace Shin_Megami_Tensei_View
{
    internal sealed class VistaResultado
    {
        private readonly ConsoleIO _io;

        public VistaResultado(ConsoleIO io) => _io = io;

        public void MostrarResultadoAtaque(string atk, string tgt, int dmg, int hpRestante, int hpMax)
        {
            _io.SepLine();
            _io.WL($"{atk} ataca a {tgt}");
            _io.WL($"{tgt} recibe {dmg} de daño");
            _io.WL($"{tgt} termina con HP:{hpRestante}/{hpMax}");
        }

        public void MostrarResultadoDisparo(string atk, string tgt, int dmg, int hpRestante, int hpMax)
        {
            _io.SepLine();
            _io.WL($"{atk} dispara a {tgt}");
            _io.WL($"{tgt} recibe {dmg} de daño");
            _io.WL($"{tgt} termina con HP:{hpRestante}/{hpMax}");
        }

        public void MostrarConsumoTurnos(int usedFull, int usedBlink, int gainedBlink)
        {
            _io.SepLine();
            _io.WL($"Se han consumido {usedFull} Full Turn(s) y {usedBlink} Blinking Turn(s)");
            _io.WL($"Se han obtenido {gainedBlink} Blinking Turn(s)");
        }

        public void MostrarRendicion(string nombre, string tag)
        {
            _io.SepLine();
            _io.WL($"{nombre} ({tag}) se rinde");
        }

        public void MostrarGanador(string nombre, string tag)
        {
            _io.SepLine();
            _io.WL($"Ganador: {nombre} ({tag})");
        }

        public void MostrarMensaje(string message)
        {
            _io.SepLine();
            if (!string.IsNullOrEmpty(message))
                _io.WL(message);
        }

        public void MostrarResultadoHabilidad(
            string usuario,
            string skill,
            string objetivo,
            int dano,
            int hpFinal,
            int hpMax,
            string? notaAfinidad = null)
        {
            _io.SepLine();
            _io.WL($"{usuario} usa {skill} sobre {objetivo}");
            _io.WL($"{objetivo} recibe {dano} de daño");
            _io.WL($"{objetivo} termina con HP:{hpFinal}/{hpMax}");
            if (!string.IsNullOrWhiteSpace(notaAfinidad))
                _io.WL($"[{notaAfinidad}]");
        }

        public void MostrarResultadoAfinidad(
            string atacante,
            string objetivo,
            Elemento elemento,
            HitOutcome outcome,
            int dano,
            int hpFinal,
            int hpMax,
            bool objetivoEliminado = false)
        {
            if (hpMax == -2) _io.SepLine();

            string accion = elemento switch
            {
                Elemento.Phys  => "ataca",
                Elemento.Gun   => "dispara",
                Elemento.Fire  => "lanza fuego",
                Elemento.Ice   => "lanza hielo",
                Elemento.Elec  => "lanza electricidad",
                Elemento.Force => "lanza viento",
                Elemento.Light => "ataca con luz",
                Elemento.Dark  => "ataca con oscuridad",
                _              => "ataca"
            };
            _io.WL($"{atacante} {accion} a {objetivo}");

            switch (outcome)
            {
                case HitOutcome.Weak:
                    _io.WL($"{objetivo} es débil contra el ataque de {atacante}");
                    break;

                case HitOutcome.Resist:
                    _io.WL($"{objetivo} es resistente el ataque de {atacante}");
                    break;

                case HitOutcome.Nullified:
                    _io.WL($"{objetivo} bloquea el ataque de {atacante}");
                    if (hpMax == -1) return;
                    _io.WL($"{objetivo} termina con HP:{hpFinal}/{hpMax}");
                    return;

                case HitOutcome.Reflected:
                    _io.WL($"{objetivo} devuelve {dano} daño a {atacante}");
                    if (hpMax == -1) return;
                    _io.WL($"{atacante} termina con HP:{hpFinal}/{hpMax}");
                    return;

                case HitOutcome.Drained:
                    _io.WL($"{objetivo} absorbe {dano} daño");
                    if (hpMax == -1) return;
                    _io.WL($"{objetivo} termina con HP:{hpFinal}/{hpMax}");
                    return;
            }

            var esOHKO = (elemento == Elemento.Light || elemento == Elemento.Dark);
            if (objetivoEliminado && esOHKO)
            {
                _io.WL($"{objetivo} ha sido eliminado");
                _io.WL($"{objetivo} termina con HP:0/{hpMax}");
                return;
            }

            _io.WL($"{objetivo} recibe {dano} de daño");
            if (hpMax == -1) return;
            _io.WL($"{objetivo} termina con HP:{hpFinal}/{hpMax}");
        }
    }
}
