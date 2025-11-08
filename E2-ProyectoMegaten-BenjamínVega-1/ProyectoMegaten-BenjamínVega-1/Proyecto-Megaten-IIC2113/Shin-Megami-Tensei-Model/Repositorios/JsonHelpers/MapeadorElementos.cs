using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Repositorios.Ayudantes
{
    internal static class MapeadorElementos
    {
        public static bool IntentarMapear(string? tipoCrudo, out Elemento elemento)
        {
            elemento = Elemento.Phys;
            if (string.IsNullOrWhiteSpace(tipoCrudo)) return false;

            switch (tipoCrudo.Trim().ToLowerInvariant())
            {
                case "phys":  elemento = Elemento.Phys;  return true;
                case "gun":   elemento = Elemento.Gun;   return true;
                case "fire":  elemento = Elemento.Fire;  return true;
                case "ice":   elemento = Elemento.Ice;   return true;
                case "elec":  elemento = Elemento.Elec;  return true;
                case "force": elemento = Elemento.Force; return true;
                case "light": elemento = Elemento.Light; return true;
                case "dark":  elemento = Elemento.Dark;  return true;
                default:      elemento = Elemento.Phys;  return true;
            }
        }
    }
}