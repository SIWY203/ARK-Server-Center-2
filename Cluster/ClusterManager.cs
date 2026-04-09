namespace ArkServerCenter.Cluster;

using System.Security.Cryptography;
using System.Text.Json;
using static MessageManager;


public static class ClusterManager
{
    public static ArkCluster? ActiveCluster { get; private set; }
    public static ClusterServer? ActiveServer { get; private set; }
    public static bool IsServerSelected => ActiveServer != null;


    private static string configFile = "clusters.json";
    private static List<ArkCluster> clusters = new();



    public static void LoadClusters()
    {
        if (!File.Exists(configFile))
        {
            clusters = new List<ArkCluster>();
            SaveClusters(); // tworzenie pliku
        }

        try
        {
            string jsonString = File.ReadAllText(configFile);
            clusters = JsonSerializer.Deserialize<List<ArkCluster>>(jsonString) ?? new();
        }
        catch (Exception ex)
        {
            Error($"Błąd podczas wczytywania pliku:\n{ex.Message}");
            clusters = new List<ArkCluster>();
        }

        if (clusters.Count < 1) ClusterCreator();
    }



    public static void SaveClusters()
    {
        var options = new JsonSerializerOptions { WriteIndented = true }; // ładny format
        string jsonString = JsonSerializer.Serialize(clusters, options);
        File.WriteAllText(configFile, jsonString);
    }



    // ============================
    //  CLUSTER CREATOR
    // ============================
    public static void ClusterCreator()
    {
        string clusterName = string.Empty;
        string clusterPath = RootPath.Value;

        string mapName;
        int serverPort;


        // ============================
        //  CLUSTER NAME
        // ============================
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n======== Kreator Klastrów ========\n");
            Console.Write("Podaj Nazwę klastra: ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (SafetyChecker.HasInvalidChars(input))
            {
                Console.Clear();
                Error("Nazwa klastra zawiera niedozwolone znaki!");
                End(); continue;
            }

            else if (string.IsNullOrWhiteSpace(input))
            {
                Console.Clear();
                Error("Wpis nie może być pusty!");
                End(); continue;
            }
            else
            {
                clusterName = input;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(RootPath.Value))
        {
            while (string.IsNullOrWhiteSpace(RootPath.Value))
            {
                Console.Clear();
                Console.WriteLine($"\n======== Kreator Klastrów ========\n");
                Warn("Jeszcze nie ustawiono domyślnej ścieżki!\n");
                RootPath.Value = FileManager.GetFolderPath() ?? string.Empty;
            }
            Console.Clear();
            Success("Scieżka główna została zapisana!");
            End();
        }


        // ============================
        //  CLUSTER PATH 
        // ============================
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n======== Kreator Klastrów ========\n");
            Console.WriteLine($"Domyślna ścieżka: {RootPath.Value}\n");
            Console.WriteLine(
                "[1] Zastosuj\n" +
                "[2] Ustaw inną\n" +
                "[3] Anuluj\n");

            Console.Write("Wybierz: ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (input == "1")
            {
                clusterPath = RootPath.Value;
                Console.Clear();
                Success("Zapisano ścieżkę!");
                End(); break;
            }

            else if (input == "2")
            {
                string? newPath = FileManager.GetFolderPath();
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    clusterPath = newPath;
                    Console.Clear();
                    Success("Zapisano ścieżkę!");
                    End(); break;
                }
            }

            else if (input == "3")
            {
                Console.Clear();
                Console.WriteLine("Anulowano tworzenie klastra.");
                End(); return;
            }

            else
            {
                Console.Clear();
                Error("Nieprawidłowy wybór, spróbuj ponownie.");
                End();
            }
            
        }


        // ============================
        //  CLUSTERS SERVER MAP 
        // ============================
        while (true)
        {
            ArkMaps.ShowMapMenu();
            int mapCount = ArkMaps.GetMapCount();
            Console.WriteLine($"\n[{mapCount+1}] Inna mapa");

            Console.Write("\nWybierz: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= mapCount)
            {
                mapName = ((ArkMaps.Map)(choice - 1)).ToString();
                Console.Clear();
                Success($"Wybrano mapę {mapName}");
                End(); break;
            }

            else if (input == $"{mapCount + 1}")
            {
                Console.Write("Wpisz nazwę mapy: ");
                mapName = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(mapName))
                {
                    Console.Clear();
                    Error("Nieprawidłowa nazwa mapy!");
                    End();
                }
                if (SafetyChecker.HasInvalidChars(mapName))
                {
                    Console.Clear();
                    Error("Nazwa mapy zawiera niedozwolone znaki!");
                    End();
                }
                else
                {
                    Console.Clear();
                    Success($"Wybrano mapę {mapName}");
                    End(); break;
                }
            }
            else
            {
                Console.Clear();
                Error("Nieprawidłowy wybór mapy!");
                End();
            }
        }


        // ============================
        //  CLUSTERS SERVER PORT
        // ============================
        int startPort = 7777;

        var usedPorts = clusters
            .SelectMany(c => c.Servers)
            .Select(s => s.Port)
            .ToHashSet(); // unikalna kolekcja

        int candidate = startPort;
        while (usedPorts.Contains(candidate))
        {
            candidate += 2;
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n======== Kreator Klastrów ========\n");
            Console.WriteLine($"Pierwszy dostępny port: {candidate}");
            Console.WriteLine(
                $"[1] Potwierdź\n" +
                $"[2] Inny port\n" +
                $"[3] Anuluj"
            );

            Console.Write("\nWybierz: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input == "1")
            {
                Console.Clear();
                Success($"Przypisano port: {candidate}");
                serverPort = candidate;
                End(); break;
            }

            else if (input == "2")
            {
                Console.Clear();
                Console.Write("Zajęte porty: ");
                foreach (int p in usedPorts)
                {
                    Console.Write($"{p} ");
                }
                Console.Write("\nWpisz: ");
                input = Console.ReadLine()?.Trim() ?? "";

                if (int.TryParse(input, out int userPort))
                {
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.Clear();
                        Error("Nie wpisano portu!");
                        End(); continue;
                    }

                    if (userPort % 2 == 0)
                    {
                        Console.Clear();
                        Error("Port musi być nieparzysty!");
                        End(); continue;
                    }

                    if (!usedPorts.Contains(candidate))
                    {
                        Console.Clear();
                        Error($"Port {userPort} jest zajęty!");
                        End(); continue;
                    }

                    serverPort = userPort;
                    Console.Clear();
                    Success($"Przypisano port: {serverPort}");
                    End(); break;
                }
                else                
                {
                    Console.Clear();
                    Error("Nieprawidłowy port!");
                    End(); continue;
                }
            }            

            else if (input == "3")
            {
                Console.Clear();
                Console.WriteLine("Anulowano tworzenie klastra.");
                End(); return;
            }

            else
            {
                Console.Clear();
                Error("Nieprawidłowy wybór, spróbuj ponownie.");
                End();
            }
        }

        // -----------------------------
        // TO DO
        // - Opcja 'Anuluj' dodawania wyłącza program
        // - refactor ClusterCreator()
        // - Updater Server, Map -> UpdateClusterServer(),
        //   bez tego nie ma folderu 'Saved' itd
        //
        // - Creator → podział na ClusterCreator / ServerCreator
        // - RemoveCluster(), RemoveServer()
        // - AddClusterFromFiles() - automat dla istniejących poza jsonem 
        // -----------------------------


        ArkCluster newCluster = new ArkCluster(clusterName, clusterPath);
        ClusterServer newServer = new ClusterServer(mapName, serverPort, clusterPath);
        newCluster.Servers.Add(newServer);

        clusters.Add(newCluster);   // zapis do listy
        ActiveCluster = newCluster; // zapis do sesji
        SaveClusters();             // zapis do json


        Console.Clear();
        Console.WriteLine($"\n======== Kreator Klastrów ========\n");
        Console.WriteLine($"Nazwa klastra: {clusterName}");
        Console.WriteLine($"Lokalizacja: {clusterPath}");
        Console.WriteLine($"Serwery w klastrze ({newCluster.Servers.Count}):");

        if (newCluster.Servers.Count == 0) Console.WriteLine(" ► brak serwerów");

        for (int i = 0; i < newCluster.Servers.Count; i++)
        {
            var server = newCluster.Servers[i];
            string status = SafetyChecker.IsServerRunningOnPort(server.Port) ? "[ONLINE]" : "[OFFLINE]";
            Console.WriteLine($" ► Mapa: {server.Map} (Port: {server.Port}) - {status}");
        }

        End(); return;
    }



