using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Combate;

public interface ICalcularDano
{
    double DanoAtaqueBase(IUnidad atacante, IUnidad objetivo);
    double DanoDisparoBase(IUnidad atacante, IUnidad objetivo);
    

    public sealed class DanoResultado
    {
        public int Damage { get; }
        public ResultadoAfinidad Outcome { get; }

        public DanoResultado(int damage, ResultadoAfinidad outcome)
        {
            Damage = damage;
            Outcome = outcome;
        }
    }
}