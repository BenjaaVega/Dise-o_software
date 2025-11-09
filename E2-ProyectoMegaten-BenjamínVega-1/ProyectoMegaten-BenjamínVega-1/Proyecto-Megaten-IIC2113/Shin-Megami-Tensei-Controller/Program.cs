using Shin_Megami_Tensei;
using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_View.Gui;
using System.Linq;

if (ShouldUseGui())
{
    StartGuiMode();
}
else
{
    RunTestReplicator();
}

bool ShouldUseGui()
{
    Console.WriteLine("¿Quieres iniciar la interfaz gráfica? (s/n)");
    var answer = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(answer))
        return false;

    return answer.Trim().StartsWith("s", StringComparison.OrdinalIgnoreCase);
}

void StartGuiMode()
{
    string teamsFolder = SelectTeamsFolder();
    var view = new ShinMegamiTenseiGuiView();
    view.Start(() =>
    {
        var game = new Game(view, teamsFolder);
        game.Play();
    });
}

string SelectTeamsFolder()
{
    Console.WriteLine("¿Qué carpeta de equipos quieres usar?");
    var dirs = Directory
        .GetDirectories("data", "*", SearchOption.TopDirectoryOnly)
        .Where(d => !d.EndsWith("-Tests", StringComparison.OrdinalIgnoreCase))
        .OrderBy(d => d)
        .ToArray();
    ShowArrayOfOptions(dirs);
    return AskUserToSelectAnOption(dirs);
}

void RunTestReplicator()
{
    string testFolder = SelectTestFolder();
    string test = SelectTest(testFolder);
    string teamsFolder = testFolder.Replace("-Tests", "");
    AnnounceTestCase(test);

    var view = View.BuildManualTestingView(test);
    var game = new Game(view, teamsFolder);
    game.Play();
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
    for (int i = 0; i < options.Length; i++)
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
    string[] tests = Directory.GetFiles(testFolder, "*.txt");
    Array.Sort(tests);
    return AskUserToSelectAnOption(tests);
}

void AnnounceTestCase(string test)
{
    Console.WriteLine($"----------------------------------------");
    Console.WriteLine($"Replicando test: {test}");
    Console.WriteLine($"----------------------------------------\n");
}
