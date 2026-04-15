namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using System.Diagnostics;
using static MessageManager;

public class ServerConfig
{
    private readonly ClusterServer _server;
    public List<string> Parameters { get; set; } = new();
    public List<string> Flags { get; set; } = new();
    public string AllArgs => string.Join("", Parameters) + " " + string.Join(" ", Flags);
    public string ConfigPath => Path.Combine(_server.ClusterRootPath, $"{_server.VisibleMap}_{_server.Port}_launchArgs.txt");

    public ServerConfig(ClusterServer server)
    {
        _server = server;
    }

    public static ServerConfig Load(ClusterServer server)
    {
        var config = new ServerConfig(server);
        if (!File.Exists(config.ConfigPath))
        {
            Warn("Brak ustawień rozruchu, tworzenie domyślnych...");
            config.SetDefaultArgs();
            config.Save();
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

    public void Save()
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

    public void AddArgument(string arg)
    {
        arg = arg.Trim();
        if (arg.StartsWith("?")) Parameters.Add(arg);
        else if (arg.StartsWith("-")) Flags.Add(" " + arg);
        else
        {
            Console.Clear();
            Error(
                $"{arg} nie jest ani parametrem, ani flagą!\n" +
                $"Parametry mają zaczynać się od '?' a flagi od '-'\n" +
                $"Anulowano.");
            End(); return;
        }
        Console.Clear();
        Success($"Dodano {arg}");
        Log($"Obecne argumenty rozruchowe: \n\n{AllArgs}\n");
        End();
    }

    public void RemoveArgument(string arg = "")
    {
        arg = arg.Trim();
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.Clear();
            Error($"Nic nie podano!");
            End(); return;
        }
        if (AllArgs.Contains(arg))
        {
            Parameters.Remove($"?{arg}");
            Flags.Remove($" {arg}");
            Console.Clear();
            Success($"Usunięto {arg}");
            Log($"Obecne argumenty rozruchowe: \n\n{AllArgs}\n");
            End();
        }
        else
        {
            Console.Clear();
            Error($"Nie znaleziono {arg}!");
            Log($"Obecne argumenty rozruchowe: \n\n{AllArgs}\n");
            End();
        }
    }

    public void CreateBackup(string file)
    {
        string? directory = Path.GetDirectoryName(file);
        string? fileName = Path.GetFileName(file);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            Error("Nieprawidłowa ścieżka pliku!");
            End(); return;
        }

        string backupDir = Path.Combine(directory, "config_backups");
        Directory.CreateDirectory(backupDir);

        string backupPath = Path.Combine(backupDir, "backup-" + fileName);
        File.Copy(file, backupPath, true);

        Console.Clear();
        Success("Uworzono kopię pliku.");
        End();
    }

    public static void OpenFile(string file)
    {
        FileManager.OpenFile(file);
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

        var config = Load(server);

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
        Log($"Parametry rozruchowe\n" +
            $"\n" +
            $"{server.Map}{config.AllArgs}");
        End();
    }


}

