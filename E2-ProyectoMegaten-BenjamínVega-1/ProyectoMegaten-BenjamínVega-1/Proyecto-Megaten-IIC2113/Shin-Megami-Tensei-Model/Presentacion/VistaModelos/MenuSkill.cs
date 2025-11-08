namespace Shin_Megami_Tensei_Model.Presentacion.VistaModelos
{
    public sealed class SkillItem
    {
        public string Nombre { get; }
        public int? Costo { get; }

        public SkillItem(string nombre, int? costo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede ser vacío.", nameof(nombre));

            Nombre = nombre;
            Costo = costo;
        }
    }

    public sealed class SkillMenu
    {
        public IReadOnlyList<SkillItem> Items { get; }
        public int CancelIndex { get; }

        private SkillMenu(List<SkillItem> items)
        {
            var lista = items ?? new List<SkillItem>();
            Items = lista.AsReadOnly();
            CancelIndex = Items.Count; 
        }

        public static SkillMenu From(List<SkillItem> items) =>
            new SkillMenu(items);
    }
}