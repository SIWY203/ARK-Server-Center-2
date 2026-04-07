//namespace Ark_Server_Center;
//using static MessageManager;

//public static class PathManager
//{
//    // 'Saved' folder path & 'SAVES' backup folder path
//    public static string PathTo_Saved { get; private set; } = string.Empty;
//    public static string PathTo_SAVES { get; private set; } = string.Empty;
//    public static string PathConfigFile { get; private set; } = "path_config.txt";


//    public static void LoadPathsFromFile()
//    {
//        if (File.Exists(PathConfigFile))
//        {
//            PathTo_Saved = File.ReadLines(PathConfigFile).FirstOrDefault() ?? string.Empty;
//            PathTo_SAVES = File.ReadLines(PathConfigFile).Skip(1).FirstOrDefault() ?? string.Empty;
//        }
//        else
//        {
//            // gdy brak pliku konfiguracyjnego
//            GetPath(0);
//            GetPath(1);
//        }
//    }



//    public static string GetPath(int index)
//    {
//        if (index == 0 && string.IsNullOrEmpty(PathTo_Saved))
//        {
//            Console.Clear();
//            Warn("Nie ustalono ścieżki do 'Saved'!");
//            SetPath(index);
//            return string.Empty;
//        }
//        else if (index == 0) return "Saved: " + PathTo_Saved;

//        if (index == 1 && string.IsNullOrEmpty(PathTo_SAVES))
//        {
//            Console.Clear();
//            Warn("Nie ustalono ścieżki do backupów!");
//            SetPath(index);
//            return string.Empty;
//        }
//        else if (index == 1) return "SAVES: " + PathTo_SAVES;

//        return string.Empty;
//    }



//    public static void SetPath(int index)
//    {
//        if (index == 0) // Saved
//        {
//            // validacja inputu
//            string? input = "";
//            do
//            {
//                Console.Clear();
//                Console.WriteLine(
//                    "Ustal i wpisz ścieżkę do folderu 'Saved'." +
//                    "\n" +
//                    "\nPowinien on się znajdować w takiej lokalizacji:" +
//                    "\n.../Ark Cluster/<mapa>/ShooterGame/Saved" +
//                    "\n" +
//                    "\nPrzykład prawidłowego formatu:" +
//                    "\nC:/Program Files/ArkServer/Ark Cluster/<mapa>/ShooterGame" +
//                    "\n" +
//                    "\n[1] Wróc do menu" +
//                    "\n"
//                    );

//                Console.Write("Podaj: ");
//                input = Console.ReadLine() ?? string.Empty;

//                if (input == "1")
//                {
//                    Console.Clear();
//                    return;
//                }

//            } while (!IsPathValid(ref input));

//            SaveConfigFile(index, Path.Combine(input));
//            Console.Clear();
//            Success("Ścieżka została zapisana!");
//            Success("Folder 'Saved': " + Path.Combine(PathTo_Saved, "Saved"));
//            End(); return;
//        }

//        if (index == 1) // SAVES
//        {
//            // validacja inputu
//            string? input = "";
//            do
//            {
//                Console.Clear();
//                Console.WriteLine(
//                    "Ustal i wpisz ścieżkę do folderu kopii zapasowych." +
//                    "\nDomyślna sugerowana lokalizacja to:" +
//                    "\n/Ark Cluster/mapa/ShooterGame" +
//                    "\n" +
//                    "\nprzykład prawidłowego formatu:" +
//                    "\nC/Program Files/ArkServer/Ark Cluster/<mapa>/ShooterGame" +
//                    "\n" +
//                    "\n[1] Wróc do menu" +
//                    "\n"
//                    );

//                Console.Write("Podaj: ");
//                input = Console.ReadLine() ?? string.Empty;

//                if (input == "1")
//                {
//                    Console.Clear();
//                    return;
//                }

//            } while (!IsPathValid(ref input));

//            Directory.CreateDirectory(Path.Combine(input, "SAVES"));
//            SaveConfigFile(index, input);
//            Console.Clear();
//            Success("Ścieżka została zapisana!");
//            Success("Folder 'SAVES': " + Path.Combine(PathTo_SAVES, "SAVES"));
//            End();  return;
//        }

//        Console.Clear();
//        Error("\nCoś poszło nie tak... Zły numer ścieżki!");
//        End();
//    }



//    public static bool IsPathValid(ref string input)
//    {
//        // czyszczenie inputu
//        input = input.Trim();
//        input = input.Replace("\"", "");
//        if (input.EndsWith(@"/")) input = input[0..^1];
//        if (input.EndsWith(@"\")) input = input[0..^1];

//        // weryfikacja inputu
//        if (string.IsNullOrWhiteSpace(input))
//        {
//            Console.Clear();
//            Warn("Nie podano ścieżki!");
//            End(); return false;
//        }

//        if (!Directory.Exists(input))
//        {
//            Console.Clear();
//            Warn("Folder nie istnieje!");
//            End();  return false;
//        }

//        return true;
//    }



//    public static void SaveConfigFile(int lineIndex, string newPath)
//    {
//        string[] lines;

//        // jeżeli plik istnieje
//        if (File.Exists(PathConfigFile)) lines = File.ReadAllLines(PathConfigFile);

//        // jeżeli plik nie istnieje
//        if (File.Exists(PathConfigFile)) lines = File.ReadAllLines(PathConfigFile);
//        else lines = new string[2] { "", "" };

//        // jeżeli plik ma za mało linii
//        if (lines.Length <= lineIndex) Array.Resize(ref lines, lineIndex + 1);

//        // tylko konkretna linijka
//        lines[lineIndex] = newPath;
//        File.WriteAllLines(PathConfigFile, lines);

//        // zapis też we właściwościach
//        if (lineIndex == 0) PathTo_Saved = newPath;
//        if (lineIndex == 1) PathTo_SAVES = newPath;
//    }



//    public static void PathDetails()
//    {
//        if (!string.IsNullOrEmpty(PathTo_Saved) && !string.IsNullOrEmpty(PathTo_SAVES))
//        {
//            Console.WriteLine("\nŚcieżki do folderu Saved z savem gry i do folderu SAVES z backupami:\n");
//        }
//        Console.WriteLine(GetPath(0));
//        Console.WriteLine(GetPath(1));
//        Console.Write("\nZmień ścieżkę: \n[1] Saved \n[2] SAVES \n\nDowolny przycisk by wrócić do menu... \n\nWybierz: ");
//        string choice = Console.ReadLine() ?? "";
//        if (choice == "1")
//        {
//            SetPath(0);
//        }
//        else if (choice == "2")
//        {
//            SetPath(1);
//        }
//        else
//        {
//            Console.Clear();
//            Log("Ustawienia ścieżek bez zmian.\n");
//            End("Kliknij dowolny przycisk...");
//        } 

//    }

//}
