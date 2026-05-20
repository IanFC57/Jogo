public static class WeaponRemoteFirePolicy
{
    public static bool ShouldReleaseSemiAutoGate(bool isSemiAutomatic)
    {
        return isSemiAutomatic;
    }
}
