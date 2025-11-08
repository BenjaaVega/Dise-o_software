using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Repositorios
{
    public interface ISkillsLookup
    {
        bool Existe(string nombre);
        int? CostoMp(string nombre);
        IEnumerable<string> Todas();

        Elemento? ElementoDe(string nombre);
        int? PowerDe(string nombre);
        
        string? HitsDe(string nombre);    
        string? EffectDe(string nombre);   
    }
}