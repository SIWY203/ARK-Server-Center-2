namespace Ark_Server_Center;
using System.Diagnostics; // Process
using static MessageManager;
using static PathManager;


public static class SafetyChecker
{
    public static bool IsServerRunning(bool logging = false)
    {
        // ArkAscendedServer
        Process[] serverProcess = Process.GetProcessesByName("ArkAscendedServer");
        if (serverProcess.Length > 0 )
        {
            if (logging) Warn("Serwer jest włączony.");
            return true;
        }
            
        else
        {
            if (logging) Success("Serwer jest wyłączony.");
            return false;
        }
    }

    public static bool ExistGameSavePath(bool logging = false)
    {
        if (PathTo_Saved == string.Empty)
        {
            if (logging) Warn("Nie ustawiono ścieżki do 'Saved'.");
            return false;
        }
        return true;
    }

    public static bool ExistBackupPath(bool logging = false)
    {

        if (PathTo_SAVES == string.Empty)
        {
            if (logging) Warn("Nie ustawiono ścieżki do backupów.");
            return false;
        }
        return true;
    }

    public static bool IsSafeNow()
    {
        bool isRunning = IsServerRunning(true);
        bool hasGameSavePath = ExistGameSavePath(true);
        bool hasBackupPath = ExistBackupPath(true);
        // Log($"Is safe now?: { !isRunning && hasGameSavePath && hasBackupPath}");
        return !isRunning && hasGameSavePath && hasBackupPath;
    }

    public static bool ExistFolders(bool logging = true)
    {
        if (!Directory.Exists(Path.Combine(PathTo_Saved, "Saved")))
        {   
            Console.Clear();
            if (logging) Warn("Nie wykryto folderu 'Saved'!\nUpewnij się, czy ścieżka jest prawidłowa.");
            return false;
        }

        if (!Directory.Exists(Path.Combine(PathTo_SAVES, "SAVES")))
        {
            Console.Clear();
            if (logging) Warn("Nie wykryto folderu na backupy!\nUpewnij się, czy ścieżka jest prawidłowa.");
            return false;
        }
            
        return true;
    }

}
