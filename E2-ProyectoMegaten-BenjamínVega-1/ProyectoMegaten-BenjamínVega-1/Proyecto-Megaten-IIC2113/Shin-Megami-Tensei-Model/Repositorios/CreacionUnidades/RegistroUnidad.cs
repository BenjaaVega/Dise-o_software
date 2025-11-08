using System.Collections.Generic;
using Shin_Megami_Tensei_Model.Repositorios.DTO;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Repositorios.Modelos
{
    internal sealed class RegistroUnidad
    {
        public Stats Stats { get; set; } = null!;
        public BloqueAfinidades? Affinities { get; set; }
        public List<string>? Skills { get; set; }
    }
}