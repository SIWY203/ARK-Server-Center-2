namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using static MessageManager;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Ark Server Center";

        RootPath.Load();
        RootPath.Setup(); // if path is not set
        ClusterManager.LoadClusters();


        // -----------------------------
        // TO DO
        //
        // - globalny adres IP
        // - skróty .bat dla serwerów
        // - język angielski
        //
        // - AddClusterFromFiles() - dodaje do pliku json z obcego folderu
        // - szablony dla ini i launch
        //
        // - Main(args) użyć do uruchomienia danego serwera/serwerów tą aplikacją za pomocą batcha
        // - przenieść wszystkie menu do klasy Menu, uporządkować, może skorzystać z jednego wzorca?
        // -----------------------------



        bool repeat = true;
        while (repeat)
        {
            while (ClusterManager.ActiveCluster == null || ClusterManager.ActiveServer == null)
            {
                ClusterManager.SelectClusterAndServer();
            }

            // ------------------------------
            //  Server Menu
            // ------------------------------
            Console.Clear();
            bool isSafeNow = SafetyChecker.IsSafeNow(ClusterManager.ActiveServer.Port);

            Console.WriteLine(
                $"\n" +
                $"============ Menu Serwera ===========\n" +
                $"Klaster: {ClusterManager.ActiveCluster.Name}, Mapa: {ClusterManager.ActiveServer.Map} \n" +
                $"Port: {ClusterManager.ActiveServer.Port}\n" +
                $"\n" +
                $"[1] Uruchom serwer\n" +
                $"[2] Backupy i przywracanie\n" +
                $"[3] Konfiguracja serwera\n" +
                $"[4] Aktualizacja Serwera\n" + 
                $"[Q] Wróć\n" +
                $"\n" +
                $"=====================================\n"
            );

            Console.Write("Wybierz: ");
            string choice = (Console.ReadLine() ?? "").ToUpper();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ServerLauncher.Launch();
                    continue;

                case "2":
                    Console.Clear();
                    BackupMenu(ClusterManager.ActiveServer);
                    continue;

                case "3":
                    Console.Clear();
                    ServerConfigMenu(ClusterManager.ActiveServer);
                    continue;

                case "4":
                    Console.Clear();
                    SteamCmdManager.UpdateServer(ClusterManager.ActiveServer);
                    continue;

                case "Q":
                    ClusterManager.ActiveServer = null;
                    continue;

                default:
                    Console.Clear();
                    Error("Nieprawidłowy wybór!");
                    End();
                    continue;
            }
        }
    }



    // ------------------------------
    //  Backup Menu
    // ------------------------------

    private static void BackupMenu(ClusterServer server)
    {
        bool isSafeNow = SafetyChecker.IsSafeNow(server.Port);
        bool repeat = true;
        while (repeat)
        {
            Console.WriteLine(
                $"\n" +
                $"===== Menu Backupów =====\n" +
                $"[1] Nowy zapis\n" +
                $"[2] Przywróć zapis\n" +
                $"[3] Cofnij przywracanie\n" +
                $"[Q] Wróć\n" +
                $"\n" +
                $"=========================\n"
           );
            Console.Write("Wybierz: ");
            string choice = (Console.ReadLine() ?? "").ToUpper();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    if (isSafeNow) BackupManager.CreateBackup(ClusterManager.ActiveServer);
                    else { Console.Clear(); Error("W tej chwili nie można utworzyć nowego zapisu!"); End(); }
                    continue;

                case "2":
                    if (isSafeNow) BackupManager.RestoreBackup(ClusterManager.ActiveServer);
                    else { Console.Clear(); Error("W tej chwili nie można przywrócić poprzedniego zapisu!"); End(); }
                    continue;

                case "3":
                    if (isSafeNow) BackupManager.RestoreSnapshot(ClusterManager.ActiveServer);
                    else { Console.Clear(); Error("W tej chwili nie można cofnąć przywracania!"); End(); }
                    continue;

                case "Q":
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

    private static void ServerConfigMenu(ClusterServer server)
    {
        bool isSafeNow = SafetyChecker.IsSafeNow(server.Port);
        bool repeat = true;
        while (repeat)
        {
            Console.Clear();
            Console.WriteLine(
               $"\n" +
               $"==== Konfiguracja Serwera ====\n" +
               $"[1] Parametry rozruchowe\n" +
               $"[2] GameUserSettings.ini\n" +
               $"[3] Game.ini\n" +
               $"[4] Folder serwera\n" +
               $"[Q] Wróć\n" +
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
                    LaunchMenu();
                    continue;
                case "2":
                    Console.Clear();
                    if (!isSafeNow) 
                    {  
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!"); 
                        End("OK! - Kliknij by przejść dalej... "); 
                    }
                    FileManager.OpenFile(Path.Combine(server.SavedPath, "Config", "WindowsServer", "GameUserSettings.ini"));
                    continue;

                case "3":
                    Console.Clear();
                    if (!isSafeNow)
                    {
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!");
                        End("OK! - Kliknij by przejść dalej... ");
                    }
                    FileManager.OpenFile(Path.Combine(server.SavedPath, "Config", "WindowsServer", "Game.ini"));
                    continue;

                case "4":
                    Console.Clear();
                    if (!isSafeNow)
                    {
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!");
                        End("OK! - Kliknij by przejść dalej... ");
                    }
                    FileManager.OpenFolder(server.SavedPath);
                    continue;

                case "Q":
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
    //  Launch Menu
    // ------------------------------
    public static void LaunchMenu()
    {
        bool repeat = true;
        while (repeat)
        {
            Console.Clear();
            Console.WriteLine(
               $"\n" +
               $"==== Konfiguracja Rozruchu ====\n" +
               $"[1] Wyświetl parametry\n" +
               $"[2] Otwórz plik\n" +
               $"[3] Resetuj plik\n" +
               $"[4] Backup pliku\n" +
               $"[Q] Wróć\n" +
               $"\n" +
               $"===============================\n"
           );
            Console.Write("Wybierz: ");
            string choice = (Console.ReadLine() ?? "").ToUpper();
            Console.WriteLine();

            ClusterServer? server = ClusterManager.ActiveServer;
            if (server == null) { Error("Nie ma aktywnego serwera!"); End(); return; }
            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ServerLauncher.LoadConfig(server).ShowConfig();
                    continue;

                case "2":
                    Console.Clear();
                    if (SafetyChecker.IsServerRunningOnPort(server.Port))
                    { 
                        Warn($"Serwer {server.VisibleMap} ({server.Port}) jest włączony!"); 
                    }
                    ServerLauncher.OpenConfigFile(ServerLauncher.LoadConfig(server));
                    continue;

                case "3":
                    Console.Clear();
                    if (SafetyChecker.IsServerRunningOnPort(server.Port))
                    {
                        Warn($"Serwer {server.VisibleMap} ({server.Port}) jest włączony! Anulowano!"); 
                    }
                    else ServerLauncher.DeleteConfig(ServerLauncher.LoadConfig(server));
                    continue;

                case "4":
                    Console.Clear();
                    ServerLauncher.LoadConfig(server).CreateConfigBackup();
                    continue;

                case "Q":
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


    public static void GlobalSettingsMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========= ARK SERVER CENTER =========");
            Console.WriteLine("Personalizacja globalnych ustawień\n");
            Console.WriteLine("[1] Aktualizacja"); 
            Console.WriteLine("[2] Adres IP");
            Console.WriteLine("[3] Folder główny");
            Console.WriteLine("[4] Skróty");
            Console.WriteLine("[5] Język (lang)");
            Console.WriteLine("[Q] Wyjdź");
            Console.Write("\nWybierz: ");

            string? input = Console.ReadLine()?.ToUpper();
            if (input == "Q")
            {
                Console.Clear();
                ClusterManager.ActiveCluster = null;
                break; // exit program
            }

            if (input == "1")
            {
                Console.Clear();
                SteamCmdManager.UpdateAllServers();
                continue;
            }

            if (input == "2")
            {
                Console.Clear();
                Console.WriteLine("dostępne wkrótce..."); End();
                continue;
            }

            if (input == "3")
            {
                Console.Clear();
                RootPath.ChangePath();
                continue;
            }

            if (input == "4")
            {
                Console.Clear();
                Console.WriteLine("dostępne wkrótce..."); End();
                continue;
            }

            if (input == "5")
            {
                Console.Clear();
                Console.WriteLine("dostępne wkrótce..."); End();
                continue;
            }

        }
    }


    // ------------------------------
    //  Instruction
    // ------------------------------

    private static void Instruction()
    {
        Console.WriteLine("aktualnie niedostępne...");
        End();
    }

}
