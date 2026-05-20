using NUnit.Framework;

public sealed class DoorAccessRulesTests
{
    [Test]
    public void PlayerAndEnemyCanOpenDoor()
    {
        Assert.IsTrue(DoorAccessRules.IsAuthorizedDoorActor(isPlayer: true, isEnemy: false));
        Assert.IsTrue(DoorAccessRules.IsAuthorizedDoorActor(isPlayer: false, isEnemy: true));
    }

    [Test]
    public void RandomColliderCannotOpenDoor()
    {
        Assert.IsFalse(DoorAccessRules.IsAuthorizedDoorActor(isPlayer: false, isEnemy: false));
    }

    [Test]
    public void DoorStaysOpenWhileActorOccupiesTrigger()
    {
        Assert.IsFalse(DoorAccessRules.ShouldClose(occupantCount: 1, openHoldUntil: 0f, now: 100f));
    }

    [Test]
    public void DoorClosesOnlyAfterHoldTime()
    {
        Assert.IsFalse(DoorAccessRules.ShouldClose(occupantCount: 0, openHoldUntil: 10f, now: 9.9f));
        Assert.IsTrue(DoorAccessRules.ShouldClose(occupantCount: 0, openHoldUntil: 10f, now: 10f));
    }
}
