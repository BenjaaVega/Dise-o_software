namespace Shin_Megami_Tensei_View
{
    internal sealed class VistaArchivoEquipo
    {
        private readonly ConsoleIO _io;

        public VistaArchivoEquipo(ConsoleIO io) => _io = io;

        public void MostrarArchivosEquipo(IReadOnlyList<string> archivos)
        {
            _io.WL("Elige un archivo para cargar los equipos");
            var lista = archivos ?? new List<string>();
            for (int i = 0; i < lista.Count; i++) _io.WL($"{i}: {lista[i]}");
        }

        public int LeerArchivo() => _io.ReadIntOrDefault(0);

        public void MostrarEquipoInvalido() => _io.WL("Archivo de equipos inválido");
    }
}