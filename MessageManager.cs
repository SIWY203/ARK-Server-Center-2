namespace ArkServerCenter;


public static class MessageManager
{
    public static void PrintLine(string message = "")
    {
        Console.WriteLine(message);
    }

    public static void Print(string message = "")
    {
        Console.Write(message);
    }

    public static void Log(string message = "Log.")
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("[LOG]: ");
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void Success(string message = "Success!")
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[OK]: ");
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void Warn(string message = "Warning!")
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[WARN]: ");
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void Error(string message = "Error!")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[ERROR]: ");
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void Info(string message = "Information.")
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[INFO]: ");
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void ArkSC(string message = "Ark Server Center.")
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Ark");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("SC");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("]: ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void End(string msg = "\nKliknij dowolny przycisk... ")
    {
        Console.Write(msg);
        Console.ReadLine();
        Console.Clear();
    }
}




// Delegat - domyślnie ustawiony na konsolę
// Przygotowanie na okienkowe UI w przyszłości
/*
    
namespace ArkServerCenter;

public static class MessageManager
{
    public static Action<string, string, ConsoleColor, bool> DisplayHandler = (prefix, msg, color, newLine) =>
    {
        Console.ForegroundColor = color;
        if (newLine) Console.WriteLine($"{prefix}{msg}");
        else Console.Write($"{prefix}{msg}");
        Console.ForegroundColor = ConsoleColor.Gray;
    };


    public static void PrintLine(string message = "", ConsoleColor color = ConsoleColor.Gray)
    {
        DisplayHandler?.Invoke("", message, ConsoleColor.Gray, true);
    }

    public static void Print(string message = "", ConsoleColor color = ConsoleColor.Gray)
    {
        DisplayHandler?.Invoke("", message, ConsoleColor.Gray, false);
    }

    public static void Log(string message = "Log.")
    {
        DisplayHandler?.Invoke("[LOG]: ", message, ConsoleColor.White, true);
    }

    public static void Success(string message = "Success!")
    {
        DisplayHandler?.Invoke("[OK]: ", message, ConsoleColor.Green, true);
    }

    public static void Warn(string message = "Warning!")
    {
        DisplayHandler?.Invoke("[WARN]: ", message, ConsoleColor.Yellow, true);
    }

    public static void Error(string message = "Error!")
    {
        DisplayHandler?.Invoke("[ERROR]: ", message, ConsoleColor.Red, true);
    }

    public static void Info(string message = "Information.")
    {
        DisplayHandler?.Invoke("[INFO]: ", message, ConsoleColor.Cyan, true);
    }

    public static void ArkSC(string message = "Ark Server Center.")
    {
        Print("[", ConsoleColor.White);
        Print("Ark", ConsoleColor.DarkCyan);
        Print("SC", ConsoleColor.Cyan);
        Print("]: ", ConsoleColor.White);
        PrintLine(message, ConsoleColor.Blue);
    }

    public static void End(string msg = "\nKliknij dowolny przycisk... ")
    {
        Print(msg);
        Console.ReadLine();
        Console.Clear();
    }

}

*/

