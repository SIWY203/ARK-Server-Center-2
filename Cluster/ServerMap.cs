namespace ArkServerCenter.Cluster;
using static MessageManager;


public static class ServerMap
{
    public static string? AskForServerMap(string header = "")
    {
        string mapName;

        while (true)
        {
            Console.Clear();
            Console.WriteLine(header);
            Console.Clear();
            ArkMaps.ShowMapMenu();
            int mapCount = ArkMaps.GetMapCount();
            Console.WriteLine($"\n[{mapCount + 1}] Inna mapa");
            Console.WriteLine($"\n[{mapCount + 2}] Anuluj");

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
                Console.WriteLine($"Wpisz \"Q\" aby wyjść z kreatora.\n");
                Console.Write("Podaj nazwę mapy: ");
                mapName = Console.ReadLine()?.Trim() ?? "";

                if (mapName.ToUpper() == "Q")
                {
                    Console.Clear();
                    Console.WriteLine("Anulowano tworzenie klastra.");
                    End(); return null;
                }

                if (string.IsNullOrWhiteSpace(mapName))
                {
                    Console.Clear();
                    Error("Nieprawidłowa nazwa mapy!");
                    End(); continue;
                }
                if (SafetyChecker.HasInvalidChars(mapName))
                {
                    Console.Clear();
                    Error("Nazwa mapy zawiera niedozwolone znaki!");
                    End(); continue;
                }
                else
                {
                    Console.Clear();
                    Success($"Wybrano mapę {mapName}");
                    End(); return mapName;
                }

                
            }

            else if (input == $"{mapCount + 2}")
            {
                Console.Clear();
                Console.WriteLine("Anulowano.");
                End(); return null;
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
