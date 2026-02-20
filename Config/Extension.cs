
namespace Auto_Downloader_GoldKingZ;

public static class Extension
{
    public static string? GetBestMapMatch(this string currentMap, IEnumerable<string> availableKeys)
    {
        if (string.IsNullOrEmpty(currentMap) || availableKeys == null) return null;

        if (availableKeys.Contains(currentMap))return currentMap;

        var prefixMatch = availableKeys
            .Where(key => key.EndsWith("_") && currentMap.StartsWith(key))
            .OrderByDescending(key => key.Length)
            .FirstOrDefault();
            
        return prefixMatch; 
    }
}