namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using System.Text.Json;
using static MessageManager;

public static class Mods
{
    private static readonly string _namesPath = "known_mods_names.json";
    private static Dictionary<string, string> KnownModsNames { get; set; } = new(); 
    // słownik przechowuje relację ID -> Nazwa (dla celów wizualnych)


    private static void LoadNames()
    {
        if (!File.Exists(_namesPath)) return;
        try
        {
            string json = File.ReadAllText(_namesPath);
            KnownModsNames = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
        catch { }
    }


    private static void SaveNames()
    {
        string json = JsonSerializer.Serialize(KnownModsNames, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_namesPath, json);
    }


    public static void ModMenu(ServerConfig config)
    {
        LoadNames();

        while (true)
        {
            // wyciągnięcie listy modów z configu serwera
            string modsFlag = config.Flags.FirstOrDefault(f => f.StartsWith("-Mods=", StringComparison.OrdinalIgnoreCase)) ?? "-Mods=";
            string idsString = modsFlag.Replace("-Mods=", "").Trim();

            // lista aktywnych modów (ID) dla aktywnego serwera
            List<string> activeModIds = string.IsNullOrWhiteSpace(idsString)
                ? new List<string>()
                : idsString.Split(',').ToList();

            Console.Clear();
            Console.WriteLine($"-----------------------------------------------------------");
            Console.WriteLine($" Zarządzanie modami serwera: {config.Server.VisibleMap} ({config.Server.Port})");
            Console.WriteLine($" Mody zapisywane są bezpośrednio w pliku rozruchowym!");
            Console.WriteLine($"-----------------------------------------------------------\n");

            if (activeModIds.Count == 0) Console.WriteLine("Lista modów jest pusta.");

            // nazwy ze słownika
            for (int i = 0; i < activeModIds.Count; i++)
            {
                string id = activeModIds[i];
                string name = KnownModsNames.ContainsKey(id) ? KnownModsNames[id] : "Nieznana nazwa";
                Console.WriteLine($"{i + 1}. [ID: {id}] - {name}");
            }

            Console.WriteLine(
                "\n" +
                "[1] Dodaj moda\n" +
                "[2] Usuń moda\n" +
                "[Q] Wróć\n");
            Console.Write("Wybierz: ");
            string input = Console.ReadLine()?.ToUpper() ?? "";

            if (input == "Q") break;

            if (input == "1")
            {
                Console.Clear();
                Console.Write("Podaj ID moda: ");
                string id = Console.ReadLine()?.Trim() ?? "";
                Console.Write("Podaj nazwę (opcjonalnie): ");
                string name = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(name)) name = "Nieznany mod";

                if (string.IsNullOrWhiteSpace(id))
                {
                    Console.Clear(); Error("Nie dodano moda!"); End(); continue;
                }

                Console.Clear();
                Console.Write(
                    "Czy dodać tego moda również do pozostałych serwerów w klastrze?\n" +
                    "[T] Tak\n" +
                    "[N] Nie\n");
                Console.Write("Wybierz: ");
                bool sync = Console.ReadLine()?.ToUpper().Trim() == "T";

                // dodawanie moda, jeśli go tam nie ma
                if (!activeModIds.Contains(id))
                {
                    activeModIds.Add(id);
                    config.UpdateOrAddArg($"-Mods={string.Join(",", activeModIds)}");
                    config.SaveConfig();
                }

                if (sync)
                {
                    foreach (var s in ClusterManager.ActiveCluster?.Servers ?? new())
                    {
                        if (s.Port == config.Server.Port) continue;

                        ServerConfig otherConfig = ServerConfig.LoadConfig(s);
                        string otherModsFlag = otherConfig.Flags.FirstOrDefault(f => f.StartsWith("-Mods=", StringComparison.OrdinalIgnoreCase)) ?? "-Mods=";
                        List<string> otherIds = otherModsFlag.Replace("-Mods=", "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (!otherIds.Contains(id))
                        {
                            otherIds.Add(id);
                            otherConfig.UpdateOrAddArg($"-Mods={string.Join(",", otherIds)}");
                            otherConfig.SaveConfig();
                        }
                    }
                }

                // dodawanie moda do listy znanych modów
                KnownModsNames[id] = name;  // dictionary
                SaveNames();                // json file
                Console.Clear(); 
                Success("Zaktualizowano modyfikacje!"); 
                End();
            }

            else if (input == "2")
            {
                Console.Clear();
                Console.WriteLine("========= Usuwanie modyfikacji =========\n");
                for (int i = 0; i < activeModIds.Count; i++)
                {
                    string id = activeModIds[i];
                    string name = KnownModsNames.ContainsKey(id) ? KnownModsNames[id] : "Nieznana nazwa";
                    Console.WriteLine($"[{i + 1}] {name} (ID: {id})");
                }
                Console.WriteLine($"\nWpisz \"Q\" aby anulować.");
                Console.Write("\nWybierz: ");

                if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= activeModIds.Count)
                {
                    // pobieranie ID
                    string idToRemove = activeModIds[index - 1];

                    Console.Clear();
                    Console.Write(
                        "Czy usunąć tego moda również z pozostałych serwerów w klastrze?\n" +
                        "[T] Tak\n" +
                        "[N] Nie\n");
                    bool syncDeleting = Console.ReadLine()?.ToUpper().Trim() == "T";

                    // usuwanie z bieżącego obiektu
                    activeModIds.Remove(idToRemove);
                    config.UpdateOrAddArg($"-Mods={string.Join(",", activeModIds)}");
                    config.SaveConfig();

                    // usuwanie z pozostałych serwerów
                    if (syncDeleting)
                    {
                        foreach (var s in ClusterManager.ActiveCluster?.Servers ?? new())
                        {
                            if (s.Port == config.Server.Port) continue;

                            ServerConfig otherConfig = ServerConfig.LoadConfig(s);
                            string otherModsFlag = otherConfig.Flags.FirstOrDefault(f => f.StartsWith("-Mods=", StringComparison.OrdinalIgnoreCase)) ?? "-Mods=";
                            List<string> otherIds = otherModsFlag.Replace("-Mods=", "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                            if (otherIds.Contains(idToRemove))
                            {
                                otherIds.Remove(idToRemove);
                                otherConfig.UpdateOrAddArg($"-Mods={string.Join(",", otherIds)}");
                                otherConfig.SaveConfig();
                            }
                        }
                    }

                    Console.Clear();
                    Success("Usunięto moda!");
                    End();
                }
            }

        }
    }
}