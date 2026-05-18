using UnityEngine;

public static class ItemNameUtility
{
    public static string Normalize(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return name.Replace("(Clone)", "").Trim();
    }
}
