using System.Collections.Generic;
using Shin_Megami_Tensei_Model.Combate;

namespace Shin_Megami_Tensei.Manejo.Helpers
{
    public static class TurnosHelper
    {
        public static (int usedFull, int usedBlink, int gainedBlink) DecidirConsumo(Turnos turnos, HitOutcome outcome)
        {
            return DecidirConsumoConSaldo(turnos.Full, turnos.Blinking, outcome);
        }

        public static (int usedFull, int usedBlink, int gainedBlink) DecidirConsumo(Turnos turnos, IReadOnlyList<HitOutcome> outcomes)
        {
            if (outcomes is null || outcomes.Count == 0) return (0, 0, 0);

            var outcome = ConsolidarResultados(outcomes);
            return DecidirConsumoConSaldo(turnos.Full, turnos.Blinking, outcome);
        }

        private static (int usedFull, int usedBlink, int gainedBlink) DecidirConsumoConSaldo(int full, int blink, HitOutcome outcome)
        {
            int usedFull = 0, usedBlink = 0, gainedBlink = 0;

            switch (outcome)
            {
                case HitOutcome.Damage:
                case HitOutcome.Resist:
                    if (blink > 0) usedBlink = 1; else usedFull = Math.Min(1, full);
                    break;

                case HitOutcome.Weak:
                    if (full > 0) { usedFull = 1; gainedBlink = 1; }
                    else if (blink > 0) { usedBlink = 1; }
                    else { usedFull = Math.Min(1, full); gainedBlink = 1; }
                    break;

                case HitOutcome.Nullified:
                {
                    int needBlink = 2;
                    int takeBlink = Math.Min(blink, needBlink);
                    usedBlink = takeBlink;
                    int needFull = needBlink - takeBlink;
                    usedFull = Math.Min(needFull, full);
                    break;
                }

                case HitOutcome.Reflected:
                case HitOutcome.Drained:
                    usedBlink = blink;
                    usedFull  = full;
                    break;

                default:
                    if (blink > 0) usedBlink = 1; else usedFull = Math.Min(1, full);
                    break;
            }

            return (usedFull, usedBlink, gainedBlink);
        }

        public static void AplicarConsumo(Turnos turnos, int usedFull, int usedBlink, int gainedBlink)
        {
            if (usedFull  > 0)  turnos.ConsumirFull(usedFull);
            if (usedBlink > 0)  turnos.ConsumirBlinking(usedBlink);
            if (gainedBlink > 0) turnos.AgregarBlinking(gainedBlink);
        }

        private static HitOutcome ConsolidarResultados(IReadOnlyList<HitOutcome> outcomes)
        {
            bool anyWeak = false;
            bool anyResist = false;
            bool anyNull = false;
            bool anyReflect = false;
            bool anyDrain = false;

            foreach (var outcome in outcomes)
            {
                switch (outcome)
                {
                    case HitOutcome.Weak:
                        anyWeak = true;
                        break;
                    case HitOutcome.Resist:
                        anyResist = true;
                        break;
                    case HitOutcome.Nullified:
                        anyNull = true;
                        break;
                    case HitOutcome.Reflected:
                        anyReflect = true;
                        break;
                    case HitOutcome.Drained:
                        anyDrain = true;
                        break;
                }
            }

            if (anyReflect) return HitOutcome.Reflected;
            if (anyDrain)   return HitOutcome.Drained;
            if (anyNull)    return HitOutcome.Nullified;
            if (anyResist)  return HitOutcome.Resist;
            if (anyWeak)    return HitOutcome.Weak;
            return HitOutcome.Damage;
        }
    }
}

