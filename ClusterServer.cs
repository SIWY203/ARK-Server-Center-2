namespace Ark_Server_Center;
using static MessageManager;


public class ClusterServer
{
    public string Map { get; private set; }
    public int Port { get; private set; }


    public ClusterServer(string map, int port)
    {
        Map = map;
        Port = port;
    }


}
