public static class DoorAccessRules
{
    public static bool IsAuthorizedDoorActor(bool isPlayer, bool isEnemy)
    {
        return isPlayer || isEnemy;
    }

    public static bool ShouldClose(int occupantCount, float openHoldUntil, float now)
    {
        return occupantCount <= 0 && now >= openHoldUntil;
    }
}