    public static void RequireServerSelection()
    {
        if (IsServerSelected)
        {
            ArkCluster? cluster = ActiveCluster; // zapis stanu, by nie było nulla
            SelectCluster();
            if (ActiveCluster != null) SelectClusterServer(ActiveCluster);
            else ActiveCluster = cluster;
        }

        while (!IsServerSelected)
        {
            SelectCluster();
            if (ActiveCluster != null) SelectClusterServer(ActiveCluster);
            else Environment.Exit(0);
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
            for (i = 0; i < clusters.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] Klaster: {clusters[i].Name} ({clusters[i].Servers.Count} serwerów)");
            }
            Console.WriteLine($"[{i + 1}] Nowy klaster");
            Console.WriteLine("[Q] Wyjdź");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "Q")
            {
                ActiveCluster = null;
                break;
            }

            if (input == $"{i + 1}")
            {
                ClusterCreator();
                End();
                break;
            }

            if (int.TryParse(input, out int choice) && choice - 1 >= 0 && choice - 1 < clusters.Count)
            {
                ActiveCluster = clusters[choice - 1];
                break;
            }
        }
    }


    public static void SelectClusterServer(ArkCluster cluster)
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
            Console.WriteLine($"--- KLASTER: {cluster.Name} ---");

            for (int i = 0; i < cluster.Servers.Count; i++)
            {
                var s = cluster.Servers[i];
                string status = SafetyChecker.IsServerRunningOnPort(s.Port) ? "[ONLINE]" : "[OFFLINE]";
                Console.WriteLine($"[{i + 1}] {s.Map} (Port: {s.Port}) - {status}");
            }
            Console.WriteLine("[Q] Wróć");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "Q") break;

            if (int.TryParse(input, out int choice) && choice - 1 >= 0 && choice - 1 < cluster.Servers.Count)
            {
                ClusterServer server = cluster.Servers[choice - 1];

                success = true;
                ActiveServer = server;
            }
        }
    }


}
