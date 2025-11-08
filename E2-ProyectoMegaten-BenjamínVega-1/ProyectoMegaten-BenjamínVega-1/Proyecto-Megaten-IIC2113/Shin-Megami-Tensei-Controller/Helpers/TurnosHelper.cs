using Shin_Megami_Tensei_Model.Combate;

namespace Shin_Megami_Tensei.Manejo.Helpers
{
    public static class TurnosHelper
    {
        public static (int usedFull, int usedBlink, int gainedBlink) DecidirConsumo(Turnos turnos, HitOutcome outcome)
        {
            int usedFull = 0, usedBlink = 0, gainedBlink = 0;
            int full = turnos.Full;
            int blink = turnos.Blinking;

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
    }
}