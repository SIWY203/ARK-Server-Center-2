namespace Ark_Server_Center;
using System.Net.NetworkInformation;
using static MessageManager;
using static PathManager;


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

    public static (bool IsSetSaved, bool IsSetSaves) ArePathsSet()
    {
        bool isSetSavedPath = !string.IsNullOrWhiteSpace(PathTo_Saved);
        bool isSetSavesPath = !string.IsNullOrWhiteSpace(PathTo_SAVES);

        return (isSetSavedPath, isSetSavesPath);
    }

    public static bool ArePathsSetAndLog()
    {
        var (isSetSavedPath, isSetSavesPath) = ArePathsSet();
        if (isSetSavedPath && isSetSavesPath) return true;

        Console.Clear();
        if (!isSetSavedPath)
        {
            Warn("Nie ustalono ścieżki do 'Saved'!");
        }
        if (!isSetSavesPath)
        {
            Warn("Nie ustalono ścieżki do backupów!");
        }
        return false;
    }

    public static bool CheckPathToSaved()
    {
        return true;
    }

    public static bool IsSafeNow(int port)
    {
        bool isRunning = IsServerRunningOnPort(port);
        if (isRunning) Warn("Serwer jest włączony!");
        else Success("Serwer jest wyłączony.");

        bool arePathsSet = ArePathsSetAndLog();
        return !isRunning && arePathsSet;
    }

    public static (bool HasSaved, bool HasSaves) CheckFoldersExistence()
    {
        bool hasSaved = Directory.Exists(Path.Combine(PathTo_Saved, "Saved"));
        bool hasSaves = Directory.Exists(Path.Combine(PathTo_SAVES, "SAVES"));

        return (hasSaved, hasSaves);
    }

    public static bool CheckFoldersExistenceAndLog()
    {
        var (hasSaved, hasSaves) = CheckFoldersExistence();
        if (hasSaved && hasSaves) return true;

        Console.Clear();
        if (!hasSaved)
        {
            Warn("Nie wykryto folderu 'Saved'!\n" +
                 "Upewnij się, czy ścieżka jest prawidłowa.");
        }
        if (!hasSaves)
        {
            Warn("Nie wykryto folderu na backupy!\n" +
                 "Upewnij się, czy ścieżka jest prawidłowa.");
        }
        return false;
    }


}
