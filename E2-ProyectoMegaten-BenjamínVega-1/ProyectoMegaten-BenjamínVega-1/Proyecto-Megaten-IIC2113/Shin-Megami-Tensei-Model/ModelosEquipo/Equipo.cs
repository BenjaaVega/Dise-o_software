namespace Shin_Megami_Tensei_Model.ModelosEquipo
{
    public sealed class Equipo
    {
        public const int ActiveSlots = 4;

        private readonly List<IUnidad> _unidades;
        public IReadOnlyList<IUnidad> Unidades => _unidades;
        
        private readonly Dictionary<IUnidad, int> _ordenArchivo;
        private int _escudosTetraja;
        
        public Samurai Samurai => (Samurai)_unidades.Single(u => u != null && u.EsSamurai);

        public Equipo(IEnumerable<IUnidad> unidades)
        {
            _unidades = (unidades ?? Enumerable.Empty<IUnidad>()).ToList();

           
            AsegurarCuatroActivosFijos();
            
            _ordenArchivo = new Dictionary<IUnidad, int>(ReferenceEqualityComparer<IUnidad>.Instance);
            int idx = 0;
            foreach (var u in _unidades)
            {
                if (u != null) _ordenArchivo[u] = idx;
                idx++;
            }
        }

        public bool TieneEscudoTetraja => _escudosTetraja > 0;

        public void AgregarEscudoTetraja(int cantidad = 1)
        {
            if (cantidad <= 0) return;
            _escudosTetraja += cantidad;
        }

        public bool ConsumirEscudoTetraja()
        {
            if (_escudosTetraja <= 0) return false;
            _escudosTetraja--;
            return true;
        }

        private void AsegurarCuatroActivosFijos()
        {
            while (_unidades.Count < ActiveSlots)
                _unidades.Add(null);
        }
        
        public int OrdenEnArchivoDe(IUnidad u)
        {
            if (u is null) return int.MaxValue;
            return _ordenArchivo.TryGetValue(u, out var i) ? i : int.MaxValue;
        }

        public IEnumerable<IUnidad> Vivas() => _unidades.Where(u => u?.EstaViva == true);

        public bool TieneDuplicadosPorNombre() =>
            _unidades.Where(u => u != null)
                     .GroupBy(u => u.Nombre, StringComparer.OrdinalIgnoreCase)
                     .Any(g => g.Count() > 1);
        

        public void MoverDesdeReserva(IUnidad quien, int slotDestino)
        {
            if (quien is null) throw new ArgumentNullException(nameof(quien));
            ValidarSlotActivo(slotDestino);

            int idxRes = IndexOfUnidad(quien);
            if (idxRes < 0) throw new InvalidOperationException("La unidad a mover no pertenece a este equipo.");
            if (idxRes < ActiveSlots) throw new InvalidOperationException("La unidad indicada no está en reserva.");

            Swap(idxRes, slotDestino);
        }

        public void IntercambiarConReserva(IUnidad reserva, int slotDestino)
        {
            if (reserva is null) throw new ArgumentNullException(nameof(reserva));
            ValidarSlotActivo(slotDestino);

            int idxRes = IndexOfUnidad(reserva);
            if (idxRes < 0) throw new InvalidOperationException("La unidad de reserva no pertenece a este equipo.");
            if (idxRes < ActiveSlots) throw new InvalidOperationException("La unidad indicada no está en reserva.");

            Swap(idxRes, slotDestino);
        }

        public void Intercambiar(IUnidad a, IUnidad b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (ReferenceEquals(a, b)) return;

            int ia = IndexOfUnidad(a);
            int ib = IndexOfUnidad(b);
            if (ia < 0 || ib < 0)
                throw new InvalidOperationException("Alguna de las unidades no pertenece a este equipo.");

            Swap(ia, ib);
        }
        
        public bool BajarMuertoDeActivosAReserva(IUnidad u)
        {
            if (u == null) return false;

            int idx = IndexOfUnidad(u);
            if (idx < 0) return false;                 
            if (idx >= ActiveSlots) return false;          
            if (u.EsSamurai) return false;                 
            if (u.EstaViva) return false;                 

            AsegurarCuatroActivosFijos();

            _unidades[idx] = null; 
            _unidades.Add(u);     
            return true;
        }
        

        private void ValidarSlotActivo(int slot)
        {
            if (slot < 0 || slot >= ActiveSlots)
                throw new ArgumentOutOfRangeException(nameof(slot), $"El slot activo debe estar en [0..{ActiveSlots - 1}].");
        }

        private int IndexOfUnidad(IUnidad u)
        {
            int idx = _unidades.FindIndex(x => ReferenceEquals(x, u));
            if (idx >= 0) return idx;
            
            return _unidades.FindIndex(x =>
                x != null && string.Equals(x.Nombre, u.Nombre, StringComparison.OrdinalIgnoreCase));
        }

        private void Swap(int i, int j)
        {
            if (i == j) return;
            (_unidades[i], _unidades[j]) = (_unidades[j], _unidades[i]);
        }
        
        private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
        {
            public static readonly ReferenceEqualityComparer<T> Instance = new();
            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
