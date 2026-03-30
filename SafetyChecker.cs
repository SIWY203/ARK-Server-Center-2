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

    // do refaktoryzacji
    public static bool ExistGameSavePath(bool logging = false)
    {
        if (PathTo_Saved == string.Empty)
        {
            if (logging) Warn("Nie ustawiono ścieżki do 'Saved'.");
            return false;
        }
        return true;
    }

    // do refaktoryzacji
    public static bool ExistBackupPath(bool logging = false)
    {

        if (PathTo_SAVES == string.Empty)
        {
            if (logging) Warn("Nie ustawiono ścieżki do backupów.");
            return false;
        }
        return true;
    }

    public static bool IsSafeNow(int port)
    {
        bool isRunning = IsServerRunningOnPort(port);
        if (isRunning) Warn("Serwer jest włączony!");
        else Success("Serwer jest wyłączony.");

        bool hasGameSavePath = ExistGameSavePath(true);
        bool hasBackupPath = ExistBackupPath(true);
        return !isRunning && hasGameSavePath && hasBackupPath;
    }

    public static (bool HasSaved, bool HasSaves) CheckFolders()
    {
        bool hasSaved = Directory.Exists(Path.Combine(PathTo_Saved, "Saved"));
        bool hasSaves = Directory.Exists(Path.Combine(PathTo_SAVES, "SAVES"));

        return (hasSaved, hasSaves);

    }

    public static bool CheckFoldersAndLog()
    {
        var (hasSaved, hasSaves) = CheckFolders();
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
