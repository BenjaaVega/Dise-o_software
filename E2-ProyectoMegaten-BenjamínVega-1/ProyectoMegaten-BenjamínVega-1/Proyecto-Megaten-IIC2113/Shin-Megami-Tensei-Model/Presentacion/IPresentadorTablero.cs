using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;

namespace Shin_Megami_Tensei_Model.Presentacion;

public interface IBoardPresenter
{
    SnapshotTablero BoardOf(Tablero t);
    SnapshotTurno TurnsOf(Turnos t);

}