#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

public sealed class GameplayCollisionTuningRulesTests
{
    [Test]
    public void DoorLeafClassificationKeepsFramesAndFinalDoorSeparate()
    {
        GameObject frame = new GameObject("DoorD_V2_Frame");
        GameObject leaf = new GameObject("DoorD_V2_Left");
        GameObject sensor = new GameObject("Sensor_Porta_Dupla");
        GameObject final = new GameObject("PortaDeFuga");

        try
        {
            Assert.AreEqual(GameplayCollisionTuningCategory.DoorFrame, GameplayCollisionTuningRules.Classify(frame));
            Assert.AreEqual(GameplayCollisionTuningCategory.DoorLeaf, GameplayCollisionTuningRules.Classify(leaf));
            Assert.AreEqual(GameplayCollisionTuningCategory.Structural, GameplayCollisionTuningRules.Classify(sensor));
            Assert.AreEqual(GameplayCollisionTuningCategory.FinalDoor, GameplayCollisionTuningRules.Classify(final));
        }
        finally
        {
            Object.DestroyImmediate(frame);
            Object.DestroyImmediate(leaf);
            Object.DestroyImmediate(sensor);
            Object.DestroyImmediate(final);
        }
    }

    [Test]
    public void PushableAndDecorationNamesHaveDifferentCollisionPolicy()
    {
        Assert.IsTrue(GameplayCollisionTuningRules.LooksLikePushable("ChairWhite (28)"));
        Assert.IsTrue(GameplayCollisionTuningRules.LooksLikePushable("Box_V1 (5)"));
        Assert.IsTrue(GameplayCollisionTuningRules.LooksLikePushable("TableSmall (1)"));

        Assert.IsTrue(GameplayCollisionTuningRules.LooksLikeDecorationPassThrough("Mug (1)"));
        Assert.IsTrue(GameplayCollisionTuningRules.LooksLikeDecorationPassThrough("Plate_Broken"));
        Assert.IsTrue(GameplayCollisionTuningRules.LooksLikeDecorationPassThrough("Pillow"));
        Assert.IsTrue(GameplayCollisionTuningRules.LooksLikeDecorationPassThrough("LampWall"));

        Assert.IsFalse(GameplayCollisionTuningRules.LooksLikePushable("Door_Frame"));
        Assert.IsFalse(GameplayCollisionTuningRules.LooksLikeDecorationPassThrough("Bedroom_floor"));
    }

    [Test]
    public void DoorTintClampPreventsBrightWhiteDoorHighlights()
    {
        Color clamped = GameplayCollisionTuningRules.ClampDoorTint(Color.white);
        Assert.LessOrEqual(clamped.r, GameplayCollisionTuningRules.DoorMaxColorComponent + 0.001f);
        Assert.LessOrEqual(clamped.g, GameplayCollisionTuningRules.DoorMaxColorComponent + 0.001f);
        Assert.LessOrEqual(clamped.b, GameplayCollisionTuningRules.DoorMaxColorComponent + 0.001f);
        Assert.AreEqual(1f, clamped.a);
    }
}
#endif
