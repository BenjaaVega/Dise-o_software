using System.Reflection;
using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei.Manejo.Helpers
{
    public static class UnidadHelper
    {
        public static void SetHp(IUnidad unidad, int hp)
        {
            var pHp = unidad.GetType().GetProperty("Hp");
            if (pHp is not null && pHp.CanWrite)
                pHp.SetValue(unidad, Math.Max(0, Math.Min(hp, unidad.HpMax)));
        }

        public static void CurarUnidad(IUnidad unidad, int amount)
        {
            if (amount <= 0 || unidad == null) return;

            var metodo = unidad.GetType().GetMethod("Curar", new[] { typeof(int) });
            if (metodo != null)
            {
                metodo.Invoke(unidad, new object[] { amount });
                return;
            }

            var pHp = unidad.GetType().GetProperty("Hp");
            if (pHp != null && pHp.CanWrite)
            {
                int nuevo = Math.Min(unidad.Hp + amount, unidad.HpMax);
                pHp.SetValue(unidad, nuevo);
            }
        }

        public static void RevivirFull(IUnidad u)
        {
            var pHp = u.GetType().GetProperty("Hp", BindingFlags.Public | BindingFlags.Instance);
            var pHpMax = u.GetType().GetProperty("HpMax", BindingFlags.Public | BindingFlags.Instance);
            if (pHp is not null && pHp.CanWrite && pHpMax is not null)
            {
                int hpMax = (int)(pHpMax.GetValue(u) ?? 0);
                pHp.SetValue(u, Math.Max(1, hpMax));
            }

            var mCurar = u.GetType().GetMethod("Curar", new[] { typeof(int) });
            if (mCurar is not null)
            {
                int falta = Math.Max(0, u.HpMax - u.Hp);
                if (falta > 0) mCurar.Invoke(u, new object[] { falta });
            }
        }

        public static void RecuperarMp(IUnidad unidad, int cantidad)
        {
            if (unidad is null || cantidad <= 0) return;

            var pMp = unidad.GetType().GetProperty("Mp", BindingFlags.Public | BindingFlags.Instance);
            if (pMp is not null && pMp.CanWrite)
            {
                int actual = unidad.Mp;
                int nuevo = Math.Min(unidad.MpMax, actual + cantidad);
                pMp.SetValue(unidad, nuevo);
            }
        }
    }
}