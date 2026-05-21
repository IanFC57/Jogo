#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class GameplaySceneCollisionTuningTests
{
    private const string GameplayScenePath = "Assets/Scenes/JogoComMenu.unity";
    private const string AsylumPrefabPath = "Assets/Cenario/Prefabs/Asylum.prefab";
    private const float NearbyFramePadding = 1.6f;
    private const float NearbyFrameMaxDistance = 2.35f;

    [Test]
    public void AsylumPrefabDoorsWithoutOpeningTriggersKeepCollidersAndNavMeshBlocking()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AsylumPrefabPath);
        Assert.NotNull(prefab, "O prefab Asylum precisa existir para validar os colliders do cenario.");

        int blockingDoorLeaves = 0;
        int blockingDoorFrames = 0;
        int doorPartsWithSolidCollider = 0;
        Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject obj = transforms[i].gameObject;
            GameplayCollisionTuningCategory category = GameplayCollisionTuningRules.Classify(obj);
            if (category != GameplayCollisionTuningCategory.DoorLeaf &&
                category != GameplayCollisionTuningCategory.DoorFrame)
            {
                continue;
            }

            if (category == GameplayCollisionTuningCategory.DoorLeaf)
            {
                blockingDoorLeaves++;
            }
            else
            {
                blockingDoorFrames++;
            }

            AssertNoDisabledSolidCollider(obj, $"{obj.name} nao pode ficar sem collider solido se nao tem trigger de abertura.");
            AssertDoesNotIgnoreFromNavMeshBuild(obj);
            if (HasEnabledSolidCollider(obj))
            {
                doorPartsWithSolidCollider++;
            }
        }

        Assert.GreaterOrEqual(blockingDoorLeaves, 120, "Portas sem trigger no prefab devem continuar existindo como barreiras do cenario.");
        Assert.GreaterOrEqual(blockingDoorFrames, 20, "Molduras sem trigger no prefab tambem devem continuar bloqueando a NavMesh.");
        Assert.GreaterOrEqual(doorPartsWithSolidCollider, 40, "A correcao precisa restaurar colliders solidos em uma quantidade relevante de portas/molduras.");
    }

    [Test]
    public void PushableObjectsUseSimplePhysicsInsteadOfStaticMeshColliders()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AsylumPrefabPath);
        Assert.NotNull(prefab);

        int pushables = 0;
        Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject obj = transforms[i].gameObject;
            if (GameplayCollisionTuningRules.Classify(obj) != GameplayCollisionTuningCategory.Pushable)
                continue;

            pushables++;
            Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
            BoxCollider boxCollider = obj.GetComponent<BoxCollider>();

            Assert.NotNull(rigidbody, $"{obj.name} precisa de Rigidbody para ser empurravel.");
            Assert.IsFalse(rigidbody.isKinematic, $"{obj.name} nao pode ficar cinematico.");
            Assert.IsTrue(rigidbody.useGravity, $"{obj.name} precisa usar gravidade.");
            Assert.IsTrue((rigidbody.constraints & RigidbodyConstraints.FreezeRotationX) != 0, $"{obj.name} deve congelar rotacao X.");
            Assert.IsTrue((rigidbody.constraints & RigidbodyConstraints.FreezeRotationZ) != 0, $"{obj.name} deve congelar rotacao Z.");
            Assert.GreaterOrEqual(rigidbody.mass, GameplayCollisionTuningRules.PushableDefaultMass);
            Assert.NotNull(boxCollider, $"{obj.name} precisa trocar o MeshCollider por BoxCollider simples.");
            Assert.IsTrue(boxCollider.enabled, $"{obj.name} precisa manter BoxCollider ativo.");
            Assert.IsFalse(boxCollider.isTrigger, $"{obj.name} precisa de colisao fisica solida.");
            Assert.AreEqual((StaticEditorFlags)0, GameObjectUtility.GetStaticEditorFlags(obj), $"{obj.name} nao pode ficar marcado como estatico.");

            MeshCollider[] meshColliders = obj.GetComponents<MeshCollider>();
            for (int j = 0; j < meshColliders.Length; j++)
            {
                Assert.IsFalse(meshColliders[j].enabled, $"{obj.name} nao pode manter MeshCollider solido junto do Rigidbody.");
            }

            AssertHasIgnoreFromNavMeshBuild(obj);
        }

        Assert.GreaterOrEqual(pushables, 100, "Cadeiras, caixas e bancos precisam virar obstaculos empurraveis em quantidade relevante.");
    }

    [Test]
    public void DoorMaterialsAreMatteDarkAndNonEmissive()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Cenario/Materials" });
        int checkedMaterials = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || !GameplayCollisionTuningRules.LooksLikeDoorMaterialName(material.name))
                continue;

            checkedMaterials++;

            if (material.HasProperty("_Metallic"))
            {
                Assert.AreEqual(0f, material.GetFloat("_Metallic"), 0.001f, $"{material.name} nao deve ser metalica.");
            }

            if (material.HasProperty("_Glossiness"))
            {
                Assert.LessOrEqual(
                    material.GetFloat("_Glossiness"),
                    GameplayCollisionTuningRules.DoorMaxSmoothness + 0.001f,
                    $"{material.name} esta brilhante demais.");
            }

            if (material.HasProperty("_Smoothness"))
            {
                Assert.LessOrEqual(
                    material.GetFloat("_Smoothness"),
                    GameplayCollisionTuningRules.DoorMaxSmoothness + 0.001f,
                    $"{material.name} esta suave/brilhante demais.");
            }

            if (material.HasProperty("_Color"))
            {
                Color color = material.GetColor("_Color");
                float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
                Assert.LessOrEqual(max, GameplayCollisionTuningRules.DoorMaxColorComponent + 0.001f, $"{material.name} esta clara demais.");
            }

            if (material.HasProperty("_EmissionColor"))
            {
                Assert.AreEqual(Color.black, material.GetColor("_EmissionColor"), $"{material.name} nao deve emitir luz.");
            }

            Assert.IsFalse(material.IsKeywordEnabled("_EMISSION"), $"{material.name} nao deve ter emissao ligada.");
        }

        Assert.GreaterOrEqual(checkedMaterials, 20, "Os principais materiais de porta precisam estar cobertos pelo ajuste visual.");
    }

    [Test]
    public void GameplaySceneDoorCollisionPolicyDependsOnOpeningTrigger()
    {
        EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

        HashSet<GameObject> openableDoorParts = CollectOpenableDoorPartsForTest();
        int openableLeaves = 0;
        int openableFrames = 0;
        int blockingDoorParts = 0;
        int blockingDoorPartsWithSolidCollider = 0;
        int tunedRenderers = 0;
        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject obj = objects[i];
            GameplayCollisionTuningCategory category = GameplayCollisionTuningRules.Classify(obj);
            if (category != GameplayCollisionTuningCategory.DoorLeaf &&
                category != GameplayCollisionTuningCategory.DoorFrame)
            {
                continue;
            }

            if (category == GameplayCollisionTuningCategory.DoorLeaf)
            {
                if (openableDoorParts.Contains(obj))
                {
                    openableLeaves++;
                }
            }
            else if (openableDoorParts.Contains(obj))
            {
                openableFrames++;
            }

            if (openableDoorParts.Contains(obj))
            {
                AssertNoEnabledSolidCollider(obj, $"{obj.name} pertence a uma porta abrivel e nao deve bloquear passagem.");
                AssertHasIgnoreFromNavMeshBuild(obj);
            }
            else
            {
                blockingDoorParts++;
                AssertNoDisabledSolidCollider(obj, $"{obj.name} nao tem trigger de abertura e precisa manter collider solido quando existir.");
                AssertDoesNotIgnoreFromNavMeshBuild(obj);
                if (HasEnabledSolidCollider(obj))
                {
                    blockingDoorPartsWithSolidCollider++;
                }
            }

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                tunedRenderers++;
                Assert.AreEqual(ReflectionProbeUsage.Off, renderer.reflectionProbeUsage, $"{obj.name} nao deve brilhar por reflection probe.");
            }
        }

        Assert.GreaterOrEqual(openableLeaves, 4, "Portas com trigger de abertura precisam ficar livres para o jogador e os inimigos passarem.");
        Assert.GreaterOrEqual(openableFrames, 1, "Molduras de portas abríveis podem ser passaveis, mas apenas quando pertencem a esse conjunto.");
        Assert.GreaterOrEqual(blockingDoorParts, 90, "Portas e molduras sem trigger precisam continuar como barreiras.");
        Assert.GreaterOrEqual(blockingDoorPartsWithSolidCollider, 20, "A cena precisa manter colliders solidos em portas que nao abrem.");
        Assert.GreaterOrEqual(tunedRenderers, 6, "Renderers de porta precisam ser validados visualmente.");
    }

    [Test]
    public void DoorOpeningAxesNeverPointToFramesOrSensors()
    {
        EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

        int validatedAxes = 0;
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || !HasEnabledTriggerCollider(behaviour.gameObject))
                continue;

            string typeName = behaviour.GetType().Name;
            if (typeName == "PortaInteligente1")
            {
                AssertValidDoorAxis(GetTransformReference(behaviour, "eixoDaPorta"), $"{behaviour.name}.eixoDaPorta");
                validatedAxes++;
            }
            else if (typeName == "PortaDupla")
            {
                AssertValidDoorAxis(GetTransformReference(behaviour, "eixoEsquerdo"), $"{behaviour.name}.eixoEsquerdo");
                AssertValidDoorAxis(GetTransformReference(behaviour, "eixoDireito"), $"{behaviour.name}.eixoDireito");
                validatedAxes += 2;
            }
            else if (typeName == "PortaInteligente")
            {
                AssertValidDoorAxis(behaviour.transform, $"{behaviour.name}.transform");
                validatedAxes++;
            }
        }

        Assert.GreaterOrEqual(validatedAxes, 2, "A cena precisa validar pelo menos os eixos de portas abríveis principais.");
    }

    [Test]
    public void GameplaySceneFinalDoorKeepsBlockingAndReflectionProbesAreCapped()
    {
        EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

        MonoBehaviour finalDoor = FindBehaviour("PortaDeFuga");
        Assert.NotNull(finalDoor, "A porta final deve continuar existindo como saida trancada.");

        Collider[] colliders = finalDoor.GetComponentsInChildren<Collider>(true);
        int solidColliders = 0;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (GameplayCollisionTuningRules.IsSolidCollider(colliders[i]))
            {
                solidColliders++;
            }
        }

        Assert.Greater(solidColliders, 0, "A porta final trancada deve continuar bloqueando fisicamente.");

        ReflectionProbe[] probes = Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Assert.Greater(probes.Length, 0, "A cena deve manter reflection probes para o ambiente.");
        for (int i = 0; i < probes.Length; i++)
        {
            Assert.LessOrEqual(
                probes[i].intensity,
                GameplayCollisionTuningRules.ReflectionProbeMaxIntensity + 0.001f,
                $"{probes[i].name} esta forte demais e pode iluminar portas ao olhar.");
        }
    }

    private static void AssertNoEnabledSolidCollider(GameObject obj, string message)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Assert.IsFalse(GameplayCollisionTuningRules.IsSolidCollider(colliders[i]), message);
        }
    }

    private static void AssertNoDisabledSolidCollider(GameObject obj, string message)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            Assert.IsFalse(collider != null && !collider.enabled && !collider.isTrigger, message);
        }
    }

    private static void AssertHasIgnoreFromNavMeshBuild(GameObject obj)
    {
        MonoBehaviour[] behaviours = obj.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour.GetType().Name != "NavMeshModifier")
                continue;

            PropertyInfo ignoreProperty = behaviour.GetType().GetProperty("ignoreFromBuild");
            PropertyInfo applyToChildrenProperty = behaviour.GetType().GetProperty("applyToChildren");
            bool ignoreFromBuild = ignoreProperty != null && (bool)ignoreProperty.GetValue(behaviour);
            bool applyToChildren = applyToChildrenProperty != null && (bool)applyToChildrenProperty.GetValue(behaviour);
            Assert.IsTrue(ignoreFromBuild, $"{obj.name} precisa ser ignorado no bake da NavMesh.");
            Assert.IsFalse(applyToChildren, $"{obj.name} deve aplicar o modificador apenas nele.");
            return;
        }

        Assert.Fail($"{obj.name} precisa de NavMeshModifier.");
    }

    private static void AssertDoesNotIgnoreFromNavMeshBuild(GameObject obj)
    {
        MonoBehaviour[] behaviours = obj.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour.GetType().Name != "NavMeshModifier")
                continue;

            PropertyInfo ignoreProperty = behaviour.GetType().GetProperty("ignoreFromBuild");
            bool ignoreFromBuild = ignoreProperty != null && (bool)ignoreProperty.GetValue(behaviour);
            Assert.IsFalse(ignoreFromBuild, $"{obj.name} nao tem trigger de abertura e deve participar do bake da NavMesh.");
        }
    }

    private static bool HasEnabledSolidCollider(GameObject obj)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (GameplayCollisionTuningRules.IsSolidCollider(colliders[i]))
                return true;
        }

        return false;
    }

    private static HashSet<GameObject> CollectOpenableDoorPartsForTest()
    {
        HashSet<GameObject> result = new HashSet<GameObject>();
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || !IsDoorOpeningController(behaviour) || !HasEnabledTriggerCollider(behaviour.gameObject))
                continue;

            Transform[] axes = ResolveDoorAxes(behaviour);
            for (int j = 0; j < axes.Length; j++)
            {
                AddOpenableDoorAxisParts(axes[j], result);
            }
        }

        return result;
    }

    private static bool IsDoorOpeningController(MonoBehaviour behaviour)
    {
        string typeName = behaviour.GetType().Name;
        return typeName == "PortaInteligente1" ||
               typeName == "PortaDupla" ||
               typeName == "PortaInteligente" ||
               typeName == "PortaAutomatica";
    }

    private static Transform[] ResolveDoorAxes(MonoBehaviour behaviour)
    {
        string typeName = behaviour.GetType().Name;
        if (typeName == "PortaInteligente1")
        {
            return new[] { GetTransformReference(behaviour, "eixoDaPorta") };
        }

        if (typeName == "PortaDupla")
        {
            return new[]
            {
                GetTransformReference(behaviour, "eixoEsquerdo"),
                GetTransformReference(behaviour, "eixoDireito")
            };
        }

        return new[] { behaviour.transform };
    }

    private static Transform GetTransformReference(MonoBehaviour behaviour, string propertyName)
    {
        SerializedObject serialized = new SerializedObject(behaviour);
        SerializedProperty property = serialized.FindProperty(propertyName);
        return property != null ? property.objectReferenceValue as Transform : null;
    }

    private static void AddOpenableDoorAxisParts(Transform axis, HashSet<GameObject> result)
    {
        if (!IsValidDoorAxis(axis))
            return;

        Transform[] children = axis.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (GameplayCollisionTuningRules.Classify(children[i].gameObject) == GameplayCollisionTuningCategory.DoorLeaf)
            {
                result.Add(children[i].gameObject);
            }
        }

        Bounds axisBounds = ResolveWorldBounds(axis);
        Bounds expanded = axisBounds;
        expanded.Expand(new Vector3(NearbyFramePadding, NearbyFramePadding, NearbyFramePadding));

        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject obj = objects[i];
            if (GameplayCollisionTuningRules.Classify(obj) != GameplayCollisionTuningCategory.DoorFrame)
                continue;

            Bounds frameBounds = ResolveWorldBounds(obj.transform);
            if (expanded.Intersects(frameBounds) ||
                Vector3.Distance(frameBounds.center, axisBounds.center) <= NearbyFrameMaxDistance)
            {
                result.Add(obj);
            }
        }
    }

    private static void AssertValidDoorAxis(Transform axis, string fieldName)
    {
        Assert.NotNull(axis, $"{fieldName} precisa apontar para a folha/eixo da porta, nao para a moldura.");
        Assert.IsFalse(LooksLikeFrameOrSensor(axis.gameObject), $"{fieldName} esta apontando para moldura/sensor: {axis.name}.");
        Assert.IsTrue(HasDoorLeafGeometry(axis), $"{fieldName} precisa conter geometria de folha de porta.");
    }

    private static bool IsValidDoorAxis(Transform axis)
    {
        return axis != null &&
               !LooksLikeFrameOrSensor(axis.gameObject) &&
               HasDoorLeafGeometry(axis);
    }

    private static bool HasDoorLeafGeometry(Transform root)
    {
        if (root == null)
            return false;

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform child = transforms[i];
            if (child == null || LooksLikeFrameOrSensor(child.gameObject))
                continue;

            GameplayCollisionTuningCategory category = GameplayCollisionTuningRules.Classify(child.gameObject);
            if (category == GameplayCollisionTuningCategory.DoorLeaf && HasRendererOrCollider(child.gameObject))
                return true;

            string name = child.name.ToLowerInvariant();
            if ((name.Contains("porta") || name.Contains("door")) && HasRendererOrCollider(child.gameObject))
                return true;
        }

        return false;
    }

    private static bool HasRendererOrCollider(GameObject obj)
    {
        return obj.GetComponent<Renderer>() != null ||
               obj.GetComponent<Collider>() != null ||
               obj.GetComponentInChildren<Renderer>(true) != null ||
               obj.GetComponentInChildren<Collider>(true) != null;
    }

    private static bool LooksLikeFrameOrSensor(GameObject obj)
    {
        string name = obj != null ? obj.name.ToLowerInvariant() : string.Empty;
        return name.Contains("frame") ||
               name.Contains("moldura") ||
               name.Contains("sensor") ||
               name.Contains("trigger") ||
               name.Contains("fuga");
    }

    private static bool HasEnabledTriggerCollider(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider != null && collider.enabled && collider.isTrigger)
                return true;
        }

        return false;
    }

    private static Bounds ResolveWorldBounds(Transform root)
    {
        bool hasBounds = false;
        Bounds bounds = new Bounds(root.position, Vector3.one * 0.1f);

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || collider.isTrigger)
                continue;

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return bounds;
    }

    private static MonoBehaviour FindBehaviour(string typeName)
    {
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
}
#endif
