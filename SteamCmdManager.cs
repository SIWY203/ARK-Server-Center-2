namespace ArkServerCenter;
using ArkServerCenter.Clusters;
using ArkServerCenter.GlobalSettings;
using System.Diagnostics;
using System.IO.Compression;
using static MessageManager;


public static class SteamCmdManager
{
    public static string SteamCmdDir => Path.Combine(RootPath.Value, "steamcmd");
    public static string SteamCmdExe => Path.Combine(SteamCmdDir, "steamcmd.exe");
    public static string ZipPath => Path.Combine(SteamCmdDir, "steamcmd.zip");


    private const string DownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";


    public static void DownloadAndExtractSteamCmd()
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

    public static void InitializeSteamCmd(ClusterServer server)
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


}
