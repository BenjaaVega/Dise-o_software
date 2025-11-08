using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Repositorios;

public interface IRepositorioUnidades
{
    IUnidad CrearSamurai(string nombre, IEnumerable<string> skills);
    IUnidad CrearMonstruo(string nombre);
}