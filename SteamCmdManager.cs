namespace ArkServerCenter;
using ArkServerCenter.Cluster;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using static MessageManager;


public static class SteamCmdManager
{
    public static string SteamCmdDir => Path.Combine(RootPath.Value, "steamcmd");
    public static string SteamCmdExe => Path.Combine(SteamCmdDir, "steamcmd.exe");
    public static string ZipPath => Path.Combine(SteamCmdDir, "steamcmd.zip");


    private const string DownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";


    public static string PrepareAndGetSteamCmdPath()
    {
        if (!File.Exists(SteamCmdExe))
        {
            DownloadAndExtractSteamCmd();
        }
        return SteamCmdExe;
    }



    private static void DownloadAndExtractSteamCmd()
    {
        try
        {
            Warn("Nie znaleziono SteamCMD. Rozpoczynam pobieranie...");
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

            Success("Inicjalizacja SteamCMD...");
            InitializeSteamCmd();

            Success("SteamCMD jest gotowe!");
            Thread.Sleep(900);
        }
        catch (Exception ex)
        {
            Error($"Błąd podczas przygotowywania SteamCMD: {ex.Message}");
            End();
        }
    }


    private static void InitializeSteamCmd() // pierwsze uruchomienie i autoupdate
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = SteamCmdExe,
            Arguments = "+quit",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        using (var process = Process.Start(startInfo))
        {
            process?.WaitForExit();
        }
    }



    public static void UpdateServer(ClusterServer server)
    {
        string steamCmdPath = SteamCmdManager.PrepareAndGetSteamCmdPath();
        if (!File.Exists(steamCmdPath)) return;

        Console.WriteLine();
        Success($"AKTUALIZACJA: {server.Map} (Port: {server.Port})");

        // "validate" - sprawdza pliki pobiera braki, "2430930" - ARK Survival Ascended AppID
        string arguments = $"+force_install_dir \"{server.ServerRootPath}\" +login anonymous +app_update 2430930 validate +quit";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = steamCmdPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        using (Process? process = Process.Start(startInfo))
        {
            process?.WaitForExit();
        }

        Success("Operacja zakończona.");

        // czyszczenie bufora, by nie zatrzymywało instalacji
        while (Console.KeyAvailable) Console.ReadKey(intercept: true);
        End();
    }


}
