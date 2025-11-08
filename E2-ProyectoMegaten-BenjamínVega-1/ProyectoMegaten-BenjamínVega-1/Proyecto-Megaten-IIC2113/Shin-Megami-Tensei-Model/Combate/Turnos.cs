namespace Shin_Megami_Tensei_Model.Combate
{
    public sealed class Turnos
    {
        public int Full { get; private set; }
        public int Blinking { get; private set; }
        public bool HayTurnos => Full > 0 || Blinking > 0;

        public Turnos(int full, int blinking)
        {
            Full = Math.Max(0, full);
            Blinking = Math.Max(0, blinking);
        }
        public static Turnos DesdeActivos(int activosVivos) => new(activosVivos, 0);
        
        public void ConsumirFull(int n) => Full = Math.Max(0, Full - n);

        public void ConsumirBlinking(int n)
        {
            if (Blinking > 0) Blinking = Math.Max(0, Blinking - n);
            else Full = Math.Max(0, Full - n);
        }

        public void AgregarBlinking(int n) => Blinking += n;
    }
}