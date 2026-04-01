namespace Ark_Server_Center;
using static MessageManager;



public class ArkCluster
{
    public string Name { get; private set; }
    public List<ClusterServer> Servers { get; private set; } = new List<ClusterServer>();


    public ArkCluster(string name, List<ClusterServer>? servers = null)
    {
        Name = name;
        Servers = servers ?? new List<ClusterServer>(); // jeśli nie podano serwerów, nowa pusta lista
    }


    public void AddServer(string map, int port)
    {
        Servers.Add(new ClusterServer(map, port));
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


}

