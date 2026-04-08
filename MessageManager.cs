namespace ArkServerCenter;

public static class MessageManager
{
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

    public static void End(string msg = "\nKliknij dowolny przycisk... ")
    {
        Console.Write(msg);
        Console.ReadLine();
        Console.Clear();
    }

}
