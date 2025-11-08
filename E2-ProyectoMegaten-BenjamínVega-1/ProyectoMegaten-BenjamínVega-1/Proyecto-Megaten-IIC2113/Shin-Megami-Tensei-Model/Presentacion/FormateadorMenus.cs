using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Presentacion.ViewModels;
using Shin_Megami_Tensei_Model.Presentacion.VistaModelos;
using ItemObjetivo = Shin_Megami_Tensei_Model.Presentacion.ViewModels.ItemObjetivo;

namespace Shin_Megami_Tensei_Model.Presentacion;

public static class FormateadorMenus
{
    
    public static MenuObjetivo VistaDeObjetivos(IEnumerable<IUnidad> objetivos) =>
        MenuObjetivo.From(
            objetivos
                .Select<IUnidad, ItemObjetivo>(unidad => new ItemObjetivo(unidad.Nombre, unidad.Hp, unidad.HpMax, unidad.Mp, unidad.MpMax))
                .ToList()
        );

    public static SkillMenu MenuHabilidades(IEnumerable<(string nombre, int? costo)> habilidades) =>
        SkillMenu.From(
            habilidades
                .Select(skill => new SkillItem(skill.nombre, skill.costo))
                .ToList()
        );
    
}