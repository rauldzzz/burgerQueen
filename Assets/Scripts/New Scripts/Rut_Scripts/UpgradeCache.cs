using UnityEngine;

public static class UpgradeCache
{
    public static bool unlockCheeseBurger = false;
    public static bool betterPan = false;
    public static bool extraCuttingZone = false;
    public static bool extraServingZone = false;
    public static int pendingSpend = 0;

    public static bool HasPending()
    {
        return unlockCheeseBurger || betterPan || extraCuttingZone || extraServingZone || pendingSpend > 0;
    }

    public static void Clear()
    {
        unlockCheeseBurger = false;
        betterPan = false;
        extraCuttingZone = false;
        extraServingZone = false;
        pendingSpend = 0;
    }

    public static void ApplyTo(GameManager gm)
    {
        if (gm == null) return;

        gm.unlockCheeseBurger = unlockCheeseBurger || gm.unlockCheeseBurger;
        gm.betterPan = betterPan || gm.betterPan;
        gm.extraCuttingZone = extraCuttingZone || gm.extraCuttingZone;
        gm.extraServingZone = extraServingZone || gm.extraServingZone;

        Debug.Log($"UpgradeCache: Applied cached upgrades to GameManager. PendingSpend=${pendingSpend} (not applied to totalMoney here).");
        Clear();
    }
}
