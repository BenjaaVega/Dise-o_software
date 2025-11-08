using Shin_Megami_Tensei_Model.ModelosEquipo;
using System;

namespace Shin_Megami_Tensei_Model.Combate;

public sealed class CalcularDano : ICalcularDano
{
    private const double K = 0.0114;
    private const int ModAtacar  = 54;
    private const int ModDisparo = 80;

    public double DanoAtaqueBase(IUnidad atacante, IUnidad objetivo)
    {
        return atacante.Stats.Str * ModAtacar * K;
    }

    public double DanoDisparoBase(IUnidad atacante, IUnidad objetivo)
    {
        return atacante.Stats.Skl * ModDisparo * K;
    }

    public int DanoAtaque(IUnidad atacante, IUnidad objetivo)
    {
        double v = DanoAtaqueBase(atacante, objetivo);
        return Math.Max(0, (int)Math.Floor(v));
    }

    public int DanoDisparo(IUnidad atacante, IUnidad objetivo)
    {
        double v = DanoDisparoBase(atacante, objetivo);
        return Math.Max(0, (int)Math.Floor(v));
    }
    
    public ICalcularDano.DanoResultado DanoHabilidad(IUnidad atacante, IUnidad objetivo, string skillName, Elemento elem, int power)
    {
        int stat = elem switch
        {
            Elemento.Phys     => atacante.Stats.Str,
            Elemento.Gun      => atacante.Stats.Skl,
            Elemento.Almighty => atacante.Stats.Mag,
            _                 => atacante.Stats.Mag
        };
        
        double raw = Math.Sqrt(Math.Max(0, stat * Math.Max(0, power)));

        var afinidad        = objetivo.Affinities.AfinidadDe(elem);
        var resultadoAfinidad     = ResolverAfinidad.Resolve(elem, afinidad);
        var hitResultado = resultadoAfinidad.Outcome;
        
        double mult = hitResultado switch
        {
            HitOutcome.Weak      => 1.5,
            HitOutcome.Resist    => 0.5,
            HitOutcome.Nullified => 0.0,
            HitOutcome.Reflected => 0.0,
            HitOutcome.Drained   => 0.0,
            _                    => 1.0
        };

        double scaled = raw * mult;
        int final = Math.Max(0, (int)Math.Floor(scaled));

        return new ICalcularDano.DanoResultado(final, resultadoAfinidad);
    }
}
