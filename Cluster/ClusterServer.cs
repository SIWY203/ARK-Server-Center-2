namespace ArkServerCenter.Cluster;


public class ClusterServer
{
    public string Map { get; private set; } = string.Empty;
    public string VisibleMap => Map.EndsWith("_WP") ? Map[..^3] : Map;
    public int Port { get; private set; }
    public string ClusterRootPath { get; private set; } = string.Empty;

    // combining paths
    public string ServerRootPath => Path.Combine(ClusterRootPath, $"{VisibleMap}_{Port}");
    public string SavedPath => Path.Combine(ServerRootPath, "ShooterGame", "Saved");
    public string BackupsPath => Path.Combine(ClusterRootPath, "Backups", "Maps", $"{VisibleMap}_{Port}");



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
        Console.WriteLine($" -> Mapa: {VisibleMap} [Port: {Port}]");
        Console.WriteLine($"    Folder: {ServerRootPath}");
        
    }

}
