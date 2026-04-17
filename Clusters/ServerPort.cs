namespace ArkServerCenter.Clusters;
using static MessageManager;


public static class ServerPort
{
    public static int? AskForServerPort(string header)
    {
        int startPort = 7777;

        var occupiedPorts = ClusterManager.Clusters
            .SelectMany(c => c.Servers)
            .Select(s => s.Port)
            .ToHashSet(); // unikalna kolekcja

        int candidate = startPort;
        while (occupiedPorts.Contains(candidate))
        {
            candidate += 2;
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine(header);
            Console.WriteLine($"Pierwszy dostępny port: {candidate}");
            Console.WriteLine(
                $"[1] Potwierdź\n" +
                $"[2] Inny port\n" +
                $"[3] Anuluj"
            );

            Console.Write("\nWybierz: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input == "1")
            {
                Console.Clear();
                Success($"Przypisano port: {candidate}");
                End(); return candidate;
            }

            else if (input == "2")
            {
                Console.Clear();
                Console.Write("Zajęte porty: ");
                foreach (int p in occupiedPorts)
                {
                    Console.Write($"{p} ");
                }
                Console.Write("\nWpisz: ");
                input = Console.ReadLine()?.Trim() ?? "";

                if (int.TryParse(input, out int userPort))
                {
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.Clear();
                        Error("Nie wpisano portu!");
                        End(); continue;
                    }

                    else if (userPort % 2 == 0)
                    {
                        Console.Clear();
                        Error("Port musi być nieparzysty!");
                        End(); continue;
                    }

                    else if (occupiedPorts.Contains(userPort))
                    {
                        Console.Clear();
                        Error($"Port {userPort} jest zajęty!");
                        End(); continue;
                    }

                    else
                    {
                        Console.Clear();
                        Success($"Przypisano port: {userPort}");
                        End(); return userPort;
                    }
                    
                }
                else
                {
                    Console.Clear();
                    Error("Nieprawidłowy port!");
                    End(); continue;
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
