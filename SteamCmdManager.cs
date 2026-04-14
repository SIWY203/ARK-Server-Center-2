namespace ArkServerCenter;
using ArkServerCenter.Cluster;
using System.Diagnostics;
using System.IO.Compression;
using static MessageManager;


public static class SteamCmdManager
{
    public static string SteamCmdDir => Path.Combine(RootPath.Value, "steamcmd");
    public static string SteamCmdExe => Path.Combine(SteamCmdDir, "steamcmd.exe");
    public static string ZipPath => Path.Combine(SteamCmdDir, "steamcmd.zip");


    private const string DownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";


    private static void DownloadAndExtractSteamCmd()
    {
        try
        {
            Log("Nie znaleziono SteamCMD. Rozpoczynam pobieranie...");
            if (!Directory.Exists(SteamCmdDir)) Directory.CreateDirectory(SteamCmdDir);

            using (var client = new HttpClient())
            {
                // Pobieranie synchroniczne
                var response = client.GetByteArrayAsync(DownloadUrl).GetAwaiter().GetResult();
                File.WriteAllBytes(ZipPath, response);
            }

            Success("Pobrano SteamCMD. Wypakowywanie...");
            ZipFile.ExtractToDirectory(ZipPath, SteamCmdDir, overwriteFiles: true);
            File.Delete(ZipPath);
        }
        catch (Exception ex)
        {
            Error($"Błąd podczas przygotowywania SteamCMD: {ex.Message}");
            End();
        }
    }

    private static void InitializeSteamCmd(ClusterServer server)
    {
        Log("Inicjalizacja SteamCMD...");
        ProcessStartInfo initInfo = new ProcessStartInfo
        {
            FileName = SteamCmdExe,
            Arguments = $"+force_install_dir \"{server.ServerRootPath}\" +login anonymous +app_update 2430930 validate +quit",
            UseShellExecute = false,
            CreateNoWindow = false
        };
        using (Process? process = Process.Start(initInfo))
        {
            process?.WaitForExit();
        }
        Success("SteamCMD jest gotowy!");
    }




    public static void UpdateServer(ClusterServer server)
    {
        if (!File.Exists(SteamCmdExe))
        {
            DownloadAndExtractSteamCmd();
            InitializeSteamCmd(server);
        }

        Console.WriteLine();
        Success($"AKTUALIZACJA: {server.Map} (Port: {server.Port})");

        // "validate" - sprawdza pliki pobiera braki
        // "2430930" - ARK Survival Ascended AppID
        string arguments = $"+force_install_dir \"{server.ServerRootPath}\" +login anonymous +app_update 2430930 validate +quit";

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

        // czyszczenie bufora, by nie zatrzymywało instalacji
        while (Console.KeyAvailable) Console.ReadKey(intercept: true);
        End();
    }


}
