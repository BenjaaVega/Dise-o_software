using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Combate;

public static class ResolverAfinidad
{
    public static ResultadoAfinidad Resolve(Elemento atk, Afinidad def)
    {
        return def switch
        {
            Afinidad.Weak   => new ResultadoAfinidad(HitOutcome.Weak,       1.5, EfectoTruno.GainBlinking()),
            Afinidad.Resist => new ResultadoAfinidad(HitOutcome.Resist,     0.5, EfectoTruno.ConsumeFull()),
            Afinidad.Null   => new ResultadoAfinidad(HitOutcome.Nullified,  0.0, EfectoTruno.ConsumeFull()),
            Afinidad.Repel  => new ResultadoAfinidad(HitOutcome.Reflected,  0.0, EfectoTruno.ConsumeFull()),
            Afinidad.Drain  => new ResultadoAfinidad(HitOutcome.Drained,    0.0, EfectoTruno.ConsumeFull()),
            _               => new ResultadoAfinidad(HitOutcome.Damage,     1.0, EfectoTruno.ConsumeFull())
        };
    }
}