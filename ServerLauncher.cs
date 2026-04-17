namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using System.Diagnostics;
using static MessageManager;


public class ServerLauncher
{
    private readonly ClusterServer _server;
    public List<string> Parameters { get; set; } = new();
    public List<string> Flags { get; set; } = new();
    public string AllArgs => string.Join("", Parameters) + " " + string.Join(" ", Flags);
    public string ConfigPath => Path.Combine(_server.ClusterRootPath, $"{_server.VisibleMap}_{_server.Port}_launchArgs.txt");

    public ServerLauncher(ClusterServer server)
    {
        _server = server;
    }

    public static ServerLauncher LoadConfig(ClusterServer server)
    {
        var config = new ServerLauncher(server);
        if (!File.Exists(config.ConfigPath))
        {
            Warn("Brak ustawień rozruchu, tworzenie domyślnych...");
            config.SetDefaultArgs();
            config.SaveConfig();
        }
        else
        {
            var lines = File.ReadAllLines(config.ConfigPath);
            foreach (var line in lines)
            {
                if (line.StartsWith("?")) config.Parameters.Add(line);
                else if (line.StartsWith("-")) config.Flags.Add(line);
            }
        }
        return config;
    }

    public void SaveConfig()
    {
        var allLines = Parameters.Concat(Flags).ToList();
        File.WriteAllLines(ConfigPath, allLines);
    }

    public void SetDefaultArgs()
    {
        Parameters = new List<string>()
        {
            $"?SessionName={ClusterManager.ActiveCluster?.Name}",
            $"?Port={_server.Port}",
            $"?AllowAnyoneBabyImprintCuddle=true",
            $"?ServerAdminPassword=\"123456\"",
        };

        Flags = new List<string>()
        {
            $"-clusterid={ClusterManager.ActiveCluster?.Name}",
            $"-ClusterDirOverride=\"{ClusterManager.ActiveCluster?.ClusterDataPath}\"",
            $"-server",
            $"-log",
            $"-MultiHome=127.0.0.1",
            $"-WinLiveMaxPlayers=10",
            $"-ForceAllowCaveFlyers",
            $"-NoBattlEye",
        };
    }

    //public void AddArgument(string arg)
    //{
    //    arg = arg.Trim();
    //    if (arg.StartsWith("?")) Parameters.Add(arg);
    //    else if (arg.StartsWith("-")) Flags.Add(arg);
    //    else
    //    {
    //        Console.Clear();
    //        Error(
    //            $"{arg} nie jest ani parametrem, ani flagą!\n" +
    //            $"Parametry mają zaczynać się od '?' a flagi od '-'\n" +
    //            $"Anulowano.");
    //        End(); return;
    //    }
    //    Console.Clear();
    //    Success($"Dodano {arg}");
    //    Log($"Obecne argumenty rozruchowe: \n\n{AllArgs}\n");
    //    End();
    //}

    //public void RemoveArgument(string arg = "")
    //{
    //    arg = arg.Trim();
    //    if (string.IsNullOrWhiteSpace(arg))
    //    {
    //        Console.Clear();
    //        Error($"Nic nie podano!");
    //        End(); return;
    //    }
    //    if (AllArgs.Contains(arg))
    //    {
    //        Parameters.Remove($"?{arg}");
    //        Flags.Remove($" {arg}");
    //        Console.Clear();
    //        Success($"Usunięto {arg}");
    //    }
    //    else
    //    {
    //        Console.Clear();
    //        Error($"Nie znaleziono {arg}!");
    //    }
    //    Log($"Obecne argumenty rozruchowe: \n\n{AllArgs}\n");
    //    End();
    //}

    public void CreateConfigBackup()
    {
        string? directory = Path.GetDirectoryName(ConfigPath);
        string? fileName = Path.GetFileName(ConfigPath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            Error("Nieprawidłowa ścieżka pliku!");
            End(); return;
        }

        string backupDir = Path.Combine(directory, "config_backups");
        Directory.CreateDirectory(backupDir);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string backupFile = Path.Combine(backupDir, $"{timestamp}_{fileName}");
        File.Copy(ConfigPath, backupFile, true);

        Console.Clear();
        Success($"Uworzono kopię: backup_{timestamp}_{fileName}");
        End();
    }

    // public void RestoreConfigBackup() { }

    public static void OpenConfigFile(ServerLauncher config)
    {
        FileManager.OpenFile(config.ConfigPath);
    }

    public void ShowConfig()
    {
        Log($"Parametry rozruchowe:\n" +
            $"\n" +
            $"{_server.Map}{AllArgs}");
        End();
    }

    public static void DeleteConfig(ServerLauncher config)
    {
        if (!string.IsNullOrWhiteSpace(config.ConfigPath))
        {
            File.Delete(config.ConfigPath);
            Success("Usunięto plik konfiguracyjny. Nowy zostanie wygenerowany automatycznie.");
            End();
        }
        else
        {
            Error("Usuwanie pliku nie powiodło się!");
            End();
        }
    }

    public static void Launch()
    {
        ClusterServer? server = ClusterManager.ActiveServer;
        Cluster? cluster = ClusterManager.ActiveCluster;

        // null & port handling
        if (server == null || cluster == null) { Error("ActiveServer = null, lub ActiveCluster = null!"); End(); return; }
        if (SafetyChecker.IsServerRunningOnPort(server.Port)) { Error("Serwer już jest włączony!"); End(); return; }

        // process info
        string serverExePath = Path.Combine(server.ServerRootPath, "ShooterGame", "Binaries", "Win64", "ArkAscendedServer.exe");
        if (!File.Exists(serverExePath)) { Error($"Nie znaleziono pliku serwera: {serverExePath}"); End(); return; }

        var config = LoadConfig(server);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = serverExePath,
            Arguments = server.Map + config.AllArgs,
            WorkingDirectory = Path.GetDirectoryName(serverExePath),
            UseShellExecute = true, // serwer w osobnym oknie
            CreateNoWindow = false
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Error($"Krytyczny błąd podczas uruchamiania: {ex.Message}");
            End();
        }

        Success("Uruchamianie serwera...\n");
        config.ShowConfig();
    }


}

