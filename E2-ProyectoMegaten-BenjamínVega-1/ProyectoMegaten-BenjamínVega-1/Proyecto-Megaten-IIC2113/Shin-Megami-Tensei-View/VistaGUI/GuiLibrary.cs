using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Shin_Megami_Tensei_View.VistaGUI;

internal sealed class GuiLibrary
{
    private readonly MethodInfo _start;
    private readonly MethodInfo _update;
    private readonly MethodInfo _showMessage;
    private readonly MethodInfo _getClicked;
    private readonly PropertyInfo _clickedText;
    private readonly PropertyInfo _clickedPlayer;
    private readonly PropertyInfo _clickedType;
    private readonly object _buttonValue;
    private readonly object _unitBoardValue;
    private readonly object _unitReserveValue;

    private GuiLibrary(Assembly assembly)
    {
        Assembly = assembly;
        SmtGuiType = assembly.GetType("Shin_Megami_Tensei_GUI.SMTGUI")
                     ?? throw new InvalidOperationException("No se encontró el tipo SMTGUI en la librería GUI.");
        StateType = assembly.GetType("Shin_Megami_Tensei_GUI.IState")
                    ?? throw new InvalidOperationException("No se encontró la interfaz IState en la librería GUI.");
        PlayerType = assembly.GetType("Shin_Megami_Tensei_GUI.IPlayer")
                     ?? throw new InvalidOperationException("No se encontró la interfaz IPlayer en la librería GUI.");
        UnitType = assembly.GetType("Shin_Megami_Tensei_GUI.IUnit")
                   ?? throw new InvalidOperationException("No se encontró la interfaz IUnit en la librería GUI.");
        TeamInfoType = assembly.GetType("Shin_Megami_Tensei_GUI.ITeamInfo")
                       ?? throw new InvalidOperationException("No se encontró la interfaz ITeamInfo en la librería GUI.");
        ClickedElementType = assembly.GetType("Shin_Megami_Tensei_GUI.IClickedElement")
                             ?? throw new InvalidOperationException("No se encontró la interfaz IClickedElement en la librería GUI.");
        ClickedElementEnum = assembly.GetType("Shin_Megami_Tensei_GUI.ClickedElementType")
                              ?? throw new InvalidOperationException("No se encontró la enumeración ClickedElementType en la librería GUI.");

        _start = SmtGuiType.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public)
                  ?? throw new InvalidOperationException("La clase SMTGUI no expone el método Start.");
        _update = SmtGuiType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public)
                   ?? throw new InvalidOperationException("La clase SMTGUI no expone el método Update.");
        _showMessage = SmtGuiType.GetMethod("ShowEndGameMessage", BindingFlags.Instance | BindingFlags.Public)
                       ?? SmtGuiType.GetMethod("ShowMessage", BindingFlags.Instance | BindingFlags.Public)
                       ?? throw new InvalidOperationException("La clase SMTGUI no expone los métodos de mensajes esperados.");
        _getClicked = SmtGuiType.GetMethod("GetClickedElement", BindingFlags.Instance | BindingFlags.Public)
                      ?? throw new InvalidOperationException("La clase SMTGUI no expone el método GetClickedElement.");

        _clickedText = ClickedElementType.GetProperty("Text", BindingFlags.Instance | BindingFlags.Public)
                        ?? throw new InvalidOperationException("IClickedElement no expone la propiedad Text.");
        _clickedPlayer = ClickedElementType.GetProperty("PlayerId", BindingFlags.Instance | BindingFlags.Public)
                          ?? throw new InvalidOperationException("IClickedElement no expone la propiedad PlayerId.");
        _clickedType = ClickedElementType.GetProperty("Type", BindingFlags.Instance | BindingFlags.Public)
                        ?? throw new InvalidOperationException("IClickedElement no expone la propiedad Type.");

        _buttonValue = Enum.Parse(ClickedElementEnum, "Button", true);
        _unitBoardValue = Enum.Parse(ClickedElementEnum, "UnitInBoard", true);
        _unitReserveValue = Enum.Parse(ClickedElementEnum, "UnitInReserve", true);
    }

    public Assembly Assembly { get; }
    public Type SmtGuiType { get; }
    public Type StateType { get; }
    public Type PlayerType { get; }
    public Type UnitType { get; }
    public Type TeamInfoType { get; }
    public Type ClickedElementType { get; }
    public Type ClickedElementEnum { get; }

    public static GuiLibrary Load()
    {
        var candidates = EnumerateCandidateAssemblies();
        foreach (var path in candidates)
        {
            try
            {
                var assembly = Assembly.LoadFrom(path);
                if (assembly.GetType("Shin_Megami_Tensei_GUI.SMTGUI") is not null)
                {
                    return new GuiLibrary(assembly);
                }
            }
            catch
            {
                // Ignoramos intentos fallidos.
            }
        }

        throw new InvalidOperationException(
            "No se pudo cargar la librería gráfica SMTGUI. Copia la carpeta GuiLIB con SMTGUI.dll junto al archivo .sln o " +
            "configura las variables de entorno SMTGUI_LIB/SMTGUI_DIR para apuntar a la librería.");
    }

    public object CreateGui() => Activator.CreateInstance(SmtGuiType)
                                ?? throw new InvalidOperationException("No se pudo instanciar SMTGUI.");

    public void Start(object gui, Action callback)
    {
        _start.Invoke(gui, new object[] { callback });
    }

    public void Update(object gui, object state)
    {
        _update.Invoke(gui, new[] { state });
    }

    public void ShowMessage(object gui, string message)
    {
        _showMessage.Invoke(gui, new object[] { message });
    }

    public object GetClickedElement(object gui)
    {
        return _getClicked.Invoke(gui, Array.Empty<object>())
               ?? throw new InvalidOperationException("GetClickedElement retornó null.");
    }

    public bool TryGetButtonText(object clicked, out string? text)
    {
        if (!IsType(clicked, _buttonValue))
        {
            text = null;
            return false;
        }

        text = _clickedText.GetValue(clicked) as string;
        return true;
    }

    public bool TryGetUnitInfo(object clicked, out string? name, out int? playerId)
    {
        if (!IsType(clicked, _unitBoardValue) && !IsType(clicked, _unitReserveValue))
        {
            name = null;
            playerId = null;
            return false;
        }

        name = _clickedText.GetValue(clicked) as string;
        playerId = _clickedPlayer.GetValue(clicked) as int?;
        return true;
    }

    private bool IsType(object clicked, object expected)
    {
        var value = _clickedType.GetValue(clicked);
        return value is not null && value.Equals(expected);
    }

    private static IEnumerable<string> EnumerateCandidateAssemblies()
    {
        var env = Environment.GetEnvironmentVariable("SMTGUI_LIB");
        if (!string.IsNullOrWhiteSpace(env) && File.Exists(env))
            yield return Path.GetFullPath(env);

        var envDir = Environment.GetEnvironmentVariable("SMTGUI_DIR");
        if (!string.IsNullOrWhiteSpace(envDir) && Directory.Exists(envDir))
        {
            foreach (var path in EnumerateDlls(envDir))
                yield return path;
        }

        string baseDir = AppContext.BaseDirectory;
        foreach (var path in EnumerateFromBase(baseDir))
            yield return path;

        string cwd = Directory.GetCurrentDirectory();
        if (!string.Equals(cwd, baseDir, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var path in EnumerateFromBase(cwd))
                yield return path;
        }
    }

    private static IEnumerable<string> EnumerateFromBase(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            var guiDir = Path.Combine(dir.FullName, "GuiLIB");
            if (Directory.Exists(guiDir))
            {
                foreach (var path in EnumerateDlls(guiDir))
                    yield return path;
            }

            dir = dir.Parent;
        }
    }

    private static IEnumerable<string> EnumerateDlls(string directory)
    {
        return Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);
    }
}
