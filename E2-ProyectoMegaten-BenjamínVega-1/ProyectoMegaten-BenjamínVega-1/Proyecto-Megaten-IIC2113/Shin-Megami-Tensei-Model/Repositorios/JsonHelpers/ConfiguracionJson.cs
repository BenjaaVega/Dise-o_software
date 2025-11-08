using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shin_Megami_Tensei_Model.Repositorios.Ayudantes
{
    internal static class ConfiguracionJson
    {
        public static JsonSerializerOptions Opciones => new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
}