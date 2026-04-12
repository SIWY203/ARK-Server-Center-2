namespace ArkServerCenter;
using ArkServerCenter.Cluster;
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
        // - UpdateClusterServer(), bez tego nie ma 'Saved' itd
        // - MoveCluster() - może użyć FileManager.MoveDirectory()? Potrzebne do ChangeRootPath()
        // - ChangeRootPath() - przenosi plik json i wszystkie klastry do nowej lokalizacji, aktualizuje RootPath.Value
        // - AddClusterFromFiles() - dodaje do pliku json z obcego folderu
        //
        // - konfig parametrów startowych serwera, np. ilość slotów, port, mapa, itd.
        // - poprawić UX: np. wszędzie 'Wróć' tylko w menu kastrów 'Wyjdź'
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
                $"[1] Backupy i przywracanie\n" +
                $"[2] Konfiguracja serwera\n" + 
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
                    BackupMenu(ClusterManager.ActiveServer);
                    continue;

                case "2":
                    Console.Clear();
                    ServerConfigMenu(ClusterManager.ActiveServer);
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

    private static void ServerConfigMenu(ClusterServer server)
    {
        bool isSafeNow = SafetyChecker.IsSafeNow(server.Port);
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
                    FileManager.OpenFile(Path.Combine(server.SavedPath, "Config", "WindowsServer", "GameUserSettings.ini"));
                    continue;

                case "2":
                    Console.Clear();
                    if (!isSafeNow)
                    {
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!");
                        End("OK! - Kliknij by przejść dalej... ");
                    }
                    FileManager.OpenFile(Path.Combine(server.SavedPath, "Config", "WindowsServer", "Game.ini"));
                    continue;

                case "3":
                    Console.Clear();
                    if (!isSafeNow)
                    {
                        Warn("W tej chwili serwer jest włączony, nie modyfikuj plików!");
                        End("OK! - Kliknij by przejść dalej... ");
                    }
                    FileManager.OpenFolder(server.SavedPath);
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
