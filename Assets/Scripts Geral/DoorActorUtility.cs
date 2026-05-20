using UnityEngine;

public static class DoorActorUtility
{
    public static bool IsDoorActor(Collider other)
    {
        if (other == null)
            return false;

        bool isPlayer = other.CompareTag("Player");
        bool isEnemy = IsEnemyActor(other);
        return DoorAccessRules.IsAuthorizedDoorActor(isPlayer, isEnemy);
    }

    public static bool IsEnemyActor(Collider other)
    {
        if (other == null)
            return false;

        return other.GetComponentInParent<EnemyHealth>() != null ||
               other.GetComponentInParent<MonsterFollow>() != null;
    }

    public static Vector3 GetActorPosition(Collider other)
    {
        return other != null ? other.transform.position : Vector3.zero;
    }
}
