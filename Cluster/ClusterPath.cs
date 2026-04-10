namespace ArkServerCenter.Cluster;
using static MessageManager;


public static class ClusterPath
{
    public static string? AskForClusterPath()
    {
        string clusterPath;

        if (string.IsNullOrWhiteSpace(RootPath.Value))
        {
            while (string.IsNullOrWhiteSpace(RootPath.Value))
            {
                Console.Clear();
                Console.WriteLine($"\n======== Kreator Klastrów ========\n");
                Warn("Jeszcze nie ustawiono domyślnej ścieżki!\n");
                RootPath.Value = FileManager.GetFolderPath() ?? string.Empty;
            }
            Console.Clear();
            Success("Scieżka główna została zapisana!");
            End();
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n======== Kreator Klastrów ========\n");
            Console.WriteLine($"Domyślna ścieżka: {RootPath.Value}\n");
            Console.WriteLine(
                "[1] Zastosuj\n" +
                "[2] Ustaw inną\n" +
                "[3] Anuluj\n");

            Console.Write("Wybierz: ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (input == "1")
            {
                clusterPath = RootPath.Value;
                Console.Clear();
                Success("Zapisano ścieżkę!");
                End(); return clusterPath;
            }

            else if (input == "2")
            {
                string? newPath = FileManager.GetFolderPath();
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    clusterPath = newPath;
                    Console.Clear();
                    Success("Zapisano ścieżkę!");
                    End(); return clusterPath;
                }
            }

            else if (input == "3")
            {
                Console.Clear();
                Console.WriteLine("Anulowano tworzenie klastra.");
                End(); return null;
            }

            else
            {
                Console.Clear();
                Error("Nieprawidłowy wybór, spróbuj ponownie.");
                End();
            }

        }
    }
}
