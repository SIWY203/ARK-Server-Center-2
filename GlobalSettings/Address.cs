namespace ArkServerCenter.GlobalSettings;

using ArkServerCenter.Clusters;
using System.Net;
using static MessageManager;

public class Address
{
	public static string IpAddress { get; set; } = "127.0.0.1";
	public static string ConfigPath => "server_ip.txt";

    public static void IpAddressMenu()
	{
        Console.WriteLine($"Aktualne IP: {IpAddress}");
        Console.WriteLine("[1] Zmień Adres IP");
        Console.WriteLine("[Q] Wróć");
		Console.Write("Wybierz: ");
        string input = (Console.ReadLine()?.Trim() ?? "").ToUpper();
        if (input == "1") ChangeIpAddress();
    }

	public static void ChangeIpAddress()
	{
		if (SafetyChecker.IsAnyServerRunning())
		{
			Error("Jeden z serwerów jest włączony! Anulowano.");
			End(); return;
		}

        Console.Clear();
        Console.WriteLine($"Aktualne IP: {IpAddress}");
        Console.WriteLine($"Ustaw nowy adres, dla serwerów ARK\n");
        Console.Write("Podaj: ");
        string input = (Console.ReadLine()?.Trim() ?? "").ToUpper();
            
		if (string.IsNullOrWhiteSpace(input))
		{
			Console.Clear();
            Error("Nie podano żadnej wartości!");
			End(); return;
		}

		if (!IPAddress.IsValid(input))
		{
            Console.Clear();
            Error("Nieprawidłowy adres IP!");
			End(); return;
		}

		IpAddress = input;
		SaveIPAddress(IpAddress);
	}

    public static void LoadIPAddress()
    {
        if (!File.Exists(ConfigPath)) File.WriteAllText(ConfigPath, "127.0.0.1");

        string ip = File.ReadAllText(ConfigPath).Trim();
        IpAddress = string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
    }

    public static void SaveIPAddress(string ip)
	{
		if (!IPAddress.IsValid(ip))
		{
			Console.Clear();
			Error("To nie jest poprawny adres IP!");
			End(); return;
		}
		else
		{
			File.WriteAllText(ConfigPath, ip);
            Console.Clear();
            Success($"Zapisano nowe IP: {ip}");
			End(); return;
        }
    }



}
