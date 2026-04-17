namespace ArkServerCenter.GlobalSettings;
using ArkServerCenter.Clusters;
using static MessageManager;

public class GlobalSettingsMenu
{
    public static void ShowMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========= ARK SERVER CENTER =========");
            Console.WriteLine("Globalne ustawienia\n");
            Console.WriteLine("[1] Aktualizacja");
            Console.WriteLine("[2] Adres IP");
            Console.WriteLine("[3] Folder główny");
            Console.WriteLine("[4] Skróty");
            Console.WriteLine("[5] Język (language)");
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
                Updater.UpdateAllServers();
                continue;
            }

            if (input == "2")
            {
                Console.Clear();
                Address.IpAddressMenu();
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

}
