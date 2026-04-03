namespace Ark_Server_Center;
using static MessageManager;


public class ClusterServer
{
    public string Map { get; private set; }
    public int Port { get; private set; }

    // składanie ścieżek z głównej ścieżki serwera
    public string ServerRootPath { get; private set; }
    public string SavedPath => Path.Combine(ServerRootPath, "ShooterGame", "Saved");
    public string BackupsPath => Path.Combine(ServerRootPath, "SAVES");

    public ClusterServer(string map, int port, string serverRootPath)
    {
        Map = map;
        Port = port;
        ServerRootPath = serverRootPath;
    }


}
