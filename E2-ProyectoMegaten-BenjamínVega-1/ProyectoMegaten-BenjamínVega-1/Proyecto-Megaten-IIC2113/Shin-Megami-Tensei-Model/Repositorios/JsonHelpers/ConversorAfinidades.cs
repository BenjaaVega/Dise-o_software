using Shin_Megami_Tensei_Model.Combate;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Repositorios.DTO;

namespace Shin_Megami_Tensei_Model.Repositorios.Ayudantes
{
    internal static class ConversorAfinidades
    {
        public static SetAfinidad ConvertirASet(BloqueAfinidades a) => new(
            Parse(a.Phys), Parse(a.Gun), Parse(a.Fire), Parse(a.Ice),
            Parse(a.Elec), Parse(a.Force), Parse(a.Light), Parse(a.Dark)
        );

        public static Afinidad Parse(string? code) => (code ?? "-").Trim() switch
        {
            "Wk" => Afinidad.Weak,
            "Rs" => Afinidad.Resist,
            "Nu" => Afinidad.Null,
            "Rp" => Afinidad.Repel,
            "Dr" => Afinidad.Drain,
            "-"  => Afinidad.Neutral,
            _    => Afinidad.Neutral
        };
    }
}