namespace Shin_Megami_Tensei_Model.ModelosEquipo
{
    public interface IUnidad
    {
        string Nombre { get; }
        bool EsSamurai { get; }
        int HpMax { get; }
        int MpMax { get; }
        int Hp { get; }
        int Mp { get; }
        Stats Stats { get; }

        SetAfinidad Affinities { get; }

        bool EstaViva => Hp > 0;

        void RecibirDano(int dano);

        void GastarMp(int mp);
    }
}