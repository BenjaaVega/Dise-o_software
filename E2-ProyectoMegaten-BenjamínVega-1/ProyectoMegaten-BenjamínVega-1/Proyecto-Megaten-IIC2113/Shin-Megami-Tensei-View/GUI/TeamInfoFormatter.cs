using System.Linq;
using System.Text;
using Shin_Megami_Tensei_GUI;

namespace Shin_Megami_Tensei_View.GUI;

public static class TeamInfoFormatter
{
    public static string FormatTeamInfo(ITeamInfo team1, ITeamInfo team2)
    {
        var sb = new StringBuilder();
        AppendTeam(sb, "Player 1 Team", team1);
        AppendTeam(sb, "Player 2 Team", team2);
        return sb.ToString().TrimEnd();
    }

    private static void AppendTeam(StringBuilder sb, string header, ITeamInfo team)
    {
        sb.AppendLine(header);

        var samuraiName = team?.SamuraiName?.Trim() ?? string.Empty;
        var skills = team?.SkillNames ?? Array.Empty<string>();
        var filteredSkills = skills.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();

        if (filteredSkills.Length > 0)
        {
            sb.AppendLine($"[Samurai] {samuraiName} ({string.Join(',', filteredSkills)})");
        }
        else
        {
            sb.AppendLine($"[Samurai] {samuraiName}");
        }

        var demons = team?.DemonNames ?? Array.Empty<string>();
        foreach (var demon in demons)
        {
            if (string.IsNullOrWhiteSpace(demon)) continue;
            sb.AppendLine(demon.Trim());
        }
    }
}
