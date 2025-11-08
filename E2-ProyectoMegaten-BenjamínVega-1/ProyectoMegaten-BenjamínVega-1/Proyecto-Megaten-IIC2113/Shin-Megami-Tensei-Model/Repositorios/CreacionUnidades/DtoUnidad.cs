using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shin_Megami_Tensei_Model.Repositorios.DTO
{
    internal sealed class DtoUnidad
    {
        [JsonPropertyName("name")]     public string? Name { get; set; }
        [JsonPropertyName("Nombre")]   public string? Nombre { get; set; }
        [JsonPropertyName("stats")]    public BloqueStats? Stats { get; set; }
        [JsonPropertyName("affinity")] public BloqueAfinidades? Affinity { get; set; }
        [JsonPropertyName("skills")]   public List<string>? Skills { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    internal sealed class BloqueStats
    {
        [JsonPropertyName("HP")]  public int HP  { get; set; }
        [JsonPropertyName("MP")]  public int MP  { get; set; }
        [JsonPropertyName("Str")] public int Str { get; set; }
        [JsonPropertyName("Skl")] public int Skl { get; set; }
        [JsonPropertyName("Mag")] public int Mag { get; set; }
        [JsonPropertyName("Spd")] public int Spd { get; set; }
        [JsonPropertyName("Lck")] public int Lck { get; set; }
    }

    internal sealed class BloqueAfinidades
    {
        [JsonPropertyName("Phys")]  public string? Phys  { get; set; }
        [JsonPropertyName("Gun")]   public string? Gun   { get; set; }
        [JsonPropertyName("Fire")]  public string? Fire  { get; set; }
        [JsonPropertyName("Ice")]   public string? Ice   { get; set; }
        [JsonPropertyName("Elec")]  public string? Elec  { get; set; }
        [JsonPropertyName("Force")] public string? Force { get; set; }
        [JsonPropertyName("Light")] public string? Light { get; set; }
        [JsonPropertyName("Dark")]  public string? Dark  { get; set; }
    }
}