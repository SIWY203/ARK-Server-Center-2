namespace ArkServerCenter.GlobalSettings;
using System.Net;
using static MessageManager;

public class Address
{
	public static string IpAddress { get; set; } = "127.0.0.1";

    public static void IpAddressMenu()
	{
        Console.WriteLine($"Aktualne IP: {IpAddress}");
        Console.WriteLine("[1] Zmień Adres IP");
        Console.WriteLine("[Q] Wróć");
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

        Console.WriteLine("========== Zmiana adresu IP ==========\n");
        Console.WriteLine($"Aktualne IP: {IpAddress}");
        Console.WriteLine($"Ustaw nowy adres, dla serwerów ARK\n");
        Console.Write("Podaj: ");
        string input = (Console.ReadLine()?.Trim() ?? "").ToUpper();
            
		if (string.IsNullOrWhiteSpace(input))
		{
            Error("Nie podano żadnej wartości!");
			End(); return;
		}

		if (!IPAddress.IsValid(input))
		{
			Error("Nieprawidłowy adres IP!");
			End(); return;
		}

		IpAddress = input;
		Success($"Ustawiono nowe IP: {input}");
		End(); return;
	}
}
