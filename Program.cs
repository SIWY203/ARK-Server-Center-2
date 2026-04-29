namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using ArkServerCenter.GlobalSettings;
using static MessageManager;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Ark Server Center";

        RootPath.Setup();
        Address.LoadIPAddress();
        ClusterManager.LoadClusters();


        // -----------------------------
        // TO DO
        //
        // - skróty .bat dla serwerów za pomocą Main(args)?
        // - import modów (do parametrów startu)
        // - język angielski
        //
        // - podpowiedzi dla konsoli arka + puszczanie komend
        // - ?QueryPort=27015++, ?RCONEnabled=True?RCONPort=27020, lub lepiej 28015
        // - szablony dla ini i launch
        //
        // - z Main() przenieść kod i zrobić ServerMenu(), BackupMenu() itd + naprawić wychodzenie z serwer menu do listy serwerów a nie klastrów
        // - może usprawnić dodawanie klastrów z pliku przez podawanie ręcznie folderu z serwerem itd, potem validacja czy sie zgadza?
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
            ServerLauncher.LoadConfig(ClusterManager.ActiveServer); // wczytaj launcher config

            Console.Clear();
            SafetyChecker.IsSafeNow(ClusterManager.ActiveServer.Port);

            Console.WriteLine(
                $"\n" +
                $"============ Menu Serwera ===========\n" +
                $"Klaster: {ClusterManager.ActiveCluster.Name}, Mapa: {ClusterManager.ActiveServer.Map} \n" +
                $"IP: {Address.IpAddress} Port: {ClusterManager.ActiveServer.Port}\n" +
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
                    Updater.UpdateServer(ClusterManager.ActiveServer);
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




    // ------------------------------
    //  Instruction
    // ------------------------------

    private static void Instruction()
    {
        Console.WriteLine("aktualnie niedostępne...");
        End();
    }

}
