public static class WeaponAmmoFlow
{
    public static bool CanConsumeRound(int currentAmmo, bool infiniteAmmo)
    {
        return infiniteAmmo || currentAmmo > 0;
    }

    public static int ConsumeRound(int currentAmmo, bool infiniteAmmo)
    {
        if (infiniteAmmo)
            return currentAmmo;

        return currentAmmo > 0 ? currentAmmo - 1 : 0;
    }
}
