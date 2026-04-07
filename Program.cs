namespace Ark_Server_Center;
using static MessageManager;
using static PathManager;

public class Program
{
    public static int port = 7777;
    public static ArkCluster? ActiveCluster { get; private set; }
    public static ClusterServer? ActiveServer { get; private set; }

    public static void Main(string[] args)
    {
        Console.Title = "Ark Server Center";


        // Ustalenie portu, klastra etc.
        SelectCluster();
        SelectClusterServer(ActiveCluster);
        ActiveCluster?.ShowClusterInfo();
        ActiveServer?.ShowServerInfo();
        Console.ReadLine();


        // ------------------------------
        //  Main Menu
        // ------------------------------
        bool repeat = true;
        while (repeat)
        {
            Console.Clear();
            LoadPathsFromFile();
            bool isSafeNow = SafetyChecker.IsSafeNow(port);

            Console.WriteLine(
                $"\n" +
                $"========= Menu Główne =========\n" +
                $"[1] Backupy i przywracanie\n" +
                $"[2] Konfiguracja serwera\n" +
                $"[3] Ustawienia ścieżek\n" +
                $"[4] Instrukcja\n" +
                $"[Q] Wyjście\n" +
                $"\n" +
                $"===============================\n"
            );

            Console.Write("Wybierz: ");
            string choice = (Console.ReadLine() ?? "").ToUpper();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    BackupMenu();
                    continue;

                case "2":
                    Console.Clear();
                    ServerConfigMenu();
                    continue;

                case "3":
                    Console.Clear();
                    PathDetails();
                    continue;

                case "4":
                    Console.Clear();
                    Instruction();
                    continue;

                case "Q":
                    Environment.Exit(0);
                    break;

                default:
                    Console.Clear();
                    Error("Nieprawidłowy wybór!");
                    End();
                    continue;
            }
        }
    }



    private static void SelectCluster()
    {
        // 1. Przykładowe dane (docelowo dane z dysku)
        List<ArkCluster> clusters = new()
        {
            new ArkCluster("Cebula", @"D:\Gry\ARK Saves\ARK Server Cebula\Cluster Data", new List<ClusterServer>
            {
                new ClusterServer("TheIsland", 7777, @"D:\Gry\ARK Saves\ARK Server Cebula\Ark TheIsland"),
                new ClusterServer("Ragnarok", 7779, @"D:\Gry\ARK Saves\ARK Server Cebula\Ark Ragnarok")
            }),

            new ArkCluster("Pomidor", @"D:\Gry\ARK Saves\ARK Server Pomidor\Cluster Data", new List<ClusterServer>
            {
                new ClusterServer("Ragnarok", 7781, @"D:\Gry\ARK Saves\ARK Server Pomidor\Ark Ragnarok")
            })
        };


        while (true)
        {
            Console.Clear();
            Console.WriteLine("========= ARK SERVER CENTER =========");
            Console.WriteLine("Wybierz klaster do zarządzania:\n");

            for (int i = 0; i < clusters.Count; i++)
            {
                Console.WriteLine($"[{i+1}] Klaster: {clusters[i].Name} ({clusters[i].Servers.Count} serwerów)");
            }
            Console.WriteLine("[X] Wyjście");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "X") break;

            if (int.TryParse(input, out int choice) && choice-1 >= 0 && choice-1 < clusters.Count)
            {
                ActiveCluster = clusters[choice-1];
                break;
            }
        }
    }

    private static void SelectClusterServer(ArkCluster? cluster)
    {
        if (cluster == null)
        {
            Console.WriteLine("Nie wybrano klastra!");
            Console.ReadLine();
            return;
        }

        bool success = false;
        while (!success)
        {
            Console.Clear();
            Console.WriteLine($"--- KLASTER: {cluster.Name} ---");

            for (int i = 0; i < cluster.Servers.Count; i++)
            {
                var s = cluster.Servers[i];
                string status = SafetyChecker.IsServerRunningOnPort(s.Port) ? "[ONLINE]" : "[OFFLINE]";
                Console.WriteLine($"[{i+1}] {s.Map} (Port: {s.Port}) - {status}");
            }
            Console.WriteLine("[B] Powrót");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "B") break;      

            if (int.TryParse(input, out int choice) && choice-1 >= 0 && choice-1 < cluster.Servers.Count)
            {
                // Tutaj wchodzisz w menu konkretnego serwera (Start/Stop/Backup)
                //OpenServerControl(cluster.Servers[choice]);
                Console.WriteLine(cluster.Servers[choice-1].Map);
                Console.ReadLine();
                ClusterServer server = cluster.Servers[choice - 1];
                success = true;
                ActiveServer = server;
            }
        }
    }



    // ------------------------------
    //  Backup Menu
    // ------------------------------

    private static void BackupMenu()
    {
        bool isSafeNow = SafetyChecker.IsSafeNow(port);
        bool repeat = true;
        while (repeat)
        {
            Console.WriteLine(
                $"\n" +
                $"===== Menu Backupów =====\n" +
                $"[1] Nowy zapis\n" +
                $"[2] Przywróć zapis\n" +
                $"[3] Cofnij przywracanie\n" +
                $"[4] Wróć\n" +
                $"\n" +
                $"=========================\n"
           );
            Console.Write("Wybierz: ");
            string choice = (Console.ReadLine() ?? "").ToUpper();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    if (isSafeNow) BackupManager.CreateBackup();
                    else { Console.Clear(); Error("W tej chwili nie można utworzyć nowego zapisu!"); End(); }
                    continue;

                case "2":
                    if (isSafeNow) BackupManager.RestoreBackup();
                    else { Console.Clear(); Error("W tej chwili nie można przywrócić poprzedniego zapisu!"); End(); }
                    continue;

                case "3":
                    if (isSafeNow) BackupManager.RestoreSnapshot();
                    else { Console.Clear(); Error("W tej chwili nie można cofnąć przywracania!"); End(); }
                    continue;

                case "4":
                    repeat = false;
                    break;

                default:
                    Console.Clear();
                    Error("Nieprawidłowy wybór!");
                    End();
                    continue;
            }

        }
    }


    // ------------------------------
    //  Server Config Menu
    // ------------------------------

    private static void ServerConfigMenu()
    {
        bool isSafeNow = SafetyChecker.IsSafeNow(port);
        bool repeat = true;
        while (repeat)
        {
            Console.WriteLine(
               $"\n" +
               $"==== Konfiguracja Serwera ====\n" +
               $"[1] GameUserSettings.ini\n" +
               $"[2] Game.ini\n" +
               $"[3] Folder serwera\n" +
               $"[4] Wróć\n" +
               $"\n" +
               $"==============================\n"
           );
            Console.Write("Wybierz: ");
            string choice = (Console.ReadLine() ?? "").ToUpper();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    if (!isSafeNow) 
                    {  
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!"); 
                        End("OK! - Kliknij by przejść dalej... "); 
                    }
                    FileManager.OpenFile(Path.Combine(PathTo_Saved, "Saved", "Config", "WindowsServer", "GameUserSettings.ini"));
                    continue;

                case "2":
                    Console.Clear();
                    if (!isSafeNow)
                    {
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!");
                        End("OK! - Kliknij by przejść dalej... ");
                    }
                    FileManager.OpenFile(Path.Combine(PathTo_Saved, "Saved", "Config", "WindowsServer", "Game.ini"));
                    continue;

                case "3":
                    Console.Clear();
                    if (!isSafeNow)
                    {
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!");
                        End("OK! - Kliknij by przejść dalej... ");
                    }
                    FileManager.OpenFolder(Path.Combine(PathTo_Saved));
                    continue;

                case "4":
                    repeat = false;
                    break;

                default:
                    Console.Clear();
                    Error("Nieprawidłowy wybór!");
                    End();
                    continue;
            }

        }
    }


    // ------------------------------
    //  Instruction
    // ------------------------------

    private static void Instruction()
    {
        Console.WriteLine(
            $"========== INSTRUKCJA ==========" +
            $"\n" +
            $"\nCo robią poszczególne opcje?" +
            $"\n" +
            $"\n 1 - tworzy nowy backup z aktualnej rozgrywki." +
            $"\n 2 - przywraca backup i tworzy zapasową kopię aktualnie podmienianego" +
            $"\n     zapisu, aby można było go wrócić jeśli backup okaże się stary." +
            $"\n 3 - przywraca nadpisany powyższą opcją zapis, nawet po nowym backupie." +
            $"\n 4 - menu plików konfiguracyjnych serwera, w przyszłości będą tu parametry" +
            $"\n     startowe serwera i wszystkie inne potrzebne funkcje." +
            $"\n 5 - menu zarządzania ścieżkami. W każdej chwili można je zmienić." +
            $"\n");
        
        End();
    }

}
