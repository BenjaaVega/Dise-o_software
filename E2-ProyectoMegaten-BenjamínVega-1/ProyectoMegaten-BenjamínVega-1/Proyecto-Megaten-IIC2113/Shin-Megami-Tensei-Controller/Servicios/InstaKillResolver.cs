using System;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo.Services;

public static class InstaKillResolver
{
    public static bool EsInstaKill(Elemento element, int hits)
        => (element == Elemento.Light || element == Elemento.Dark) && hits <= 1;

    public static long CalcularPuntaje(IUnidad atacante, int power)
    {
        var suerte = Math.Max(0, atacante.Stats.Lck);
        var poder  = Math.Max(0, power);
        return suerte + poder;
    }

    public static double CalcularUmbral(IUnidad objetivo, Elemento element)
    {
        double umbral = objetivo.Stats.Lck;
        var afinidad  = objetivo.Affinities.AfinidadDe(element);

        umbral = afinidad switch
        {
            Afinidad.Weak   => Math.Max(1.0, umbral / 2.0),
            Afinidad.Resist => umbral * 2.0,
            _               => umbral
        };

        return Math.Max(1.0, umbral);
    }
}
