using UnityEngine;

public sealed class EnemySpawnExclusionZone : MonoBehaviour
{
    public Vector3 centro = new Vector3(0f, 1f, 2.5f);
    public Vector3 tamanho = new Vector3(8f, 4f, 9f);
    public float margemExtra = 1.5f;

    public bool Contains(Vector3 worldPosition)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition) - centro;
        Vector3 halfSize = new Vector3(
            Mathf.Max(0f, tamanho.x) * 0.5f + Mathf.Max(0f, margemExtra),
            Mathf.Max(0f, tamanho.y) * 0.5f + Mathf.Max(0f, margemExtra),
            Mathf.Max(0f, tamanho.z) * 0.5f + Mathf.Max(0f, margemExtra));

        return Mathf.Abs(local.x) <= halfSize.x &&
               Mathf.Abs(local.y) <= halfSize.y &&
               Mathf.Abs(local.z) <= halfSize.z;
    }
}
