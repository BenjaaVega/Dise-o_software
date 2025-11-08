// Shin_Megami_Tensei_Model/ModelosEquipo/Monstruo.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shin_Megami_Tensei_Model.ModelosEquipo
{
    public sealed class Monstruo : IUnidad
    {
        public string Nombre { get; }
        public bool EsSamurai => false;
        public int HpMax { get; }
        public int MpMax { get; }
        public int Hp { get; private set; }
        public int Mp { get; private set; }
        public Stats Stats { get; }

        public IReadOnlyList<string> Habilidades { get; }

        public SetAfinidad Affinities { get; private set; } =
            new SetAfinidad(
                Afinidad.Neutral, Afinidad.Neutral, Afinidad.Neutral, Afinidad.Neutral,
                Afinidad.Neutral, Afinidad.Neutral, Afinidad.Neutral, Afinidad.Neutral
            );

        public Monstruo(string nombre, Stats stats, IEnumerable<string>? habilidades = null)
        {
            Nombre = nombre;
            Stats  = stats;
            HpMax  = Hp = stats.Hp;
            MpMax  = Mp = stats.Mp;
            Habilidades = (habilidades ?? Array.Empty<string>()).ToList().AsReadOnly();
        }

        public bool EstaViva => Hp > 0;
        public void RecibirDano(int dano) => Hp = Math.Max(0, Hp - Math.Max(0, dano));
        public void GastarMp(int mp) => Mp = Math.Max(0, Mp - mp);
        public void SetAffinities(SetAfinidad set) => Affinities = set;
    }
}