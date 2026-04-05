namespace Ark_Server_Center;
using static MessageManager;



public class ArkCluster
{
    public string Name { get; private set; }
    public string ClusterDataPath { get; private set; }
    public string ClusterBackupsPath => Path.Combine(ClusterDataPath, "CLUSTER_BACKUP");
    public List<ClusterServer> Servers { get; private set; } = new List<ClusterServer>();
    

    public ArkCluster(string name, string clusterDataPath, List<ClusterServer>? servers = null)
    {
        Name = name;
        ClusterDataPath = clusterDataPath;
        Servers = servers ?? new List<ClusterServer>(); // jeśli nie podano serwerów, nowa pusta lista
    }


    public void AddServer(string map, int port, string serverRootPath)
    {
        Servers.Add(new ClusterServer(map, port, serverRootPath));
        Success($"Dodano serwer do klastra '{Name}': Map = {map}, Port = {port}");
    }


    public void RemoveServer(string map, int port)
    {
        var serverToRemove = Servers.FirstOrDefault(s => s.Map == map && s.Port == port);
        if (serverToRemove != null)
        {
            Servers.Remove(serverToRemove);
            Success($"Usunięto serwer z klastra '{Name}': Map = {map}, Port = {port}");
        }
        else
        {
            Warn($"Nie znaleziono serwera do usunięcia w klastrze '{Name}': Map = {map}, Port = {port}");
        }
    }


    public void ShowClusterInfo()
    {
        Console.WriteLine($"\n=== Klaster: {Name} ===");
        if (Servers.Count == 0) Console.WriteLine(" (brak serwerów)");

        foreach (var server in Servers)
        {
            Console.WriteLine($" -> Mapa: {server.Map} [Port: {server.Port}]");
        }
    }


}

