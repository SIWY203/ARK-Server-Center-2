namespace ArkServerCenter.Cluster;

using System.Text.Json.Serialization;
using static MessageManager;


public class Cluster
{
    public string Name { get; private set; }
    public List<ClusterServer> Servers { get; private set; } = new List<ClusterServer>();

    [JsonIgnore] public string ClusterRootPath => Path.Combine(RootPath.Value, Name);
    [JsonIgnore] public string ClusterDataPath => Path.Combine(ClusterRootPath, "Cluster Data");
    [JsonIgnore] public string ClusterBackupsPath => Path.Combine(ClusterRootPath, "Backups", "Cluster Data");
    

    public Cluster(string name, List<ClusterServer>? servers = null)
    {
        Name = name;
        Servers = servers ?? new List<ClusterServer>();
    }


    public void AddServer(string map, int port)
    {
        Servers.Add(new ClusterServer(map, port, Name));
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
        Console.WriteLine($"\n======== Cluster Info ========\n");
        Console.WriteLine($"Nazwa klastra: {Name}");
        Console.WriteLine($"Lokalizacja: {ClusterRootPath}\n");
        Console.WriteLine($"Serwery w klastrze ({Servers.Count}):\n");

        if (Servers.Count == 0) Console.WriteLine(" - brak serwerów");

        for (int i = 0; i < Servers.Count; i++)
        {
            var server = Servers[i];
            string status = SafetyChecker.IsServerRunningOnPort(server.Port) ? "[ONLINE]" : "[OFFLINE]";
            Console.WriteLine($" - Mapa: {server.Map} (Port: {server.Port}) | {status}");
        }
    }


}

