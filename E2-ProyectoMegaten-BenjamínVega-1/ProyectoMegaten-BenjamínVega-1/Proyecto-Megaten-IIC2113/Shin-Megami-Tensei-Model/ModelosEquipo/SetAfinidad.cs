namespace Shin_Megami_Tensei_Model.ModelosEquipo
{

    public sealed class SetAfinidad
    {
        public Afinidad Phys  { get; }
        public Afinidad Gun   { get; }
        public Afinidad Fire  { get; }
        public Afinidad Ice   { get; }
        public Afinidad Elec  { get; }
        public Afinidad Force { get; }
        public Afinidad Light { get; }
        public Afinidad Dark  { get; }

        public SetAfinidad(Afinidad phys, Afinidad gun, Afinidad fire, Afinidad ice,
            Afinidad elec, Afinidad force, Afinidad light, Afinidad dark)
        {
            Phys = phys; Gun = gun; Fire = fire; Ice = ice;
            Elec = elec; Force = force; Light = light; Dark = dark;
        }

        public Afinidad AfinidadDe(Elemento e) => e switch
        {
            Elemento.Phys  => Phys,
            Elemento.Gun   => Gun,
            Elemento.Fire  => Fire,
            Elemento.Ice   => Ice,
            Elemento.Elec  => Elec,
            Elemento.Force => Force,
            Elemento.Light => Light,
            Elemento.Dark  => Dark,
            _ => Afinidad.Neutral
        };
    }
}