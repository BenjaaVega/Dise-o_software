namespace Shin_Megami_Tensei.Manejo.Services
{
    public sealed class ContadorK
    {
        private int _kJ1, _kJ2;

        public int Get(bool esJ1) => esJ1 ? _kJ1 : _kJ2;
        public void Inc(bool esJ1)
        {
            if (esJ1) _kJ1++; else _kJ2++;
        }
    }
}