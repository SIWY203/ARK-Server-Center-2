namespace Ark_Server_Center;

using System.ComponentModel.Design;
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
        Success("Kreator Klastrów");
        End(); return;
    }


    public static void RequireServerSelection()
    {
        if (IsServerSelected)
        {
            // Zapamiętanie stanu, aby nie zostawić nulla
            ArkCluster? cluster = ActiveCluster;
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

            for (int i = 0; i < clusters.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] Klaster: {clusters[i].Name} ({clusters[i].Servers.Count} serwerów)");
            }
            Console.WriteLine("[Q] Wyjdź");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "Q")
            {
                ActiveCluster = null;
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
