namespace ArkServerCenter.Clusters;

using ArkServerCenter.GlobalSettings;
using System.Text.Json;
using static MessageManager;


public static class ClusterManager
{
    public static Cluster? ActiveCluster { get; set; }
    public static ClusterServer? ActiveServer { get; set; }
    public static bool IsServerSelected => ActiveServer != null;

    private static string _configFile = "clusters.json";

    private static List<Cluster> _clusters = new();
    public static List<Cluster> Clusters
    {
        get => _clusters;
        private set => _clusters = value;
    }


    public static void LoadClusters()
    {
        if (!File.Exists(_configFile))
        {
            _clusters = new List<Cluster>();
            SaveClusters(); // tworzenie pliku
        }

        try
        {
            string jsonString = File.ReadAllText(_configFile);
            _clusters = JsonSerializer.Deserialize<List<Cluster>>(jsonString) ?? new();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Error($"Błąd podczas wczytywania pliku:\n{ex.Message}");
            _clusters = new List<Cluster>();
            End();
        }

        if (_clusters.Count < 1) SelectClusterSource();
    }


    public static void SaveClusters()
    {
        var options = new JsonSerializerOptions { WriteIndented = true }; // ładny format
        string jsonString = JsonSerializer.Serialize(_clusters, options);
        File.WriteAllText(_configFile, jsonString);
    }


    // w tej metodzie zawsze przed return usuń tymczasowego newCluster z listy _clusters
    public static void LoadClusterFromFolder()
    {
        Console.Clear();
        string? path = FileManager.AskForFolderPath("Podaj ścieżkę do klastra");
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

        // 1. cluster name
        string clusterName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
        if (Clusters.Select(c => c.Name).ToList().Contains(clusterName))
        {
            Error("Nazwa klastra jest już zajęta!");
            End(); return;
        }
        string newClusterPath = Path.Combine(RootPath.Value, clusterName);
        Cluster newCluster = new Cluster(clusterName);
        // 2. podfoldery
        string[] subDirectories = Directory.GetDirectories(path);
        List<string> oldNames = new(); // do zmiany nazw nowych folderów (nowe porty)
        foreach (string subDir in subDirectories)
        {
            // 3. nazwy podfolderów
            string folderName = Path.GetFileName(subDir);
            // 4. nazwy VisibleMap i czy to serwer?
            if (!folderName.Contains('_') || !Directory.Exists(Path.Combine(subDir, "ShooterGame"))) continue;
            oldNames.Add(Path.GetFileName(subDir));
            string[] parts = folderName.Split('_');
            string mapName = parts[0];

            // 6. przypisz mapę
            while (true)
            {
                Console.Clear();
                Console.WriteLine(
                    $"Wczytano mapę: {mapName} z nazwy folderu. Nazwa kodowa może być inna!\n" +
                    $"Zazwyczaj różni się tylko dopiskiem \"_WP\", np: TheIsland => TheIsland_WP\n" +
                    $"Czy nazwa kodowa tej mapy to {mapName}_WP?\n" +
                    $"\n" +
                    $"[T] Tak, to jest poprawna nazwa\n" +
                    $"[N] Nie, przejdź do ręcznej zmiany\n" +
                    $"[Q] Anuluj");
                Console.Write("\nWybierz: ");
                string input = Console.ReadLine()?.Trim()?.ToUpper() ?? "";

                if (input == "T")
                {
                    mapName = mapName + "_WP";
                    break;
                }
                if (input == "N")
                {
                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine($"Wpisz \"Q\" aby anulować.\n");
                        Console.Write("Podaj nazwę kodową mapy: ");
                        input = Console.ReadLine()?.Trim()?.ToUpper() ?? "";
                        if (string.IsNullOrWhiteSpace(input))
                        {
                            Console.Clear();
                            Error("Nie podano nazwy mapy!");
                            End(); continue;
                        }
                        if (input == "Q") { if (_clusters.Contains(newCluster)) _clusters.Remove(newCluster); return; }
                        mapName = input;
                        break;
                    }
                    break;
                }
                if (input == "Q") { if (_clusters.Contains(newCluster)) _clusters.Remove(newCluster); return; }
                else
                {
                    Console.Clear();
                    Error("Nieprawidłowy wybór!");
                    End();
                }
            }

            // 7. przypisz port
            int? port = ServerPort.AskForServerPort("======== Konfiguracja portów ========");
            if (port == null) { if (_clusters.Contains(newCluster)) _clusters.Remove(newCluster); return; } // anuluj
 
            // 8. zapisz serwer
            ClusterServer server = new ClusterServer(mapName, port.Value, newClusterPath);
            newCluster.Servers.Add(server);
            if (!_clusters.Contains(newCluster)) _clusters.Add(newCluster); // anti-dupe clusters
            Success($"Wczytano serwer: {mapName} na porcie {port}");  
        }

