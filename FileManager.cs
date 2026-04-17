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



    public static string? AskForFolderPath(string? message = null)
    {
        while (true)
        {
            if (message != null) Console.WriteLine($"{message}\n");
            Console.WriteLine("Wpisz \"Q\", aby wyjść...\n");
            Console.Write("Wpisz ścieżkę lub przeciągnij folder: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            input = input.Replace("\"", ""); // Upuszczenie dodaje cudzysłowy gdy w ścieżce jest spacja

            if (!string.IsNullOrWhiteSpace(input) && Directory.Exists(input))
            {
                Console.Clear();
                Success($"Zapisano: {input}");
                return input;
            }

            if (input.ToUpper() == "Q")
            {
                return null; // anuluj
            }

            Console.Clear();
            Error("Błąd: Podana ścieżka nie istnieje lub jest nieprawidłowa!");
            End(); continue;
        }
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


}
