namespace Ark_Server_Center;

using System.Xml.Linq;
using static MessageManager;


public class ClusterServer
{
    public string Map { get; private set; }
    public int Port { get; private set; }
    public string ClusterRootPath { get; private set; }

    // combining paths
    public string ServerRootPath => Path.Combine(ClusterRootPath, Map, $"{Map}_{Port}");
    public string SavedPath => Path.Combine(ServerRootPath, "ShooterGame", "Saved");
    public string BackupsPath => Path.Combine(ClusterRootPath, "Backups", "Maps", $"{Map}_{Port}");



    public ClusterServer(string map, int port, string clusterRootPath)
    {
        Map = map;
        Port = port;
        ClusterRootPath = clusterRootPath;
    }


    public void ShowServerInfo()
    {
        Console.WriteLine($"\n=== Server Info ===");
        Console.WriteLine($"\n");
        Console.WriteLine($" -> Mapa: {Map} [Port: {Port}]");
        Console.WriteLine($"    Folder: {ServerRootPath}");
        
    }

}
