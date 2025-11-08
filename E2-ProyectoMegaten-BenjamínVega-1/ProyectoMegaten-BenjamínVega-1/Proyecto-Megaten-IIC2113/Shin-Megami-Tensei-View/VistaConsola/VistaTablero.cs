using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;

namespace Shin_Megami_Tensei_View
{
    internal sealed class VistaTablero
    {
        private readonly ConsoleIO _io;

        public VistaTablero(ConsoleIO io) => _io = io;

        public void MostrarInicioRonda(string samuraiNombre, string tag)
        {
            _io.SepLine();
            _io.WL($"Ronda de {samuraiNombre} ({tag})");
        }

        public void MostrarTablero(SnapshotTablero snapshot)
        {
            _io.SepLine();
            _io.WL($"Equipo de {snapshot.J1Nombre} (J1)");
            ImprimirSlots(snapshot.J1Slots);

            _io.WL($"Equipo de {snapshot.J2Nombre} (J2)");
            ImprimirSlots(snapshot.J2Slots);
        }

        public void MostrarTurnos(SnapshotTurno t)
        {
            _io.SepLine();
            _io.WL($"Full Turns: {t.Full}");
            _io.WL($"Blinking Turns: {t.Blinking}");
        }

        public void MostarOrden(IReadOnlyList<string> ordenNumerado)
        {
            _io.SepLine();
            _io.WL("Orden:");
            var lista = ordenNumerado ?? Array.Empty<string>();
            foreach (var linea in lista) _io.WL(linea);
        }

        private void ImprimirSlots(IReadOnlyList<Slot> slots)
        {
            var xs = slots ?? Array.Empty<Slot>();
            foreach (var sl in xs)
            {
                if (sl.Nombre is null) _io.WL($"{sl.Letra}-");
                else _io.WL($"{sl.Letra}-{sl.Nombre} HP:{sl.Hp}/{sl.HpMax} MP:{sl.Mp}/{sl.MpMax}");
            }
        }
    }
}