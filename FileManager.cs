namespace ArkServerCenter;
using System.Diagnostics; // Process
using static MessageManager;


public static class FileManager
{
    public static void OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Error($"Plik '{new FileInfo(filePath).Name}' nie istnieje, lub ścieżka jest nieprawidłowa!");
            End(); return;
        }

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            Process.Start(psi);
            Log("Otwarto plik: " + Path.GetFileName(filePath));
            End();
        }

        catch (Exception ex)
        {
            Error("Nie udało się otworzyć pliku: " + ex.Message);
            End();
        }
    }



    public static void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });

            Log("Otwarto folder: " + Path.GetFileName(folderPath));
            End();
        }
        else
        {
            Error($"Folder '{ new DirectoryInfo(folderPath).Name }' nie istnieje, lub ścieżka jest nieprawidłowa!");
            End(); return;
        }
    }

}
