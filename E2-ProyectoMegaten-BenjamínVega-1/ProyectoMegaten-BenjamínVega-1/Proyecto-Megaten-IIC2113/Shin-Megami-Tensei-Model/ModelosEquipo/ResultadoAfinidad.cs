namespace Shin_Megami_Tensei_Model.Combate;
public enum HitOutcome { Damage, Weak, Resist, Nullified, Reflected, Drained }

public sealed record ResultadoAfinidad(
    HitOutcome Outcome,
    double DamageMultiplier,
    EfectoTruno Effect
);