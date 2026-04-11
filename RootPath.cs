namespace ArkServerCenter;
using System.Text.Json;
using static MessageManager;

public static class RootPath
{
    private static readonly string _configFile = "rootpath.json";
    private static string _value = string.Empty;

    public static bool IsSet => !string.IsNullOrWhiteSpace(_value);

    public static string Value => _value;

    public static bool TrySetPath(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath)) return false;
        string cleanPath = newPath.Trim().Replace("\"", "");

        if (Directory.Exists(cleanPath))
        {
            _value = cleanPath;
            Save();
            return true;
        }
        return false;
    }


    private static void Save()
    {
        string json = JsonSerializer.Serialize(_value);
        File.WriteAllText(_configFile, json);
    }


    public static void Load()
    {
        if (!File.Exists(_configFile)) return;

        try
        {
            string json = File.ReadAllText(_configFile);
            var data = JsonSerializer.Deserialize<string>(json);
            if (!string.IsNullOrEmpty(data)) _value = data;
        }
        catch { }
    }


    public static void Setup()
    {
        while (!IsSet)
        {
            Console.Clear();
            string? input = FileManager.AskForFolderPath("Aby rozpocząć, trzeba ustawić domyślną główną ścieżkę dla wszystkich klastrów.");

            if (input == null)
            {
                Environment.Exit(0);
            }

            if (!TrySetPath(input))
            {
                Error($"Ścieżka {input} nie istnieje!");
                End();
            }
        }
    }


}

