namespace Shin_Megami_Tensei_Model.Presentacion.ViewModels
{
    public sealed class ItemObjetivo
    {
        public string Nombre { get; }
        public int Hp { get; }
        public int HpMax { get; }
        public int Mp { get; }
        public int MpMax { get; }

        public ItemObjetivo(string nombre, int hp, int hpMax, int mp, int mpMax)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede ser vacío.", nameof(nombre));

            Nombre = nombre;
            Hp = hp;
            HpMax = hpMax;
            Mp = mp;
            MpMax = mpMax;
        }
    }

    public sealed class MenuObjetivo
    {
        public IReadOnlyList<ItemObjetivo> Items { get; }
        public int CancelIndex { get; }

        private MenuObjetivo(List<ItemObjetivo> items)
        {
            var lista = items ?? new List<ItemObjetivo>();
            Items = lista.AsReadOnly();
            CancelIndex = Items.Count;
        }

        public static MenuObjetivo From(List<ItemObjetivo> items) =>
            new MenuObjetivo(items);
    }
}