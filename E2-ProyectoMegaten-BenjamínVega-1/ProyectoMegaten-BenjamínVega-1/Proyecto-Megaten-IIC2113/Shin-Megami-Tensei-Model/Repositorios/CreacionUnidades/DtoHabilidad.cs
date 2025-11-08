using System.Text.Json.Serialization;

namespace Shin_Megami_Tensei_Model.Repositorios.DTO
{
    internal sealed class DtoHabilidad
    {
        [JsonPropertyName("name")]    public string? Name { get; set; }
        [JsonPropertyName("Nombre")]  public string? Nombre { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        [JsonPropertyName("cost")]    public int? Cost { get; set; }
        [JsonPropertyName("CostoMp")] public int? CostoMp { get; set; }

        [JsonPropertyName("type")]    public string? Type { get; set; }
        [JsonPropertyName("power")]   public int? Power { get; set; }

        [JsonPropertyName("Hits")]    public string? Hits { get; set; }
        [JsonPropertyName("Effect")]  public string? Effect { get; set; }
    }
}