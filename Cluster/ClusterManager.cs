namespace ArkServerCenter.Cluster;
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


    public static void ClusterCreator()
    {
        
        string clusterName = string.Empty;
        string clusterPath = RootPath.Value;
        ArkCluster newCluster = new ArkCluster(clusterName, clusterPath);

        string mapName = string.Empty;
        int? serverPort = null;
        ClusterServer newServer = new ClusterServer(mapName, serverPort ?? 0, clusterPath);


        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n======== Kreator Klastrów ========\n");
            Console.Write("Podaj Nazwę klastra: ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            bool hasInvalidChars = input.Any(c => invalidChars.Contains(c));
            if (hasInvalidChars)
            {
                Console.Clear();
                Error(
                    $"Wpisano niedozwolone znaki!\n" +
                    $"Niedozwolone są: {string.Join(" ", invalidChars.Where(c => !char.IsControl(c)).ToArray())}"); // filtr niewidocznych znaków
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


        // dodać enum map i ręczne wpisywanie niestandardowych map
        // dodawanie serwerów do klastra
        // algorytm przypisywania portów i opcja zmiany z info o błędach
        // poprawić cały ClusterCreator() by był user-friendly
        // 
        // w SelectCluster() ewentualnie usunąć End() po ClusterCreator()



        Console.Clear();
        Console.WriteLine($"\n======== Kreator Klastrów ========\n");
        Console.WriteLine();
        Console.WriteLine($"Nazwa klastra: {clusterName}\n");
        Console.WriteLine($"Serwery:\n");
        for (int i = 0; i < newCluster.Servers.Count; i++)
        {
            var server = newCluster.Servers[i];
            string status = SafetyChecker.IsServerRunningOnPort(server.Port) ? "[ONLINE]" : "[OFFLINE]";
            Console.WriteLine($" - Mapa: {server.Map} (Port: {server.Port}) - {status}");
        }



        ActiveCluster = newCluster;
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
