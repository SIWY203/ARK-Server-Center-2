namespace ArkServerCenter.Cluster;
using static MessageManager;


public static class ClusterName
{
    public static string? AskForClusterName()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n======== Kreator Klastrów ========\n");
            Console.WriteLine($"Wpisz \"Q\" aby wyjść z kreatora.\n");
            Console.Write("Podaj Nazwę klastra: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            if (name.ToUpper() == "Q")
            {
                Console.Clear();
                Console.WriteLine("Anulowano tworzenie klastra.");
                End(); return null;
            }

            else if (SafetyChecker.HasInvalidChars(name))
            {
                Console.Clear();
                Error("Nazwa klastra zawiera niedozwolone znaki!");
                End(); continue;
            }

            else if (string.IsNullOrWhiteSpace(name))
            {
                Console.Clear();
                Error("Wpis nie może być pusty!");
                End(); continue;
            }

            else
            {
                return name;
            }
        }
    }
}
