namespace Ark_Server_Center;
using static MessageManager;
using static PathManager;

public class Program
{
    public static int port = 7777;

    public static void Main(string[] args)
    {
        Console.Title = "Ark Server Center";

        // ------------------------------
        //  Main Menu
        // ------------------------------
        bool repeat = true;
        

        // Ustalenie portu, klastra etc.


        while (repeat)
        {
            Console.Clear();
            LoadPathsFromFile();
            SafetyChecker.CheckFolders();
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
