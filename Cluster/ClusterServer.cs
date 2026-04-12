using System.Text.Json.Serialization;

namespace ArkServerCenter.Cluster;


public class ClusterServer
{
    public string Map { get; private set; }
    public int Port { get; private set; }
    public string ParentClusterName { get; private set; }

    [JsonIgnore] public string VisibleMap => Map.EndsWith("_WP") ? Map[..^3] : Map;
    [JsonIgnore] public string ClusterRootPath => Path.Combine(RootPath.Value, ParentClusterName);
    [JsonIgnore] public string ServerRootPath => Path.Combine(ClusterRootPath, $"{VisibleMap}_{Port}");
    [JsonIgnore] public string SavedPath => Path.Combine(ServerRootPath, "ShooterGame", "Saved");
    [JsonIgnore] public string BackupsPath => Path.Combine(ClusterRootPath, "Backups", "Maps", $"{VisibleMap}_{Port}");



    public ClusterServer(string map, int port, string parentClusterName)
    {
        Map = map;
        Port = port;
        ParentClusterName = parentClusterName;
    }


    public void ShowServerInfo()
    {
        Console.WriteLine($"\n======== Server Info ========\n");
        Console.WriteLine($" Klaster: {ParentClusterName}");
        Console.WriteLine($" Mapa: {VisibleMap} [Port: {Port}]");
    }

}
