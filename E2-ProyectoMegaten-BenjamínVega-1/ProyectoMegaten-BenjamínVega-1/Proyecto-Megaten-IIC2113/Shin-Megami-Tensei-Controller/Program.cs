using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_View.GUI;
using Shin_Megami_Tensei_Model.Entrada;
using Shin_Megami_Tensei_Model.Repositorios;
using Shin_Megami_Tensei_Model.Validacion;

bool useGui = true; // GUI por defecto para ejecución manual
if (useGui)
{
    RunGui();
    return;
}

/*
 * Este código permite replicar un test case. Primero pregunta por el grupo de test
 * case a replicar. Luego pregunta por el test case específico que se quiere replicar.
 *
 * Por ejemplo, si tu programa está fallando el test case:
 *      "data/E1-BasicCombat-Tests/006.txt"
 * ... puedes ver qué está ocurriendo mediante correr este programa y decir que quieres
 * replicar del grupo "E1-BasicCombat-Tests" el test case 6.
 *
 * Al presionar enter, se ingresa el input del test case en forma automática. Si el
 * color es azúl significa que el output de tu programa es el esperado. Si es rojo
 * significa que el output de tu programa es distinto al esperado (i.e., el test falló).
 *
 * Si, por algún motivo, quieres ejecutar tu programa de modo manual (sin replicar un
 * test case específico), puedes cambiar la línea:
 *      var view = View.BuildManualTestingView(test);
 * por:
 *      var view = View.BuildConsoleView();
 */



string testFolder = SelectTestFolder();
string test = SelectTest(testFolder);
string teamsFolder = testFolder.Replace("-Tests","");
AnnounceTestCase(test);

var view = View.BuildManualTestingView(test);
var game = new Game(view, teamsFolder);
game.Play();

void RunGui()
{
    var window = new SMTGUI();
    var view = new VistaJuegoGUI(window);
    var skillsLookup = new JsonSkills();

    window.Start(() =>
    {
        var team1 = window.GetTeamInfo(1);
        var team2 = window.GetTeamInfo(2);

        if (!ValidateTeam(team1, skillsLookup) || !ValidateTeam(team2, skillsLookup))
        {
            window.ShowEndGameMessage("Equipo inválido");
            return;
        }

        var folder = PrepareTeamsFolder(team1!, team2!);
        var game = new Game(view, folder);
        game.Play();
    });
}

bool ValidateTeam(ITeamInfo? team, ISkillsLookup skills)
{
    if (team is null) return false;

    var intentos = BuildIntentos(team);
    if (intentos.Count == 0) return false;
    if (string.IsNullOrWhiteSpace(intentos[0].Nombre)) return false;

    var resultado = ValidacionEquipos.ValidarCrudos(intentos, skills);
    return resultado.EsValido;
}

List<IntentoUnidad> BuildIntentos(ITeamInfo team)
{
    var unidades = new List<IntentoUnidad>();

    var samuraiName = (team.SamuraiName ?? string.Empty).Trim();
    var skillList = (team.SkillNames ?? Array.Empty<string>())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => s.Trim())
        .ToList();
    unidades.Add(new IntentoUnidad(samuraiName, true, skillList));

    foreach (var demon in team.DemonNames ?? Array.Empty<string>())
    {
        if (string.IsNullOrWhiteSpace(demon)) continue;
        unidades.Add(new IntentoUnidad(demon.Trim(), false, Array.Empty<string>()));
    }

    return unidades;
}

string PrepareTeamsFolder(ITeamInfo team1, ITeamInfo team2)
{
    var root = Path.Combine(Path.GetTempPath(), "ShinMegamiTensei-GUI");
    Directory.CreateDirectory(root);

    var folder = Path.Combine(root, Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(folder);

    var filePath = Path.Combine(folder, "EquiposGUI.txt");
    var contenido = TeamInfoFormatter.FormatTeamInfo(team1, team2);
    File.WriteAllText(filePath, contenido + Environment.NewLine);

    return folder;
}

string SelectTestFolder()
{
    Console.WriteLine("¿Qué grupo de test quieres usar?");
    string[] dirs = GetAvailableTestsInOrder();
    ShowArrayOfOptions(dirs);
    return AskUserToSelectAnOption(dirs);
}

string[] GetAvailableTestsInOrder()
{
    string[] dirs = Directory.GetDirectories("data", "*-Tests", SearchOption.TopDirectoryOnly);
    Array.Sort(dirs);
    return dirs;
}

void ShowArrayOfOptions(string[] options)
{
    for(int i = 0; i < options.Length; i++)
        Console.WriteLine($"{i}- {options[i]}");
}

string AskUserToSelectAnOption(string[] options)
{
    int minValue = 0;
    int maxValue = options.Length - 1;
    int selectedOption = AskUserToSelectNumber(minValue, maxValue);
    return options[selectedOption];
}

int AskUserToSelectNumber(int minValue, int maxValue)
{
    Console.WriteLine($"(Ingresa un número entre {minValue} y {maxValue})");
    int value;
    bool wasParsePossible;
    do
    {
        string? userInput = Console.ReadLine();
        wasParsePossible = int.TryParse(userInput, out value);
    } while (!wasParsePossible || IsValueOutsideTheValidRange(minValue, value, maxValue));

    return value;
}

bool IsValueOutsideTheValidRange(int minValue, int value, int maxValue)
    => value < minValue || value > maxValue;

string SelectTest(string testFolder)
{
    Console.WriteLine("¿Qué test quieres ejecutar?");
    string[] tests = Directory.GetFiles(testFolder, "*.txt" );
    Array.Sort(tests);
    return AskUserToSelectAnOption(tests);
}

void AnnounceTestCase(string test)
{
    Console.WriteLine($"----------------------------------------");
    Console.WriteLine($"Replicando test: {test}");
    Console.WriteLine($"----------------------------------------\n");
}
