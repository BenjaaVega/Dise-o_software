using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public static class RealizarDano
    {
        private const int HP_NO_MOSTRAR = -1;

        public static HitOutcome HacerAtaqueBasico(
            IVistaJuego vistaJuego,
            IUnidad actual,
            IUnidad objetivo,
            Elemento elemento,
            double rawBase)
        {
            var afinidad     = objetivo.Affinities.AfinidadDe(elemento);
            var resultadoAfinidad  = ResolverAfinidad.Resolve(elemento, afinidad);
            var outcome = resultadoAfinidad.Outcome;

            double mult = outcome switch
            {
                HitOutcome.Weak      => 1.5,
                HitOutcome.Resist    => 0.5,
                HitOutcome.Nullified => 0.0,
                HitOutcome.Reflected => 0.0,
                HitOutcome.Drained   => 0.0,
                _                    => 1.0
            };

            int neutralSkill = Math.Max(0, (int)Math.Floor(rawBase)); 

            vistaJuego.ShowMessage("");

            int danoParaMostrar;
            int hpFinalMostrar;
            int hpMaxMostrar;

            switch (outcome)
            {
                case HitOutcome.Nullified:
                    danoParaMostrar = 0;
                    hpFinalMostrar  = objetivo.Hp;
                    hpMaxMostrar    = objetivo.HpMax;
                    break;

                case HitOutcome.Reflected:
                    actual.RecibirDano(neutralSkill);
                    danoParaMostrar = neutralSkill;
                    hpFinalMostrar  = actual.Hp;
                    hpMaxMostrar    = actual.HpMax; 
                    break;

                case HitOutcome.Drained:
                {
                    int curar = neutralSkill;
                    var mCurar = objetivo.GetType().GetMethod("Curar", new[] { typeof(int) });
                    if (mCurar is not null) mCurar.Invoke(objetivo, new object[] { curar });
                    else
                    {
                        int hpDespues = Math.Min(objetivo.Hp + curar, objetivo.HpMax);
                        var pHp = objetivo.GetType().GetProperty("Hp");
                        if (pHp is not null && pHp.CanWrite) pHp.SetValue(objetivo, hpDespues);
                    }
                    danoParaMostrar = curar;
                    hpFinalMostrar  = objetivo.Hp;
                    hpMaxMostrar    = objetivo.HpMax;
                    break;
                }

                default:
                {
                    double scaled = rawBase * mult;
                    int final = Math.Max(0, (int)Math.Floor(scaled));
                    objetivo.RecibirDano(final);
                    danoParaMostrar = final;
                    hpFinalMostrar  = objetivo.Hp;
                    hpMaxMostrar    = objetivo.HpMax;
                    break;
                }
            }

            vistaJuego.ShowElementalResolution(
                actual.Nombre, objetivo.Nombre, elemento, outcome,
                danoParaMostrar, hpFinalMostrar, hpMaxMostrar, objetivoEliminado: !objetivo.EstaViva
            );

            return outcome;
        }
        public static HitOutcome HacerAtaqueMultiHit(
            IVistaJuego view,
            CalcularDano calc,
            Tablero t,
            bool esJ1,
            IUnidad actual,
            IUnidad objetivo,
            string skillName,
            Elemento element,
            int power,
            int numHits)
        {
            view.ShowMessage("");
            bool esOhko = element == Elemento.Light || element == Elemento.Dark;

            HitOutcome last = HitOutcome.Damage;
            for (int hit = 1; hit <= numHits; hit++)
            {
                var res = calc.DanoHabilidad(actual, objetivo, skillName, element, power);
                last = res.Outcome.Outcome;

                bool esUltimo = (hit == numHits);

                int danoParaMostrar;
                int hpFinalMostrar;
                int hpMaxMostrar = esUltimo ? objetivo.HpMax : HP_NO_MOSTRAR;

                int stat = element switch
                {
                    Elemento.Phys => actual.Stats.Str,
                    Elemento.Gun  => actual.Stats.Skl,
                    _             => actual.Stats.Mag
                };
                int neutralSkill = Math.Max(0,
                    (int)Math.Floor(Math.Sqrt(Math.Max(0, stat * Math.Max(0, power))))
                );

                switch (last)
                {
                    case HitOutcome.Nullified:
                        danoParaMostrar = 0;
                        hpFinalMostrar  = objetivo.Hp;
                        break;

                    case HitOutcome.Reflected:
                        actual.RecibirDano(neutralSkill);
                        danoParaMostrar = neutralSkill;
                        hpFinalMostrar  = actual.Hp;
                        hpMaxMostrar    = esUltimo ? actual.HpMax : HP_NO_MOSTRAR;
                        break;

                    case HitOutcome.Drained:
                    {
                        int heal = neutralSkill;
                        var mCurar = objetivo.GetType().GetMethod("Curar", new[] { typeof(int) });
                        if (mCurar is not null) mCurar.Invoke(objetivo, new object[] { heal });
                        else
                        {
                            int hpDespues = Math.Min(objetivo.Hp + heal, objetivo.HpMax);
                            var pHp = objetivo.GetType().GetProperty("Hp");
                            if (pHp is not null && pHp.CanWrite) pHp.SetValue(objetivo, hpDespues);
                        }
                        danoParaMostrar = heal;
                        hpFinalMostrar  = objetivo.Hp;
                        break;
                    }

                    default:
                        if (esOhko)
                        {
                            danoParaMostrar = objetivo.Hp;
                            objetivo.RecibirDano(objetivo.Hp);
                            hpFinalMostrar  = 0;
                            hpMaxMostrar    = objetivo.HpMax; 
                        }
                        else
                        {
                            objetivo.RecibirDano(res.Damage);
                            danoParaMostrar = res.Damage;
                            hpFinalMostrar  = objetivo.Hp;
                        }
                        break;
                }

                view.ShowElementalResolution(
                    actual.Nombre, objetivo.Nombre, element, last,
                    danoParaMostrar, hpFinalMostrar, hpMaxMostrar,
                    objetivoEliminado: (esOhko && !objetivo.EstaViva)
                );

                if (!objetivo.EstaViva && !objetivo.EsSamurai)
                {
                    var equipoRival = t.Rival(esJ1);
                    equipoRival.BajarMuertoDeActivosAReserva(objetivo);
                }
            }

            return last;
        }
    }
}
