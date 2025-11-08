using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.Presentacion.ViewModels;

namespace Shin_Megami_Tensei_View
{
    public sealed class VistaJuegoConsola : IVistaJuego
    {
        private readonly ConsoleIO _io;
        private readonly VistaArchivoEquipo _files;
        private readonly VistaTablero _board;
        private readonly VistaMenu _menus;
        private readonly VistaResultado _res;

        public VistaJuegoConsola(dynamic view)
        {
            _io = new ConsoleIO(view);
            _files = new VistaArchivoEquipo(_io);
            _board = new VistaTablero(_io);
            _menus = new VistaMenu(_io);
            _res   = new VistaResultado(_io);
        }

        public void ShowTeamFiles(IReadOnlyList<string> archivos) => _files.MostrarArchivosEquipo(archivos);
        public int  ReadFileIndex() => _files.LeerArchivo();
        public void ShowInvalidTeamsAndExit() => _files.MostrarEquipoInvalido();
        
        public void ShowRoundHeader(string samuraiNombre, string tag) => _board.MostrarInicioRonda(samuraiNombre, tag);
        public void ShowBoard(SnapshotTablero snapshot) => _board.MostrarTablero(snapshot);
        public void ShowTurns(SnapshotTurno snapshot) => _board.MostrarTurnos(snapshot);
        public void ShowOrder(IReadOnlyList<string> ordenNumerado) => _board.MostarOrden(ordenNumerado);
        
        public int ReadActionFor(string unidadNombre, MenuAcciones menu) => _menus.LeerAccion(unidadNombre, menu);
        public int ReadTarget(string atacanteNombre, MenuObjetivo menu) => _menus.LeerObjetivo(atacanteNombre, menu);
        public int ReadSkill(string unidadNombre, SkillMenu menu) => _menus.LeerHabilidad(unidadNombre, menu);
        public int ReadCustom(string header, IReadOnlyList<string> opciones) => _menus.LeerRandom(header, opciones);
        
        public void ShowAttackResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax) =>
            _res.MostrarResultadoAtaque(atacante, objetivo, dano, hpFinal, hpMax);

        public void ShowShotResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax) =>
            _res.MostrarResultadoDisparo(atacante, objetivo, dano, hpFinal, hpMax);

        public void ShowTurnsConsumption(int fullUsed, int blinkingUsed, int blinkingGained) =>
            _res.MostrarConsumoTurnos(fullUsed, blinkingUsed, blinkingGained);

        public void ShowSurrender(string nombre, string tag) => _res.MostrarRendicion(nombre, tag);

        public void ShowWinner(string nombre, string tag) => _res.MostrarGanador(nombre, tag);

        public void ShowMessage(string message) => _res.MostrarMensaje(message);

        public void ShowSkillResolution(
            string usuario,
            string skill,
            string objetivo,
            int dano,
            int hpFinal,
            int hpMax,
            string? notaAfinidad = null) =>
            _res.MostrarResultadoHabilidad(usuario, skill, objetivo, dano, hpFinal, hpMax, notaAfinidad);

        public void ShowElementalResolution(
            string atacante,
            string objetivo,
            Elemento elemento,
            HitOutcome outcome,
            int dano,
            int hpFinal,
            int hpMax,
            bool objetivoEliminado = false) =>
            _res.MostrarResultadoAfinidad(atacante, objetivo, elemento, outcome, dano, hpFinal, hpMax, objetivoEliminado);
    }
}
