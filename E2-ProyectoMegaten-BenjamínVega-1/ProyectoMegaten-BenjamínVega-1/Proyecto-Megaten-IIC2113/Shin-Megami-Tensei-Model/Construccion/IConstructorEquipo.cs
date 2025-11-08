using Shin_Megami_Tensei_Model.Entrada;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Construccion;

public interface IConstructorEquipo
{
    Equipo Construir(IReadOnlyList<IntentoUnidad> crudo);
}

