using Shin_Megami_Tensei_Model.Entrada;
using Shin_Megami_Tensei_Model.ModelosEquipo;
using Shin_Megami_Tensei_Model.Repositorios;

namespace Shin_Megami_Tensei_Model.Construccion;

public sealed class ContructorEquipo : IConstructorEquipo
{
    private readonly IRepositorioUnidades _repositorioUnidades;

    public ContructorEquipo(IRepositorioUnidades repositorioUnidades) =>
        _repositorioUnidades = repositorioUnidades;

    public Equipo Construir(IReadOnlyList<IntentoUnidad> unidadesCrudas)
    {
        var unidades = new List<IUnidad>();
        foreach (var intento in unidadesCrudas)
        {
            unidades.Add(CrearUnidad(intento));
        }
        return new Equipo(unidades);
    }

    private IUnidad CrearUnidad(IntentoUnidad intento) =>
        intento.EsSamurai
            ? _repositorioUnidades.CrearSamurai(intento.Nombre, intento.Skills)
            : _repositorioUnidades.CrearMonstruo(intento.Nombre);
}