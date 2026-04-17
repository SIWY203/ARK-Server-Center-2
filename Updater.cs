namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using System.Diagnostics;
using static SteamCmdManager;
using static MessageManager;


public class Updater
{
    private const string AppId = "2430930";
    public static void UpdateServer(ClusterServer server, bool isAuto = false)
    {
        if (SafetyChecker.IsServerRunningOnPort(server.Port))
        {
            Error($"Serwer jest włączony! Anulowano.");
            End(); return;
        }

        if (!File.Exists(SteamCmdExe))
        {
            DownloadAndExtractSteamCmd();
            InitializeSteamCmd(server);
        }

        Console.WriteLine();
        Success($"AKTUALIZACJA: {server.Map} (Port: {server.Port})");

        // "validate" - sprawdza pliki pobiera braki
        // "2430930" - ARK Survival Ascended AppID
        string arguments = $"+force_install_dir \"{server.ServerRootPath}\" +login anonymous +app_update {AppId} validate +quit";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = SteamCmdExe,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        using (Process? process = Process.Start(startInfo))
        {
            process?.WaitForExit();
        }

        Success("Operacja zakończona.");
        Console.WriteLine(
            "[Info] Jeśli to jest pierwsza instalacja, przed uruchomieniem serwera wielu plików\n" +
            "jeszcze nie ma, więc backupy, konfiguracja na plikach itp, mogą jeszcze nie działać.");

        while (Console.KeyAvailable) Console.ReadKey(intercept: true); // czyszczenie bufora
        if (!isAuto) End(); // nie zamrozi UpdateAllServers()
    }


    public static void UpdateAllServers()
    {
        Console.Clear();
        Warn("Czy napewno chcesz kontynuować?\n" +
            "Aktualizacja wszystkich serwerów może potrwać BARDZO długo!\n");
        Console.WriteLine(
            "[T] Tak, kontynuuj\n" +
            "[N] Nie, anuluj\n");
        Console.Write("Wybierz: ");
        string input = Console.ReadLine() ?? "";

        if (input.ToUpper() != "T") { Console.Clear(); Error("Anulowano."); End(); return; }

        Console.Clear();
        List<Cluster> clusters = ClusterManager.Clusters;

        if (SafetyChecker.IsAnyServerRunning())
        {
            Error($"Jeden z serwerów jest włączony! Anulowano.");
            End(); return;
        }

        for (int i = 0; i < clusters.Count; i++)
        {
            Console.WriteLine();
            Success($"AKTUALIZACJA SERWERÓW W KLASTRZE: {clusters[i].Name}");
            for (int j = 0; j < clusters[i].Servers.Count; j++)
            {
                UpdateServer(clusters[i].Servers[j], true);
            }
        }
        while (Console.KeyAvailable) Console.ReadKey(intercept: true); // czyszczenie bufora

        Success(
            "Gotowe!\n" +
            "+-----------------------------------+\n" +
            "  Zaktualizowano wszystkie serwery!  \n" +
            "+-----------------------------------+");
        End();
    }

}
