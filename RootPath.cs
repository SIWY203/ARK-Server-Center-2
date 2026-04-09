namespace ArkServerCenter;
using System.Text.Json;
using static MessageManager;

public static class RootPath
{
    private static readonly string _configFile = "rootpath.json";
    private static string _value = string.Empty;
    public static string Value
    {
        get => _value;

        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            string cleanPath = value.Trim().Replace("\"", "");

            if (Directory.Exists(cleanPath))
            {
                _value = cleanPath;
                Save();
            }
            else
            {
                Error($"Ścieżka {cleanPath} nie istnieje!");
            }
        }
    }


    public static void Load()
    {
        if (!File.Exists(_configFile)) { return; }

        try
        {
            string json = File.ReadAllText(_configFile);
            var data = JsonSerializer.Deserialize<string>(json);
            if (!string.IsNullOrEmpty(data)) _value = data;
            else { Error("Plik rootpath.json jest pusty lub niepoprawny."); End(); }
        }
        catch (Exception ex) 
        { 
            Error($"Błąd podczas wczytywania pliku rootpath.json:\n {ex.Message}"); End();
        }
    }

    private static void Save()
    {
        var data = _value;
        string json = JsonSerializer.Serialize(data);
        File.WriteAllText(_configFile, json);
    }


}

