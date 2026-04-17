namespace ArkServerCenter.GlobalSettings;
using ArkServerCenter;
using System.Text.Json;
using static MessageManager;


public static class RootPath
{
    private static readonly string _configFile = "rootpath.json";
    private static string _value = string.Empty;

    public static bool IsSet => !string.IsNullOrWhiteSpace(_value);

    public static string Value
    {
        get => _value;
        private set => _value = value;
    }


    public static bool TrySetPath(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath)) return false;
        string cleanPath = newPath.Trim().Replace("\"", "");

        if (!Directory.Exists(cleanPath)) return false;

        _value = cleanPath;
        Save();
        return true;
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


    public static void ChangePath()
    {
        Console.Clear();

        if (SafetyChecker.IsAnyServerRunning())
        {
            Error($"Jeden z serwerów jest włączony! Anulowano.");
            End(); return;
        }

        string? newPath = FileManager.AskForFolderPath("Ustawianie głównego folderu.");
        if (newPath == null) return; // anuluj
        if (newPath == RootPath.Value)
        {
            Error("Wybrany folder jest już ustawiony jako domyślny! Brak zmian.");
            End(); return;
        }

        bool success = CopyDirectory(RootPath.Value, newPath);
        if (success)
        {
            RootPath.Value = newPath;
            Save();

            Success("Zapisano ścieżkę!");
            while (Console.KeyAvailable) Console.ReadKey(intercept: true); // czyszczenie bufora klawiatury
            End(); return;
        }

        Error("Nie udało się skopiować plików. Anulowano.");
        End(); return;
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



    public static bool CopyDirectory(string sourceDir, string targetDir)
    {
        Console.Clear();

        // sprawdzanie czy folder źródłowy istnieje
        if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
        {
            Warn($"Folder źródłowy nie istnieje!\n{sourceDir}\n");
            Log($"Jeśli pliki zostały przeniesione ręcznie, wszystko jest OK.");
            Log($"Aktualizuję ścieżkę...");
            return true;
        }

        // sprawdzanie czy folder docelowy nie jest pustym stringiem
        if (string.IsNullOrWhiteSpace(targetDir))
        {
            Error("Ścieżka docelowa jest pusta!");
            return false;
        }

        // bezpiecznik (Krytyczny przy rekurencji) zapobiega próbie kopiowania np. X do X\Kopia
        string rel = Path.GetRelativePath(sourceDir, targetDir);
        bool isSubfolder = !rel.StartsWith("..") && rel != ".";
        if (isSubfolder)
        {
            Error("Nie możesz skopiować głównego folderu do jego własnego podfolderu!");
            return false;
        }
        Log("Kopiowanie plików. Może to potrwać kilka minut...");

        string tempDir = $"{targetDir}_temp";
        try
        {
            // 1. czyszczenie temp
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            // 2. kopiowanie do temp
            FileManager.CopyDirectoryCore(sourceDir, tempDir);

            // 3. sprawdzenie czy aby folder nie jest pusty
            if (Directory.GetFileSystemEntries(tempDir).Length == 0)
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Error($"Folder tymczasowy jest pusty: \n{tempDir} \nOperacja przerwana.");
                return false;
            }

            // 4. usuwanie folderu docelowego
            if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);

            // 5. przeniesienie do celu
            Directory.Move(tempDir, targetDir);
        }

        catch (Exception ex)
        {
            Error($"{ex.Message}");
            Console.WriteLine("Cofanie zmian... ");

            // usuwanie uszkodzonych danych
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            return false;
        }
        return true;

    }


}

