#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyNavMeshSceneTests
{
    private const string ScenePath = "Assets/Scenes/JogoComMenu.unity";

    [SetUp]
    public void LoadGameplayScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [Test]
    public void GameplaySceneHasPlayerAndNavMesh()
    {
        Transform player = FindPlayer();
        Assert.NotNull(player, "A cena precisa de um objeto com tag Player para os inimigos perseguirem.");

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Assert.Greater(triangulation.vertices.Length, 0, "A cena precisa de NavMesh assada/carregada.");

        Assert.IsTrue(
            NavMesh.SamplePosition(player.position, out _, 3f, NavMesh.AllAreas),
            "O jogador precisa estar proximo de uma area navegavel para os inimigos calcularem caminho.");
    }

    [Test]
    public void FixedSpawnPointsReachPlayer()
    {
        Transform player = FindPlayer();
        Assert.NotNull(player);
        Assert.IsTrue(NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 3f, NavMesh.AllAreas));
        EnemySpawnExclusionZone[] exclusionZones = Object.FindObjectsByType<EnemySpawnExclusionZone>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int spawnPoints = 0;
        int reachable = 0;
        int outsideFinalZone = 0;

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject obj = allObjects[i];
            if (!obj.name.StartsWith("PontoSpawn"))
                continue;

            spawnPoints++;
            if (!NavMesh.SamplePosition(obj.transform.position, out NavMeshHit spawnHit, 3f, NavMesh.AllAreas))
                continue;

            if (!IsInsideAnyExclusionZone(spawnHit.position, exclusionZones))
            {
                outsideFinalZone++;
            }

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(spawnHit.position, playerHit.position, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                reachable++;
            }
        }

        Assert.Greater(spawnPoints, 0, "A cena deve manter pontos fixos como fallback de spawn.");
        Assert.AreEqual(spawnPoints, reachable, "Todos os pontos fixos de spawn precisam ter caminho completo ate o jogador.");
        Assert.AreEqual(spawnPoints, outsideFinalZone, "Pontos fixos nao podem ficar dentro da sala/area final trancada.");
    }

    [Test]
    public void FinalExitHasSpawnExclusionAndDoesNotIgnoreLockedDoorInBake()
    {
        GameObject finalDoor = FindGameObjectWithComponent("PortaDeFuga");
        Assert.NotNull(finalDoor, "A cena precisa de PortaDeFuga para demarcar a area final.");

        EnemySpawnExclusionZone zone = finalDoor.GetComponent<EnemySpawnExclusionZone>();
        Assert.NotNull(zone, "PortaDeFuga precisa de EnemySpawnExclusionZone para bloquear spawn perto/dentro da sala final.");
        Assert.IsTrue(zone.Contains(finalDoor.transform.position), "A zona deve cobrir a propria porta final.");
        Assert.IsTrue(zone.Contains(finalDoor.transform.TransformPoint(new Vector3(0f, 1f, 2f))), "A zona deve cobrir o lado interno da sala final.");

        MonoBehaviour[] components = finalDoor.GetComponents<MonoBehaviour>();
        for (int i = 0; i < components.Length; i++)
        {
            MonoBehaviour component = components[i];
            if (component == null || component.GetType().Name != "NavMeshModifier")
                continue;

            PropertyInfo ignoreProperty = component.GetType().GetProperty("ignoreFromBuild");
            bool ignoreFromBuild = ignoreProperty != null && (bool)ignoreProperty.GetValue(component);
            Assert.IsFalse(ignoreFromBuild, "A porta final trancada deve continuar bloqueando o bake da NavMesh.");
        }
    }

    [Test]
    public void SpawnerProducesReachableCandidatesOutsideFinalZone()
    {
        MonoBehaviour spawner = FindBehaviour("GeradorDeInimigos");
        Assert.NotNull(spawner, "A cena precisa de GeradorDeInimigos.");

        MethodInfo method = spawner.GetType().GetMethod("TryGetValidatedSpawnPose", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method, "GeradorDeInimigos precisa expor TryGetValidatedSpawnPose para validar spawn em testes.");

        Transform player = FindPlayer();
        Assert.NotNull(player);
        Assert.IsTrue(NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 3f, NavMesh.AllAreas));
        Camera playerCamera = FindPlayerCamera(player);
        Assert.NotNull(playerCamera, "A cena precisa de Camera no jogador para bloquear spawns no campo de visao.");

        bool requireOutOfView = GetBoolField(spawner, "exigirSpawnForaDoCampoDeVisao", true);
        float minimumDistance = EnemySpawnRules.SanitizeMinimumSpawnDistance(GetFloatField(spawner, "distanciaMinimaDoJogador", 14f));
        float viewMargin = EnemySpawnRules.SanitizeViewSafetyMargin(GetFloatField(spawner, "margemCampoVisaoSpawn", 0.12f));
        float enemyVisualHeight = Mathf.Max(0.1f, GetFloatField(spawner, "alturaVisualInimigoParaSpawn", 2.2f));

        Assert.IsTrue(requireOutOfView, "O spawner da cena deve exigir spawn fora do campo de visao do jogador.");
        Assert.GreaterOrEqual(minimumDistance, 14f, "O spawn precisa ficar um pouco mais longe do jogador.");

        EnemySpawnExclusionZone[] exclusionZones = Object.FindObjectsByType<EnemySpawnExclusionZone>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        Assert.Greater(exclusionZones.Length, 0, "A cena precisa de pelo menos uma zona de exclusao de spawn.");

        Random.InitState(24052026);
        for (int i = 0; i < 32; i++)
        {
            object[] args = { Vector3.zero, Quaternion.identity };
            bool success = (bool)method.Invoke(spawner, args);
            Assert.IsTrue(success, "O spawner deve conseguir gerar candidatos validos repetidamente.");

            Vector3 position = (Vector3)args[0];
            Assert.IsFalse(IsInsideAnyExclusionZone(position, exclusionZones), "Spawn validado nao pode cair na area final.");
            Assert.GreaterOrEqual(
                Vector3.Distance(position, playerHit.position),
                minimumDistance - 0.05f,
                "Spawn validado deve respeitar a distancia minima maior do jogador.");
            Assert.IsFalse(
                IsInsideCameraView(playerCamera, position, viewMargin, enemyVisualHeight),
                "Spawn validado nao pode aparecer dentro do campo de visao do jogador, nem perto das bordas da tela.");

            NavMeshPath path = new NavMeshPath();
            bool complete = NavMesh.CalculatePath(position, playerHit.position, NavMesh.AllAreas, path) &&
                            path.status == NavMeshPathStatus.PathComplete;
            Assert.IsTrue(complete, "Todo spawn validado precisa ter caminho completo ate o jogador.");
        }
    }

    [Test]
    public void ReachableAllowedNavMeshHasEnoughCandidateCoverage()
    {
        Transform player = FindPlayer();
        Assert.NotNull(player);
        Assert.IsTrue(NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 3f, NavMesh.AllAreas));
        Camera playerCamera = FindPlayerCamera(player);
        Assert.NotNull(playerCamera);
        float minimumDistance = EnemySpawnRules.DefaultMinimumSpawnDistanceFromPlayer;
        float viewMargin = EnemySpawnRules.DefaultViewSafetyViewportMargin;
        float enemyVisualHeight = 2.2f;
        EnemySpawnExclusionZone[] exclusionZones = Object.FindObjectsByType<EnemySpawnExclusionZone>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Assert.Greater(triangulation.indices.Length, 0);

        int triangleCount = triangulation.indices.Length / 3;
        int samples = Mathf.Min(64, triangleCount);
        int step = Mathf.Max(1, triangleCount / samples);
        int tested = 0;
        int reachableAndAllowed = 0;

        for (int triangle = 0; triangle < triangleCount && tested < samples; triangle += step)
        {
            Vector3 center = GetTriangleCenter(triangulation, triangle);
            if (!NavMesh.SamplePosition(center, out NavMeshHit spawnHit, 1.5f, NavMesh.AllAreas))
                continue;

            NavMeshPath path = new NavMeshPath();
            bool complete = NavMesh.CalculatePath(spawnHit.position, playerHit.position, NavMesh.AllAreas, path) &&
                            path.status == NavMeshPathStatus.PathComplete;

            bool farEnough = Vector3.Distance(spawnHit.position, playerHit.position) >= minimumDistance;
            bool outsideView = !IsInsideCameraView(playerCamera, spawnHit.position, viewMargin, enemyVisualHeight);
            if (complete && farEnough && outsideView && !IsInsideAnyExclusionZone(spawnHit.position, exclusionZones))
            {
                reachableAndAllowed++;
            }

            tested++;
        }

        Assert.GreaterOrEqual(tested, Mathf.Min(32, samples), "Poucas amostras de NavMesh foram validadas.");
        Assert.GreaterOrEqual(reachableAndAllowed, 8, "A NavMesh precisa oferecer area permitida suficiente para spawn variado fora da visao.");
    }

    private static Transform FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private static MonoBehaviour FindBehaviour(string typeName)
    {
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour != null && behaviour.GetType().Name == typeName)
            {
                return behaviour;
            }
        }

        return null;
    }

    private static Camera FindPlayerCamera(Transform player)
    {
        if (player != null)
        {
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                return playerCamera;
            }
        }

        return Camera.main;
    }

    private static float GetFloatField(MonoBehaviour behaviour, string fieldName, float fallback)
    {
        FieldInfo field = behaviour.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field != null ? (float)field.GetValue(behaviour) : fallback;
    }

    private static bool GetBoolField(MonoBehaviour behaviour, string fieldName, bool fallback)
    {
        FieldInfo field = behaviour.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field != null ? (bool)field.GetValue(behaviour) : fallback;
    }

    private static GameObject FindGameObjectWithComponent(string typeName)
    {
        MonoBehaviour behaviour = FindBehaviour(typeName);
        return behaviour != null ? behaviour.gameObject : GameObject.Find(typeName);
    }

    private static bool IsInsideAnyExclusionZone(Vector3 position, EnemySpawnExclusionZone[] zones)
    {
        for (int i = 0; i < zones.Length; i++)
        {
            EnemySpawnExclusionZone zone = zones[i];
            if (zone != null && zone.Contains(position))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInsideCameraView(Camera camera, Vector3 position, float margin, float height)
    {
        Vector3 basePosition = position + Vector3.up * 0.25f;
        Vector3 centerPosition = position + Vector3.up * (height * 0.5f);
        Vector3 topPosition = position + Vector3.up * height;

        return EnemySpawnRules.IsViewportPointInsidePlayerView(camera.WorldToViewportPoint(basePosition), margin) ||
               EnemySpawnRules.IsViewportPointInsidePlayerView(camera.WorldToViewportPoint(centerPosition), margin) ||
               EnemySpawnRules.IsViewportPointInsidePlayerView(camera.WorldToViewportPoint(topPosition), margin);
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
#endif