        // 9. finalizacja
        if (newCluster.Servers.Count > 0)
        {
            SaveClusters();
            Console.Clear();
            Log("Kopiowanie plików klastra...");
            FileManager.CopyDirectoryCore(path, newClusterPath);
            ActiveCluster = newCluster;

            // 10. update nazw folderów na prawidłowe porty
            for (int i = 0; i < newCluster.Servers.Count; i++)
            {
                var server = newCluster.Servers[i];
                string newFolderName = $"{server.VisibleMap}_{server.Port}";
                try
                {
                    Directory.Move(Path.Combine(server.ClusterRootPath, oldNames[i]), server.ServerRootPath);
                    string oldBackupPath = Path.Combine(server.ClusterRootPath, "Backups", oldNames[i]);
                    if (Path.Exists(oldBackupPath)) Directory.Move(Path.Combine(server.ClusterRootPath, "Backups", oldNames[i]), server.BackupsPath);
                    
                    string oldConfigPath = Path.Combine(server.ClusterRootPath, oldNames[i] + "_launchArgs.txt");
                    if (Path.Exists(oldConfigPath))
                    {
                        File.Move(oldConfigPath, Path.Combine(server.ClusterRootPath, $"{server.VisibleMap}_{server.Port}_launchArgs.txt"));
                    }
                }
                catch (Exception ex)
                {
                    Warn($"Nie udało się zmienić nazwy: {ex.Message}");
                }
            }

            Success($"Wczytano klaster '{clusterName}' z liczbą serwerów: {newCluster.Servers.Count}");
        }
        else
        {
            Error("Nie znaleziono poprawnych folderów serwerów w tej lokalizacji.");
        }
        while (Console.KeyAvailable) Console.ReadKey(intercept: true); // czyszczenie bufora
        End();
    }


    public static void SelectClusterSource()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(
                "\n========== Kreator Klastrów ==========\n" +
                "[1] Stwórz nowy klaster\n" +
                "[2] Wczytaj klaster z plików\n" +
                "[Q] Wróć"
            );
            Console.Write("\nWybierz: ");
            string input = Console.ReadLine()?.Trim()?.ToUpper() ?? "";

            switch (input)
            {
                case "1":
                    CreateCluster();
                    return;

                case "2":
                    LoadClusterFromFolder();
                    return;

                case "Q":
                    return;

                default:
                    Console.Clear();
                    Error("Nieprawidłowy wybór!");
                    End(); continue;
            }
        }
        
    }


    public static void CreateCluster()
    {
        string clusterPath = RootPath.Value;

        string? clusterName = ClusterName.AskForClusterName("\n========== Kreator Klastrów ==========\n");
        if (clusterName == null) return; // "Anuluj"

        string? mapName = ServerMap.AskForServerMap("\n========== Kreator Klastrów ==========\n");
        if (mapName == null) return;

        int? serverPort = ServerPort.AskForServerPort("\n========== Kreator Klastrów ==========\n");
        if (serverPort == null) return;


        Cluster newCluster = new Cluster(clusterName);
        ClusterServer newServer = new ClusterServer(mapName, serverPort.Value, clusterName);
        newCluster.Servers.Add(newServer);

        _clusters.Add(newCluster);  // zapis do listy
        ActiveCluster = newCluster; // zapis do sesji
        SaveClusters();             // zapis do json

        Directory.CreateDirectory(newCluster.ClusterRootPath);
        Directory.CreateDirectory(newCluster.ClusterDataPath);
        Directory.CreateDirectory(newServer.ServerRootPath);

        Updater.UpdateServer(newServer);
        CreateServer(); // next servers
    }


    public static void DeleteCluster()
    {
        while (true)
        {
            if (_clusters.Count == 0)
            {
                Console.Clear();
                Error("Obecnie nie ma żadnych klastrów!");
                End(); return;
            }

            Console.Clear();
            Console.WriteLine($"\nKtóry klaster usunąć?\n");

            int i;
            for (i = 0; i < _clusters.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] Klaster: {_clusters[i].Name} ({_clusters[i].Servers.Count} serwerów)");
            }
            Console.WriteLine("[Q] Wyjdź");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "Q")
            {
                ActiveCluster = null;
                return;
            }

            if (int.TryParse(input, out int choice) && choice - 1 >= 0 && choice - 1 < _clusters.Count)
            {
                ActiveCluster = _clusters[choice - 1];
                break;
            }

            if (ActiveCluster == null)
            {
                Console.Clear();
                Error("Nie wybrano klastra!");
                End(); continue;
            }
        }

        while (true)
        {
            Console.Clear();
            Warn(
                $"UWAGA! Usunięcie klastra spowoduje wykasowanie z dysku\n" +
                $"danych klastra {ActiveCluster?.Name} wraz z jego serwerami\n");
            Console.WriteLine(
                "[T] Tak, usuń\n" +
                "[Q] Nie usuwaj!");
            Console.Write("\nWybierz: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input.ToUpper() != "T")
            {
                Console.Clear();
                Console.WriteLine("Anulowano usuwanie klastra.");
                End(); return;
            }

            else
            {
                Console.Clear();
                Warn($"Czy napewno chcesz usunąć klaster {ActiveCluster?.Name}?\n");
                Console.WriteLine(
                    "[T] Tak, usuń\n" +
                    "[Q] Nie usuwaj!");
                Console.Write("\nWybierz: ");
                string confirm = Console.ReadLine()?.Trim() ?? "";

                if (confirm.ToUpper() != "T")
                {
                    Console.Clear();
                    Console.WriteLine("Anulowano usuwanie klastra.");
                    End(); return;
                }

                _clusters.Remove(ActiveCluster!);
                SaveClusters(); // lista i json

                string pathToClean = ActiveCluster?.ClusterRootPath ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(pathToClean) && Directory.Exists(pathToClean))
                {
                    Directory.Delete(pathToClean, true);
                }

                ActiveCluster = null;
                ActiveServer = null;

                Console.Clear();
                Success("Klaster został usunięty.");
                End(); return;
            }
        }
    }


    public static void CreateServer()
    {
        string? map = string.Empty;
        int? port = null;
        while (true)
        {
            if (ActiveCluster == null)
            {
                Error("Nie wybrano klastra!");
                End(); return;
            }

            Console.Clear();
            ActiveCluster.ShowClusterInfo();

            Console.WriteLine(
                    "\n" +
                    "[1] Dodaj serwer\n" +
                    "[Q] Wyjdź"
                );
            Console.Write("\nWybierz: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input.ToUpper() == "Q") return;

            else if (input == "1")
            {
                map = ServerMap.AskForServerMap("\n========== Kreator Serwerów ==========\n");
                if (map == null) continue;

                port = ServerPort.AskForServerPort("\n========== Kreator Serwerów ==========\n");
                if (port == null) continue;

                ClusterServer server = new ClusterServer(map, port.Value, ActiveCluster.Name);
                ActiveCluster.Servers.Add(server);

                Directory.CreateDirectory(server.ServerRootPath);
                SaveClusters();

                Updater.UpdateServer(server);
            }

            else
            {
                Console.Clear();
                Error("Nieprawidłowy wybór!");
                End();
            }
        }
        
    }


    public static void DeleteServer()
    {
        if (ActiveCluster == null)
        {
            Console.Clear();
            Error("Nie wybrano klastra!");
            End(); return;
        }

        string? input;
        while (true)
        {
            if (ActiveCluster?.Servers.Count == 0)
            {
                Console.Clear();
                Error("Tu nie ma żadnych serwerów!");
                End(); return;
            }

            Console.Clear();
            Console.WriteLine($"\nKtóry serwer usunąć?\n");
            int i;
            for (i = 0; i < ActiveCluster?.Servers.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] Serwer: {ActiveCluster.Servers[i].Map} ({ActiveCluster.Servers[i].Port})");
            }
            Console.WriteLine("[Q] Wyjdź");
            Console.Write("\nWybierz: ");

            input = Console.ReadLine()?.ToUpper();
            if (input == "Q")
            {
                ActiveServer = null;
                return;
            }

            if (int.TryParse(input, out int choice) && choice - 1 >= 0 && choice - 1 < ActiveCluster?.Servers.Count)
            {
                ActiveServer = ActiveCluster.Servers[choice - 1];
                break;
            }

            else 
            {
                Console.Clear();
                Error("Nieprawidłowy wybór!");
                End();
            }
        }

        if (ActiveServer == null)
        {
            Console.Clear();
            Error("Nie wybrano serwera!");
            End(); return;
        }

        Console.Clear();
        Warn(
            $"UWAGA! Usunięcie serwera spowoduje wykasowanie z dysku\n" +
            $"danych serwera {ActiveServer?.Map} ({ActiveServer?.Port}) wraz z jego plikami\n");
        Console.WriteLine(
            "[T] Tak, usuń\n" +
            "[N] Nie usuwaj!");
        Console.Write("\nWybierz: ");
        input = Console.ReadLine()?.Trim() ?? "";

        if (input.ToUpper() != "T")
        {
            Console.Clear();
            Console.WriteLine("Anulowano usuwanie serwera.");
            End(); return;
        }

        else
        {
            Console.Clear();
            Warn($"Czy napewno chcesz usunąć serwer {ActiveServer?.Map} ({ActiveServer?.Port})?\n");
            Console.WriteLine(
                "[T] Tak, usuń\n" +
                "[N] Nie usuwaj!"); 
            Console.Write("\nWybierz: ");
            string confirm = Console.ReadLine()?.Trim() ?? "";

            if (confirm != "T")
            {
                Console.Clear();
                Console.WriteLine("Anulowano usuwanie serwera.");
                End(); return;
            }

            ActiveCluster.Servers.Remove(ActiveServer!);
            SaveClusters(); // lista i json

            string pathToClean = ActiveServer?.ServerRootPath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(pathToClean) && Directory.Exists(pathToClean))
            {
                Directory.Delete(pathToClean, true);
            }

            ActiveServer = null;

            Console.Clear();
            Success("Serwer został usunięty.");
            End(); return;
        }
    }


    public static void SelectClusterAndServer()
    {
        ActiveServer = null;
        while (ActiveServer == null)
        {
            SelectCluster();

            if (ActiveCluster == null) Environment.Exit(0);
            else SelectClusterServer(ActiveCluster);
        }
    }


    public static void SelectCluster()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========= ARK SERVER CENTER =========");
            Console.WriteLine("Wybierz klaster do zarządzania:\n");
            int i;
            for (i = 0; i < _clusters.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] Klaster: {_clusters[i].Name} ({_clusters[i].Servers.Count} serwerów)");
            }
            Console.WriteLine($"[{i + 1}] Nowy klaster");
            Console.WriteLine($"[{i + 2}] Usuń klaster");
            Console.WriteLine($"[{i + 3}] Ustawienia");
            Console.WriteLine("[Q] Wyjdź");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "Q")
            {
                ActiveCluster = null;
                break; // exit program
            }

            if (input == $"{i + 1}")
            {
                SelectClusterSource();
                continue;
            }

            if (input == $"{i + 2}")
            {
                DeleteCluster();
                continue;
            }

            if (input == $"{i + 3}")
            {
                GlobalSettingsMenu.ShowMenu();
                continue;
            }

            if (int.TryParse(input, out int choice) && choice - 1 >= 0 && choice - 1 < _clusters.Count)
            {
                ActiveCluster = _clusters[choice - 1];
                break;
            }
        }
    }


    public static void SelectClusterServer(Cluster cluster)
    {
        if (cluster == null)
        {
            Console.Clear();
            Console.WriteLine("Nie wybrano klastra!");
            End();
            return;
        }

        bool success = false;
        while (!success)
        {
            Console.Clear();
            Console.WriteLine($"------ KLASTER: {cluster.Name} ------");

            int i;
            for (i = 0; i < cluster.Servers.Count; i++)
            {
                var s = cluster.Servers[i];
                string status = SafetyChecker.IsServerRunningOnPort(s.Port) ? "[ONLINE]" : "[OFFLINE]";
                Console.WriteLine($"[{i + 1}] {s.Map} (Port: {s.Port}) - {status}");
            }
            Console.WriteLine($"[{i + 1}] Nowy serwer");
            Console.WriteLine($"[{i + 2}] Usuń serwer");
            Console.WriteLine("[Q] Wróć");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "Q") return;

            if (input == $"{i + 1}")
            {
                CreateServer();
                continue;
            }

            if (input == $"{i + 2}")
            {
                DeleteServer();
                continue;
            }

            if (int.TryParse(input, out int choice) && choice - 1 >= 0 && choice - 1 < cluster.Servers.Count)
            {
                ClusterServer server = cluster.Servers[choice - 1];
                ActiveServer = server;
                success = true;
            }
        }
    }


}
