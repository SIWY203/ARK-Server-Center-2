namespace ArkServerCenter.Cluster;
using static MessageManager;


public static class ClusterName
{
    public static string AskForClusterName()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n======== Kreator Klastrów ========\n");
            Console.Write("Podaj Nazwę klastra: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (SafetyChecker.HasInvalidChars(input))
            {
                Console.Clear();
                Error("Nazwa klastra zawiera niedozwolone znaki!");
                End(); continue;
            }

            else if (string.IsNullOrWhiteSpace(input))
            {
                Console.Clear();
                Error("Wpis nie może być pusty!");
                End(); continue;
            }
            else
            {
                return input;
            }
        }
    }
}
