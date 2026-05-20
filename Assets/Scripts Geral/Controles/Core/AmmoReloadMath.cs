using UnityEngine;

public static class AmmoReloadMath
{
    public static int GetReloadAmount(int currentAmmo, int ammoCapacity, int reserveAmmo)
    {
        int missingAmmo = Mathf.Max(0, ammoCapacity - currentAmmo);
        int availableReserve = Mathf.Max(0, reserveAmmo);
        return Mathf.Min(missingAmmo, availableReserve);
    }
}
