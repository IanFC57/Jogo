using System.Text;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyNavigationDiagnostics
{
    private const string GameplayScenePath = "Assets/Scenes/JogoComMenu.unity";

    public static void RepairGameplayNavMesh()
    {
        EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

        int finalZones = EnsureFinalSpawnExclusionZone();
        int removedFinalDoorModifiers = RemoveFinalDoorNavMeshModifiers();
        int doorModifiers = ApplyDoorLeafModifiers();
        int rebuiltSurfaces = RebuildNavMeshSurfaces();
        int relocatedSpawns = RelocateUnreachableSpawnPoints();

        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log($"EnemyNavigationDiagnostics: finalZones={finalZones}, removedFinalDoorModifiers={removedFinalDoorModifiers}, modifiers={doorModifiers}, rebuiltSurfaces={rebuiltSurfaces}, relocatedSpawns={relocatedSpawns}");
        ReportGameplayNavMesh();
    }

    public static void ReportGameplayNavMesh()
    {
        EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

        Transform player = FindPlayer();
        if (player == null)
        {
            Debug.LogError("EnemyNavigationDiagnostics: Player tag nao encontrada.");
            return;
        }

        if (!NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 3f, NavMesh.AllAreas))
        {
            Debug.LogError("EnemyNavigationDiagnostics: Player nao esta proximo da NavMesh.");
            return;
        }

        StringBuilder report = new StringBuilder();
        report.AppendLine("EnemyNavigationDiagnostics");
        report.AppendLine($"Player: {player.position} -> NavMesh {playerHit.position}");

        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (GameObject obj in objects)
        {
            if (!obj.name.StartsWith("PontoSpawn"))
                continue;

            bool sampled = NavMesh.SamplePosition(obj.transform.position, out NavMeshHit spawnHit, 3f, NavMesh.AllAreas);
            NavMeshPathStatus status = NavMeshPathStatus.PathInvalid;
            if (sampled)
            {
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(spawnHit.position, playerHit.position, NavMesh.AllAreas, path);
                status = path.status;
            }

            report.AppendLine($"{obj.name}: scene={obj.transform.position} sampled={sampled} nav={spawnHit.position} path={status}");
        }

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        int triangleCount = triangulation.indices.Length / 3;
        int samples = Mathf.Min(32, triangleCount);
        int step = Mathf.Max(1, triangleCount / Mathf.Max(1, samples));
        int tested = 0;
        int complete = 0;

        for (int triangle = 0; triangle < triangleCount && tested < samples; triangle += step)
        {
            Vector3 center = GetTriangleCenter(triangulation, triangle);
            if (!NavMesh.SamplePosition(center, out NavMeshHit sampleHit, 1.5f, NavMesh.AllAreas))
                continue;

            NavMeshPath path = new NavMeshPath();
            bool ok = NavMesh.CalculatePath(sampleHit.position, playerHit.position, NavMesh.AllAreas, path) &&
                      path.status == NavMeshPathStatus.PathComplete;

            tested++;
            if (ok)
            {
                complete++;
            }
            else
            {
                report.AppendLine($"Unreachable sample: triangle={triangle} pos={sampleHit.position} status={path.status}");
            }
        }

        report.AppendLine($"NavMesh samples complete: {complete}/{tested}");
        Debug.Log(report.ToString());
    }

    private static int ApplyDoorLeafModifiers()
    {
        int changed = 0;
        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (GameObject obj in objects)
        {
            if (!LooksLikeMovingDoorLeaf(obj))
                continue;

            NavMeshModifier modifier = obj.GetComponent<NavMeshModifier>();
            if (modifier == null)
            {
                modifier = obj.AddComponent<NavMeshModifier>();
                changed++;
            }

            if (!modifier.ignoreFromBuild || modifier.applyToChildren)
            {
                modifier.ignoreFromBuild = true;
                modifier.applyToChildren = false;
                EditorUtility.SetDirty(modifier);
                changed++;
            }
        }

        return changed;
    }

    private static int EnsureFinalSpawnExclusionZone()
    {
        PortaDeFuga[] finalDoors = Object.FindObjectsByType<PortaDeFuga>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int changed = 0;

        foreach (PortaDeFuga finalDoor in finalDoors)
        {
            EnemySpawnExclusionZone zone = finalDoor.GetComponent<EnemySpawnExclusionZone>();
            if (zone == null)
            {
                zone = finalDoor.gameObject.AddComponent<EnemySpawnExclusionZone>();
                changed++;
            }

            Vector3 center = new Vector3(0f, 1f, 2.5f);
            Vector3 size = new Vector3(9f, 4f, 10f);
            const float margin = 1.5f;

            if (zone.centro != center || zone.tamanho != size || !Mathf.Approximately(zone.margemExtra, margin))
            {
                zone.centro = center;
                zone.tamanho = size;
                zone.margemExtra = margin;
                EditorUtility.SetDirty(zone);
                changed++;
            }
        }

        return changed;
    }

    private static int RemoveFinalDoorNavMeshModifiers()
    {
        PortaDeFuga[] finalDoors = Object.FindObjectsByType<PortaDeFuga>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int removed = 0;

        foreach (PortaDeFuga finalDoor in finalDoors)
        {
            NavMeshModifier[] modifiers = finalDoor.GetComponents<NavMeshModifier>();
            foreach (NavMeshModifier modifier in modifiers)
            {
                Object.DestroyImmediate(modifier, true);
                removed++;
            }
        }

        return removed;
    }

    private static int RebuildNavMeshSurfaces()
    {
        NavMeshSurface[] surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int rebuilt = 0;

        foreach (NavMeshSurface surface in surfaces)
        {
            if (surface == null || !surface.enabled)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(surface.navMeshData);
            if (string.IsNullOrEmpty(assetPath))
            {
                string sceneDirectory = System.IO.Path.GetDirectoryName(GameplayScenePath);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(GameplayScenePath);
                string targetDirectory = System.IO.Path.Combine(sceneDirectory, sceneName);
                if (!AssetDatabase.IsValidFolder(targetDirectory))
                {
                    AssetDatabase.CreateFolder(sceneDirectory, sceneName);
                }

                assetPath = AssetDatabase.GenerateUniqueAssetPath(
                    System.IO.Path.Combine(targetDirectory, "NavMesh-" + surface.gameObject.name + ".asset"));
            }

            surface.BuildNavMesh();
            NavMeshData data = surface.navMeshData;
            if (data == null)
                continue;

            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(data, assetPath);
            EditorUtility.SetDirty(surface);
            rebuilt++;
        }

        return rebuilt;
    }

    private static int RelocateUnreachableSpawnPoints()
    {
        Transform player = FindPlayer();
        if (player == null ||
            !NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 3f, NavMesh.AllAreas))
        {
            return 0;
        }

        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int changed = 0;

        foreach (GameObject obj in objects)
        {
            if (!obj.name.StartsWith("PontoSpawn"))
                continue;

            if (HasCompletePath(obj.transform.position, playerHit.position, 3f, out Vector3 sampledCurrent) &&
                !IsInsideAnySpawnExclusion(sampledCurrent))
            {
                continue;
            }

            if (TryFindNearestReachableNavMeshPoint(obj.transform.position, playerHit.position, out Vector3 reachable))
            {
                obj.transform.position = reachable + Vector3.up * 0.3f;
                EditorUtility.SetDirty(obj.transform);
                changed++;
            }
        }

        return changed;
    }

    private static bool TryFindNearestReachableNavMeshPoint(Vector3 origin, Vector3 playerNavMeshPosition, out Vector3 reachable)
    {
        reachable = Vector3.zero;
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        int triangleCount = triangulation.indices.Length / 3;
        float bestDistance = float.MaxValue;
        bool found = false;

        for (int triangle = 0; triangle < triangleCount; triangle++)
        {
            Vector3 center = GetTriangleCenter(triangulation, triangle);
            if (!NavMesh.SamplePosition(center, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                continue;

            Vector3 finalPosition = hit.position + Vector3.up * 0.3f;
            if (!HasCompletePath(finalPosition, playerNavMeshPosition, 0.5f, out Vector3 preciseSample))
                continue;

            if (!HasCompletePath(finalPosition, playerNavMeshPosition, 3f, out Vector3 gameplaySample))
                continue;

            if ((preciseSample - gameplaySample).sqrMagnitude > 0.5f * 0.5f)
                continue;

            if (IsInsideAnySpawnExclusion(gameplaySample))
                continue;

            float distance = Vector3.SqrMagnitude(hit.position - origin);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            reachable = hit.position;
            found = true;
        }

        return found;
    }

    private static bool HasCompletePath(Vector3 from, Vector3 to, float sampleRadius, out Vector3 sampledFrom)
    {
        sampledFrom = Vector3.zero;
        if (!NavMesh.SamplePosition(from, out NavMeshHit fromHit, Mathf.Max(0.1f, sampleRadius), NavMesh.AllAreas))
            return false;

        sampledFrom = fromHit.position;
        NavMeshPath path = new NavMeshPath();
        return NavMesh.CalculatePath(fromHit.position, to, NavMesh.AllAreas, path) &&
               path.status == NavMeshPathStatus.PathComplete;
    }

    private static bool IsInsideAnySpawnExclusion(Vector3 position)
    {
        EnemySpawnExclusionZone[] zones = Object.FindObjectsByType<EnemySpawnExclusionZone>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        foreach (EnemySpawnExclusionZone zone in zones)
        {
            if (zone != null && zone.Contains(position))
            {
                return true;
            }
        }

        return false;
    }

    private static bool LooksLikeMovingDoorLeaf(GameObject obj)
    {
        string name = obj.name.ToLowerInvariant();

        if (!(name.Contains("door") || name.Contains("porta")))
            return false;

        if (name.Contains("fuga") || obj.GetComponent<PortaDeFuga>() != null)
            return false;

        if (name.Contains("frame") || name.Contains("moldura") || name.Contains("eixo") || name.Contains("trigger"))
            return false;

        return obj.GetComponent<MeshRenderer>() != null || obj.GetComponent<MeshCollider>() != null;
    }

    private static Transform FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private static Vector3 GetTriangleCenter(NavMeshTriangulation triangulation, int triangle)
    {
        int index = triangle * 3;
        Vector3 a = triangulation.vertices[triangulation.indices[index]];
        Vector3 b = triangulation.vertices[triangulation.indices[index + 1]];
        Vector3 c = triangulation.vertices[triangulation.indices[index + 2]];
        return (a + b + c) / 3f;
    }
}
