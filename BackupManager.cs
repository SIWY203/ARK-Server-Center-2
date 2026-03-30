namespace Ark_Server_Center;
using static MessageManager;
using static PathManager;

public static class BackupManager
{
    public static void CreateBackup()
    {
        Console.Clear();
        string savedDir = Path.Combine(PathTo_Saved, "Saved");
        string dirName = "Saved-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string backupDir = Path.Combine(PathTo_SAVES, "SAVES", dirName);

        if (!SafetyChecker.CheckFoldersAndLog()) { End(); return; }

        Console.WriteLine("Tworzenie zapisu, poczekaj na potwierdzenie...");
        bool success = CopyDirectory(savedDir, backupDir);
        if (success) Success($"Gotowe! Utworzono: {dirName}");
        End();
    }

    public static void RestoreBackup()
    {
        Console.Clear();
        string savedDir = Path.Combine(PathTo_Saved, "Saved");
        string databaseDir = Path.Combine(PathTo_Saved, "SAVES");
        string snapshotName = "Snapshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string snapshotDir = Path.Combine(PathTo_SAVES, "SAVES", "#Snapshots", snapshotName);
        

        if (!SafetyChecker.CheckFoldersAndLog()) { End(); return; }

        string latestBackupDir = GetLatestBackup(databaseDir);
        if (string.IsNullOrEmpty(latestBackupDir))
        {
            Console.Clear();
            Error("Brak backupów!");
            End(); return;
        }

        Console.WriteLine($"Najnowszy backup: {Path.GetFileName(latestBackupDir)}");

        Console.Write(
            "Czy napewno chcesz przywrócić poprzedni zapis?\n" +
            "Obecny stan zostanie zapisany jako snapshot.\n" +
            "\n[T] TAK \n[N] NIE \nWybierz [T/N]: ");
        string choice = Console.ReadLine() ?? "N";
        if (choice.ToUpper() != "T")
        {
            Console.Clear();
            Error("Przywracanie zostało anulowane.");
            End(); return;
        }

        Console.Clear();
        Console.WriteLine("Tworzenie snapshota obecnego zapisu...");
        bool success = CopyDirectory(savedDir, snapshotDir);
        if (success) Log($"Zapisano snapshot: {snapshotName}");
        else { End(); return; }

        Console.WriteLine("\nPrzywracanie poprzedniego zapisu...");
        success = CopyDirectory(latestBackupDir, savedDir);
        if (success) Success($"Gotowe! Przywrócono: {Path.GetFileName(latestBackupDir)}");
        End();
    }


    public static void RestoreSnapshot()
    {
        Console.Clear();
        string savedDir = Path.Combine(PathTo_Saved, "Saved");
        string snapshotDir = Path.Combine(PathTo_SAVES, "SAVES", "#Snapshots");
        Directory.CreateDirectory(snapshotDir);

        if (!SafetyChecker.CheckFoldersAndLog()) { End(); return; }

        string latest = GetLatestSnapshot(snapshotDir);
        if (string.IsNullOrEmpty(latest))
        {
            Console.Clear();
            Error("Brak snapshotów!");
            End(); return;
        }

        Console.WriteLine($"Najnowszy snapshot: {Path.GetFileName(latest)}");

        Console.Write(
            "Czy napewno chcesz cofnąć się do zapisu z przed przywrócenia backupu?\n" +
            "To przywróci stan gry z przed przywracania backupu.\n" +
            "Sam backup nadal będzie możliwy do przywrócenia.\n" +
            "\n[T] TAK, cofnij przywracanie \n[N] NIE, zostaw jak jest \n\nWybierz [T/N]: ");
        string choice = Console.ReadLine() ?? "N";
        if (choice.ToUpper() != "T")
        {
            Console.Clear();
            Error("Cofanie backupu zostało anulowane.");
            End(); return;
        }

        Console.Clear();
        Console.WriteLine("Przywracanie zapisu zastąpionego backupem...");
        bool success = CopyDirectory(latest, savedDir);
        if (success) Success($"Gotowe! Przywrócono: {Path.GetFileName(latest)}");
        End();
    }



    // ------------------------------
    //  Metody pomocnicze
    // ------------------------------
    public static bool CopyDirectory(string sourceDir, string targetDir)
    {
        if (!SafetyChecker.CheckFoldersAndLog())
        {
            Console.Clear();
            Error("Nieprawidłowa ścieżka, niepowodzenie!");
            return false;
        }

        string tempDir = $"{targetDir}_temp";

        try
        {
            // 1. czyszczenie temp
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            // 2. kopiowanie do temp
            CopyDirectoryCore(sourceDir, tempDir);

            // 3. sprawdzenie czy aby folder nie jest pusty
            if (Directory.GetFileSystemEntries(tempDir).Length == 0)
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Error($"Folder tymczasowy jest pusty: \n{tempDir} \nOperacja przerwana.");
                return false;
            } 

            // 4. usuwanie folderu docelowego
            if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);

            // 5. przeniesienie do celu
            Directory.Move(tempDir, targetDir);
        }

        catch (Exception ex)
        {
            Error($"{ex.Message}");
            Console.WriteLine("Cofanie zmian... ");

            // usuwanie uszkodzonych danych
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            return false;
        }
        return true;

    }


    public static void CopyDirectoryCore(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        // kopia plików w folderze
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile, true);
        }

        // rekurencyjne kopiowanie podfolderów
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDir);
            string targetSubDir = Path.Combine(targetDir, subDirName);
            CopyDirectoryCore(subDir, targetSubDir); // rekurencja (f. wywołuje samą siebie)
        }

    }


    public static string GetLatestBackup(string path)
        => GetLatestFromDir("backup", path);

    public static string GetLatestSnapshot(string path)
        => GetLatestFromDir("snapshot", path);

    private static string GetLatestFromDir(string operation, string path)
    {
        if (!SafetyChecker.CheckFoldersAndLog()) return string.Empty;

        // ignorowanie folderu "#Snapshots"
        string[] entries = Directory.GetDirectories(path)
            .Where(d => !d.EndsWith("#Snapshots"))
            .ToArray();

        // jeżeli pusty folder
        if (entries.Length == 0)
        {
            Warn($"Brak {operation}ów do przywrócenia!");
            return string.Empty;
        }

        // sortowanie rosnąco, ostatni element
        Array.Sort(entries);
        return entries[entries.Length - 1];
    }


}
