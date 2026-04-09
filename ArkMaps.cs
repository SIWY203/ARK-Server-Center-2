namespace ArkServerCenter;

public static class ArkMaps
{
    public enum Map
    {
        TheIsland_WP,
        TheCenter_WP,
        ScorchedEarth_WP,
        Ragnarok_WP,
        Aberration_WP,
        Extinction_WP,
        Valguero_WP,
        // Genesis1
        // Genesis2
        // CrystalIsles
        // LostIsland
        // Fjordur
        // Aquatica
        Astraeos_WP,
        LostColony_WP,
        BobsMissions_WP, // mod id: 1005639
    }


    public static void ShowMapMenu()
    {
        Console.WriteLine("===== Lista dostępnych map =====\n");
        string[] maps = Enum.GetNames(typeof(Map));

        for (int i = 0; i < maps.Length; i++)
        {
            Console.WriteLine($"[{i + 1}] {maps[i]}");
        }
    }


    public static int GetMapCount()
    {
        return Enum.GetNames(typeof(Map)).Length;
    }
}

