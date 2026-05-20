using NUnit.Framework;
using UnityEngine;

public sealed class EnemySpawnExclusionZoneTests
{
    private GameObject zoneObject;

    [TearDown]
    public void TearDown()
    {
        if (zoneObject != null)
        {
            Object.DestroyImmediate(zoneObject);
        }
    }

    [Test]
    public void ZoneContainsOwnCenterAndMargin()
    {
        zoneObject = new GameObject("SpawnExclusionZone");
        EnemySpawnExclusionZone zone = zoneObject.AddComponent<EnemySpawnExclusionZone>();
        zone.centro = new Vector3(0f, 1f, 2f);
        zone.tamanho = new Vector3(4f, 2f, 4f);
        zone.margemExtra = 1f;

        Assert.IsTrue(zone.Contains(zoneObject.transform.TransformPoint(zone.centro)));
        Assert.IsTrue(zone.Contains(zoneObject.transform.TransformPoint(new Vector3(2.9f, 1f, 2f))));
    }

    [Test]
    public void ZoneRejectsPointsOutsideExpandedBox()
    {
        zoneObject = new GameObject("SpawnExclusionZone");
        EnemySpawnExclusionZone zone = zoneObject.AddComponent<EnemySpawnExclusionZone>();
        zone.centro = Vector3.zero;
        zone.tamanho = new Vector3(2f, 2f, 2f);
        zone.margemExtra = 0.25f;

        Assert.IsFalse(zone.Contains(new Vector3(2f, 0f, 0f)));
        Assert.IsFalse(zone.Contains(new Vector3(0f, 0f, 2f)));
    }
}
