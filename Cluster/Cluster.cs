namespace ArkServerCenter.Cluster;
using static MessageManager;


public class Cluster
{
    public string Name { get; private set; }
    public string ClusterRootPath { get; private set; }
    public string ClusterDataPath => Path.Combine(ClusterRootPath, "Cluster Data");
    public string ClusterBackupsPath => Path.Combine(ClusterRootPath, "Backups", "Cluster Data");
    public List<ClusterServer> Servers { get; private set; } = new List<ClusterServer>();
    


    public Cluster(string name, string clusterRootPath, List<ClusterServer>? servers = null)
    {
        Name = name;
        ClusterRootPath = clusterRootPath;
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
            Console.WriteLine($"\n -> Mapa: {server.Map} [Port: {server.Port}]");
            Console.WriteLine($"    Folder: {server.ServerRootPath}");
        }
    }


}

