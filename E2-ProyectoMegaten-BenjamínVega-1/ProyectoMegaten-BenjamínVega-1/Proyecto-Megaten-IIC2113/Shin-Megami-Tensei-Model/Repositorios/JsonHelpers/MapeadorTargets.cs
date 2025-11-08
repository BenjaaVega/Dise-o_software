using Shin_Megami_Tensei_Model.ModelosEquipo;

namespace Shin_Megami_Tensei_Model.Repositorios.Ayudantes
{
    internal static class MapeadorTargets
    {
        public static bool IntentarMapear(string? objetivoCrudo, out SkillTarget target)
        {
            target = SkillTarget.Unknown;
            if (string.IsNullOrWhiteSpace(objetivoCrudo)) return false;

            switch (objetivoCrudo.Trim().ToLowerInvariant())
            {
                case "single":
                    target = SkillTarget.Single;
                    return true;
                case "all":
                    target = SkillTarget.AllEnemies;
                    return true;
                case "multi":
                    target = SkillTarget.MultiEnemies;
                    return true;
                case "party":
                    target = SkillTarget.Party;
                    return true;
                case "ally":
                    target = SkillTarget.SingleAlly;
                    return true;
                case "self":
                    target = SkillTarget.Self;
                    return true;
                case "universal":
                    target = SkillTarget.Universal;
                    return true;
                default:
                    target = SkillTarget.Unknown;
                    return false;
            }
        }
    }
}
