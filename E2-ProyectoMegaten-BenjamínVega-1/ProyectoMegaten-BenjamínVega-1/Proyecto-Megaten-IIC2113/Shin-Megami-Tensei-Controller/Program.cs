using System;
using System.IO;
using System.Linq;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_View.VistaGUI;
using Shin_Megami_Tensei;

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

var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
if (ShouldUseGui(args))
{
    RunGui();
    return;
}

RunConsole();

void RunGui()
{
    string teamsFolder = ResolveDefaultTeamsFolder();
    var consoleView = View.BuildConsoleView();
    var guiView = new VistaGUI.VistaJuegoGui(consoleView);
    var game = new Game(guiView, teamsFolder);
    guiView.Run(() => game.Play());
}

void RunConsole()
{
    string testFolder = SelectTestFolder();
    string test = SelectTest(testFolder);
    string teamsFolder = testFolder.Replace("-Tests","");
    AnnounceTestCase(test);

    var view = View.BuildManualTestingView(test);
    var game = new Game(view, teamsFolder);
    game.Play();
}

bool ShouldUseGui(string[] cliArgs)
{
    var env = Environment.GetEnvironmentVariable("SMT_USE_GUI");
    if (!string.IsNullOrWhiteSpace(env))
    {
        var trimmed = env.Trim();
        if (string.Equals(trimmed, "0", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    if (cliArgs.Any(arg => string.Equals(arg, "--console", StringComparison.OrdinalIgnoreCase)))
        return false;

    if (cliArgs.Any(arg => string.Equals(arg, "--gui", StringComparison.OrdinalIgnoreCase)))
        return true;

    return true;
}

string ResolveDefaultTeamsFolder()
{
    var env = Environment.GetEnvironmentVariable("SMT_TEAMS_FOLDER");
    if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
    {
        return Path.GetFullPath(env);
    }

    string? dataDir = FindDataDirectory(Directory.GetCurrentDirectory())
                      ?? FindDataDirectory(AppContext.BaseDirectory);

    if (dataDir is null)
    {
        return Directory.GetCurrentDirectory();
    }

    foreach (var dir in Directory.EnumerateDirectories(dataDir).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
    {
        if (dir.EndsWith("-Tests", StringComparison.OrdinalIgnoreCase)) continue;
        if (Directory.EnumerateFiles(dir, "*.txt").Any()) return dir;
    }

    return dataDir;
}

string? FindDataDirectory(string? start)
{
    if (string.IsNullOrWhiteSpace(start)) return null;

    var dir = new DirectoryInfo(start);
    while (dir is not null)
    {
        var candidate = Path.Combine(dir.FullName, "data");
        if (Directory.Exists(candidate))
        {
            return candidate;
        }

        dir = dir.Parent;
    }

    return null;
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