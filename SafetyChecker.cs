namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using System.Net.NetworkInformation;
using static MessageManager;


public static class SafetyChecker
{
    public static bool IsServerRunningOnPort(int port)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var udpListeners = properties.GetActiveUdpListeners();

        foreach (var listener in udpListeners)
        {
            if (listener.Port == port) return true;
        }

        return false;
    }


    public static bool IsAnyServerRunning()
    {
        List<Cluster> clusters = ClusterManager.Clusters;
        for (int i = 0; i < clusters.Count; i++)
        {
            for (int j = 0; j < clusters[i].Servers.Count; j++)
            {
                if (IsServerRunningOnPort(clusters[i].Servers[j].Port)) return false; 
            }
        }
        return true;
    }


    public static (bool IsSetSaved, bool IsSetBackups) ArePathsSet()
    {
        var server = ClusterManager.ActiveServer;
        if (server == null) return (false, false);

        bool isSetSavedPath = !string.IsNullOrWhiteSpace(server.SavedPath);
        bool isSetBackupsPath = !string.IsNullOrWhiteSpace(server.BackupsPath);

        return (isSetSavedPath, isSetBackupsPath);
    }

    public static bool ArePathsSetAndLog()
    {
        var (isSetSavedPath, isSetBackupsPath) = ArePathsSet();
        if (isSetSavedPath && isSetBackupsPath) return true;

        Console.Clear();
        if (!isSetSavedPath)
        {
            Warn("Nie ustalono ścieżki do 'Saved'!");
        }
        if (!isSetBackupsPath)
        {
            Warn("Nie ustalono ścieżki do backupów!");
        }
        return false;
    }


    public static bool IsSafeNow(int port) // check server running and exists Saved and backups folder
    {
        bool isRunning = IsServerRunningOnPort(port);
        if (isRunning) Warn("Serwer jest włączony!");
        else Success("Serwer jest wyłączony.");

        bool arePathsSet = ArePathsSetAndLog();
        return !isRunning && arePathsSet;
    }


    public static (bool HasSaved, bool HasBackups) CheckFoldersExistence()
    {
        var server = ClusterManager.ActiveServer;
        if (server == null) return (false, false);

        bool hasSaved = Directory.Exists(server.SavedPath);
        bool hasBackups = Directory.Exists(server.BackupsPath);

        return (hasSaved, hasBackups);
    }


    public static bool CheckFoldersExistenceAndLog()
    {
        var (hasSaved, hasBackups) = CheckFoldersExistence();
        if (hasSaved && hasBackups) return true;

        Console.Clear();
        if (!hasSaved)
        {
            Warn("Nie wykryto folderu 'Saved'!\n");
        }
        if (!hasBackups)
        {
            Warn("Nie wykryto folderu na backupy!\n");
        }
        return false;
    }


    public static bool HasInvalidChars(string input)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        bool hasInvalidChars = input.Any(c => invalidChars.Contains(c));

        if (hasInvalidChars) return true;
        return false;
    }

}
