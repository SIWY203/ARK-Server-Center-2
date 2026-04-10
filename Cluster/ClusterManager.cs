namespace ArkServerCenter.Cluster;

using System.Security.Cryptography;
using System.Text.Json;
using static MessageManager;


public static class ClusterManager
{
    public static Cluster? ActiveCluster { get; private set; }
    public static ClusterServer? ActiveServer { get; private set; }
    public static bool IsServerSelected => ActiveServer != null;


    public static string configFile = "clusters.json";
    public static List<Cluster> clusters = new();



    public static void LoadClusters()
    {
        if (!File.Exists(configFile))
        {
            clusters = new List<Cluster>();
            SaveClusters(); // tworzenie pliku
        }

        try
        {
            string jsonString = File.ReadAllText(configFile);
            clusters = JsonSerializer.Deserialize<List<Cluster>>(jsonString) ?? new();
        }
        catch (Exception ex)
        {
            Error($"Błąd podczas wczytywania pliku:\n{ex.Message}");
            clusters = new List<Cluster>();
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
        // zmienić na 'string?' a null jako opcja "Anuluj" z dużą pętlą tutaj?
        // czy wewnątrz metod odsłużyć 'anuluj'?
        string clusterName = ClusterName.AskForClusterName();
        string clusterPath = ClusterPath.AskForClusterPath() ?? "";
        string mapName = ClusterServerMap.AskForClusterServerMap();
        int serverPort = ClusterServerPort.AskForClusterServerPort() ?? 0;

        // -----------------------------
        // TO DO
        // - poprawić 'Anuluj' bo aktualnie pomija krok i przeskakuje dalej = źle
        // - nowa metoda UpdateClusterServer(), bez tego nie ma 'Saved' itd
        // - Creator → podział na ClusterCreator() / ServerCreator()
        // - dodać nowe RemoveCluster(), RemoveServer()
        // - dodać AddClusterFromFiles() - dodaje do pliku json z obcego folderu
        // -----------------------------


        Cluster newCluster = new Cluster(clusterName, clusterPath);
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
            Cluster? cluster = ActiveCluster; // zapis stanu, by nie było nulla
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
