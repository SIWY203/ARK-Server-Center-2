namespace ArkServerCenter.Cluster;
using static MessageManager;


public static class ClusterPath
{
    public static string? AskForClusterPath()
    {
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
                Console.Clear();
                Success("Zapisano ścieżkę!");
                End(); return RootPath.Value;
            }

            else if (input == "2")
            {
                Console.Clear();
                string? newPath = FileManager.AskForFolderPath("Ustawianie niestandardowej ścieżki.");
                if (newPath == null) return null; // anuluj

                else
                {
                    Console.Clear();
                    Success("Zapisano ścieżkę!");
                    End(); return newPath;
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
