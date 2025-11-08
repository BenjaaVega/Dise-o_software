using Shin_Megami_Tensei_Model.Presentacion.ViewModels;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using System.Collections.Generic;

namespace Shin_Megami_Tensei_View;

public interface IVistaJuego
{
    void ShowTeamFiles(IReadOnlyList<string> archivos);
    int  ReadFileIndex();
    void ShowInvalidTeamsAndExit();
    
    void ShowRoundHeader(string samuraiNombre, string jugadorTag);
    void ShowBoard(SnapshotTablero snapshot);
    void ShowTurns(SnapshotTurno snapshot);
    void ShowOrder(IReadOnlyList<string> ordenNumerado);

    int  ReadActionFor(string unidadNombre, MenuAcciones menu);
    int  ReadTarget(string atacanteNombre, MenuObjetivo menu);
    int  ReadSkill(string unidadNombre, SkillMenu menu);
    void ShowAttackResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax);
    void ShowShotResolution(string atacante, string objetivo, int dano, int hpFinal, int hpMax);
    void ShowTurnsConsumption(int fullUsed, int blinkingUsed, int blinkingGained);

    void ShowSurrender(string nombre, string tag);
    void ShowWinner(string nombre, string tag);
    
    void ShowMessage(string message);

    void ShowSkillResolution(
        string usuario,
        string skill,
        string objetivo,
        int dano,
        int hpFinal,
        int hpMax,
        string? notaAfinidad = null
    );
    
    void ShowElementalResolution(
        string atacante,
        string objetivo,
        Shin_Megami_Tensei_Model.ModelosEquipo.Elemento elemento,
        Shin_Megami_Tensei_Model.Combate.HitOutcome outcome,
        int dano,
        int hpFinal,
        int hpMax,
        bool objetivoEliminado = false
    );
    
    int ReadCustom(string header, IReadOnlyList<string> opciones);
}
