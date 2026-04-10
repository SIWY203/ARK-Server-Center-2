namespace ArkServerCenter.Cluster;
using static MessageManager;


public static class ClusterServerMap
{
    public static string AskForClusterServerMap()
    {
        string mapName;

        while (true)
        {
            ArkMaps.ShowMapMenu();
            int mapCount = ArkMaps.GetMapCount();
            Console.WriteLine($"\n[{mapCount + 1}] Inna mapa");

            Console.Write("\nWybierz: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= mapCount)
            {
                mapName = ((ArkMaps.Map)(choice - 1)).ToString();
                Console.Clear();
                Success($"Wybrano mapę {mapName}");
                End(); return mapName;
            }

            else if (input == $"{mapCount + 1}")
            {
                Console.Write("Wpisz nazwę mapy: ");
                mapName = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(mapName))
                {
                    Console.Clear();
                    Error("Nieprawidłowa nazwa mapy!");
                    End();
                }
                if (SafetyChecker.HasInvalidChars(mapName))
                {
                    Console.Clear();
                    Error("Nazwa mapy zawiera niedozwolone znaki!");
                    End();
                }
                else
                {
                    Console.Clear();
                    Success($"Wybrano mapę {mapName}");
                    End(); return mapName;
                }
            }
            else
            {
                Console.Clear();
                Error("Nieprawidłowy wybór mapy!");
                End();
            }
        }
    }
}
