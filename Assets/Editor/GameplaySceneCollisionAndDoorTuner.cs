using System;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class GameplaySceneCollisionAndDoorTuner
{
    public const string GameplayScenePath = "Assets/Scenes/JogoComMenu.unity";
    public const string AsylumPrefabPath = "Assets/Cenario/Prefabs/Asylum.prefab";
    public const string PushablePhysicsMaterialPath = "Assets/Cenario/Materials/PushableLowFriction.physicMaterial";

    [MenuItem("Tools/Gameplay/Apply Collision And Door Tuning")]
    public static void Apply()
    {
        TuningReport report = new TuningReport();
        PhysicMaterial pushableMaterial = GetOrCreatePushablePhysicsMaterial(report);

        ApplyDoorMaterialSettings(report);
        ApplyAsylumPrefab(pushableMaterial, report);
        ApplyGameplayScene(pushableMaterial, report);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(report.ToString());
    }

    [MenuItem("Tools/Gameplay/Apply Collision Tuning And Rebuild NavMesh")]
    public static void ApplyAndRebuildNavMesh()
    {
        Apply();
        EnemyNavigationDiagnostics.RepairGameplayNavMesh();
    }

    private static void ApplyAsylumPrefab(PhysicMaterial pushableMaterial, TuningReport report)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(AsylumPrefabPath);
        try
        {
            GameObject[] roots = { root };
            RepairDoorAnimationAxes(roots, report);
            DoorTuningContext doorContext = DoorTuningContext.Create(roots);
            ProcessHierarchy(root, pushableMaterial, report, doorContext);
            PrefabUtility.SaveAsPrefabAsset(root, AsylumPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ApplyGameplayScene(PhysicMaterial pushableMaterial, TuningReport report)
    {
        Scene previousActiveScene = SceneManager.GetActiveScene();
        Scene scene = FindLoadedScene(GameplayScenePath);
        bool openedScene = !scene.IsValid();

        if (openedScene)
        {
            scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Additive);
        }

        try
        {
            GameObject[] roots = scene.GetRootGameObjects();
            RepairDoorAnimationAxes(roots, report);
            DoorTuningContext doorContext = DoorTuningContext.Create(roots);
            for (int i = 0; i < roots.Length; i++)
            {
                ProcessHierarchy(roots[i], pushableMaterial, report, doorContext);
            }

            CapReflectionProbes(scene, report);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        finally
        {
            if (openedScene)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            if (previousActiveScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(previousActiveScene);
            }
        }
    }

    internal static HashSet<GameObject> CollectOpenableDoorParts(GameObject[] roots)
    {
        return DoorTuningContext.Create(roots).CopyOpenableDoorParts();
    }

    private static void ProcessHierarchy(
        GameObject root,
        PhysicMaterial pushableMaterial,
        TuningReport report,
        DoorTuningContext doorContext)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject obj = transforms[i].gameObject;
            GameplayCollisionTuningCategory category = GameplayCollisionTuningRules.Classify(obj);

            switch (category)
            {
                case GameplayCollisionTuningCategory.DoorLeaf:
                case GameplayCollisionTuningCategory.DoorFrame:
                    TuneDoorRenderers(obj, report);
                    if (doorContext.IsOpenableDoorPart(obj))
                    {
                        MakeDoorPartPassable(obj, report);
                        EnsureIgnoredFromNavMeshBuild(obj, report);
                        report.OpenableDoorParts++;
                    }
                    else
                    {
                        RestoreDoorPartCollision(obj, report);
                        EnsureIncludedInNavMeshBuild(obj, report);
                        report.BlockingDoorParts++;
                    }
                    break;

                case GameplayCollisionTuningCategory.Pushable:
                    MakePushable(obj, pushableMaterial, report);
                    EnsureIgnoredFromNavMeshBuild(obj, report);
                    break;

                case GameplayCollisionTuningCategory.DecorationPassThrough:
                    DisableSolidColliders(obj, report);
                    EnsureIgnoredFromNavMeshBuild(obj, report);
                    break;
            }
        }
    }

    private static void MakeDoorPartPassable(GameObject obj, TuningReport report)
    {
        DisableSolidColliders(obj, report);
        ClearStaticFlags(obj, report);
    }

    private static void MakePushable(GameObject obj, PhysicMaterial pushableMaterial, TuningReport report)
    {
        ClearStaticFlags(obj, report);

        MeshCollider[] meshColliders = obj.GetComponents<MeshCollider>();
        for (int i = 0; i < meshColliders.Length; i++)
        {
            MeshCollider meshCollider = meshColliders[i];
            if (meshCollider != null && meshCollider.enabled)
            {
                meshCollider.enabled = false;
                EditorUtility.SetDirty(meshCollider);
                report.DisabledSolidColliders++;
            }
        }

        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = obj.AddComponent<BoxCollider>();
            report.AddedBoxColliders++;
        }

        Bounds localBounds = ResolveLocalBounds(obj);
        Vector3 size = localBounds.size;
        size.x = Mathf.Max(0.2f, size.x);
        size.y = Mathf.Max(0.2f, size.y);
        size.z = Mathf.Max(0.2f, size.z);

        if (boxCollider.center != localBounds.center || boxCollider.size != size || boxCollider.isTrigger)
        {
            boxCollider.center = localBounds.center;
            boxCollider.size = size;
            boxCollider.isTrigger = false;
            EditorUtility.SetDirty(boxCollider);
            report.UpdatedBoxColliders++;
        }

        if (boxCollider.sharedMaterial != pushableMaterial)
        {
            boxCollider.sharedMaterial = pushableMaterial;
            EditorUtility.SetDirty(boxCollider);
        }

        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = obj.AddComponent<Rigidbody>();
            report.AddedRigidbodies++;
        }

        float targetMass = GameplayCollisionTuningRules.ShouldUseHeavyPushableMass(obj)
            ? GameplayCollisionTuningRules.PushableHeavyMass
            : GameplayCollisionTuningRules.PushableDefaultMass;

        RigidbodyConstraints targetConstraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        if (!Mathf.Approximately(rigidbody.mass, targetMass) ||
            !Mathf.Approximately(rigidbody.drag, GameplayCollisionTuningRules.PushableDrag) ||
            !Mathf.Approximately(rigidbody.angularDrag, GameplayCollisionTuningRules.PushableAngularDrag) ||
            rigidbody.constraints != targetConstraints ||
            !rigidbody.useGravity ||
            rigidbody.isKinematic)
        {
            rigidbody.mass = targetMass;
            rigidbody.drag = GameplayCollisionTuningRules.PushableDrag;
            rigidbody.angularDrag = GameplayCollisionTuningRules.PushableAngularDrag;
            rigidbody.constraints = targetConstraints;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            EditorUtility.SetDirty(rigidbody);
            report.UpdatedRigidbodies++;
        }
    }

    private static void DisableSolidColliders(GameObject obj, TuningReport report)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (!GameplayCollisionTuningRules.IsSolidCollider(collider))
                continue;

            collider.enabled = false;
            EditorUtility.SetDirty(collider);
            report.DisabledSolidColliders++;
        }
    }

    private static void RestoreDoorPartCollision(GameObject obj, TuningReport report)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || collider.isTrigger || collider.enabled)
                continue;

            collider.enabled = true;
            EditorUtility.SetDirty(collider);
            report.RestoredSolidColliders++;
        }
    }

    private static void TuneDoorRenderers(GameObject obj, TuningReport report)
    {
        Renderer[] renderers = obj.GetComponents<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
                continue;

            if (renderer.reflectionProbeUsage != ReflectionProbeUsage.Off)
            {
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                EditorUtility.SetDirty(renderer);
                report.RenderersWithoutReflectionProbes++;
            }

            if (renderer.lightProbeUsage != LightProbeUsage.BlendProbes)
            {
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                EditorUtility.SetDirty(renderer);
            }
        }
    }

    private static void ApplyDoorMaterialSettings(TuningReport report)
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Cenario/Materials" });
        for (int i = 0; i < materialGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
            string materialName = Path.GetFileNameWithoutExtension(path);
            if (!GameplayCollisionTuningRules.LooksLikeDoorMaterialName(materialName))
                continue;

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                continue;

            bool changed = false;

            if (material.HasProperty("_Metallic") && !Mathf.Approximately(material.GetFloat("_Metallic"), 0f))
            {
                material.SetFloat("_Metallic", 0f);
                changed = true;
            }

            if (material.HasProperty("_Glossiness"))
            {
                float smoothness = material.GetFloat("_Glossiness");
                if (smoothness > GameplayCollisionTuningRules.DoorMaxSmoothness)
                {
                    material.SetFloat("_Glossiness", GameplayCollisionTuningRules.DoorMaxSmoothness);
                    changed = true;
                }
            }

            if (material.HasProperty("_Smoothness"))
            {
                float smoothness = material.GetFloat("_Smoothness");
                if (smoothness > GameplayCollisionTuningRules.DoorMaxSmoothness)
                {
                    material.SetFloat("_Smoothness", GameplayCollisionTuningRules.DoorMaxSmoothness);
                    changed = true;
                }
            }

            if (material.HasProperty("_Color"))
            {
                Color color = material.GetColor("_Color");
                Color clamped = GameplayCollisionTuningRules.ClampDoorTint(color);
                if (color != clamped)
                {
                    material.SetColor("_Color", clamped);
                    changed = true;
                }
            }

            if (material.HasProperty("_EmissionColor") && material.GetColor("_EmissionColor") != Color.black)
            {
                material.SetColor("_EmissionColor", Color.black);
                changed = true;
            }

            if (material.IsKeywordEnabled("_EMISSION"))
            {
                material.DisableKeyword("_EMISSION");
                changed = true;
            }

            if (material.globalIlluminationFlags != MaterialGlobalIlluminationFlags.EmissiveIsBlack)
            {
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(material);
                report.TunedDoorMaterials++;
            }
        }
    }

    private static void CapReflectionProbes(Scene scene, TuningReport report)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            ReflectionProbe[] probes = roots[i].GetComponentsInChildren<ReflectionProbe>(true);
            for (int j = 0; j < probes.Length; j++)
            {
                ReflectionProbe probe = probes[j];
                if (probe == null || probe.intensity <= GameplayCollisionTuningRules.ReflectionProbeMaxIntensity)
                    continue;

                probe.intensity = GameplayCollisionTuningRules.ReflectionProbeMaxIntensity;
                EditorUtility.SetDirty(probe);
                report.CappedReflectionProbes++;
            }
        }
    }

    private static void EnsureIgnoredFromNavMeshBuild(GameObject obj, TuningReport report)
    {
        NavMeshModifier modifier = obj.GetComponent<NavMeshModifier>();
        if (modifier == null)
        {
            modifier = obj.AddComponent<NavMeshModifier>();
            report.AddedNavMeshModifiers++;
        }

        if (!modifier.ignoreFromBuild || modifier.applyToChildren)
        {
            modifier.ignoreFromBuild = true;
            modifier.applyToChildren = false;
            EditorUtility.SetDirty(modifier);
            report.UpdatedNavMeshModifiers++;
        }
    }

    private static void EnsureIncludedInNavMeshBuild(GameObject obj, TuningReport report)
    {
        NavMeshModifier[] modifiers = obj.GetComponents<NavMeshModifier>();
        for (int i = 0; i < modifiers.Length; i++)
        {
            NavMeshModifier modifier = modifiers[i];
            if (modifier == null || !modifier.ignoreFromBuild)
                continue;

            modifier.ignoreFromBuild = false;
            modifier.applyToChildren = false;
            EditorUtility.SetDirty(modifier);
            report.RestoredNavMeshModifiers++;
        }
    }

    private static void RepairDoorAnimationAxes(GameObject[] roots, TuningReport report)
    {
        MonoBehaviour[] behaviours = GetBehavioursInRoots(roots);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
                continue;

            string typeName = behaviour.GetType().Name;
            if (typeName == "PortaInteligente1")
            {
                RepairTransformReference(behaviour, "eixoDaPorta", DoorAxisSide.Single, report);
            }
            else if (typeName == "PortaDupla")
            {
                RepairTransformReference(behaviour, "eixoEsquerdo", DoorAxisSide.Left, report);
                RepairTransformReference(behaviour, "eixoDireito", DoorAxisSide.Right, report);
            }
        }
    }

    private static void RepairTransformReference(
        MonoBehaviour controller,
        string propertyName,
        DoorAxisSide side,
        TuningReport report)
    {
        SerializedObject serialized = new SerializedObject(controller);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
            return;

        Transform current = property.objectReferenceValue as Transform;
        if (IsValidDoorAxis(current))
            return;

        Transform repaired = FindBestDoorAxisCandidate(controller.transform, current, side);
        if (repaired == null || repaired == current)
            return;

        property.objectReferenceValue = repaired;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(controller);
        report.RepairedDoorAxes++;
    }

    private static Transform FindBestDoorAxisCandidate(Transform controller, Transform currentAxis, DoorAxisSide side)
    {
        List<Transform> searchRoots = BuildDoorAxisSearchRoots(controller, currentAxis);
        for (int i = 0; i < searchRoots.Count; i++)
        {
            Transform candidate = FindBestDoorAxisCandidateInRoot(searchRoots[i], controller, currentAxis, side);
            if (candidate != null)
                return candidate;
        }

        return null;
    }

    private static Transform FindBestDoorAxisCandidateInRoot(
        Transform root,
        Transform controller,
        Transform currentAxis,
        DoorAxisSide side)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        Transform best = null;
        int bestScore = int.MinValue;
        Vector3 anchor = currentAxis != null ? currentAxis.position : controller.position;

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || candidate == controller || !IsPotentialDoorAxis(candidate))
                continue;

            int score = ScoreDoorAxisCandidate(candidate, anchor, side);
            if (score <= bestScore)
                continue;

            best = candidate;
            bestScore = score;
        }

        return best;
    }

    private static int ScoreDoorAxisCandidate(Transform candidate, Vector3 anchor, DoorAxisSide side)
    {
        string name = candidate.name.ToLowerInvariant();
        int score = 0;

        if (name.Contains("porta_eixo") || name.Contains("door_axis"))
        {
            score += 220;
        }
        else if (name.Contains("eixo") || name.Contains("axis"))
        {
            score += 140;
        }
        else if (name.Contains("porta") || name.Contains("door"))
        {
            score += 70;
        }

        if (HasDoorLeafGeometry(candidate))
        {
            score += 100;
        }

        if (side == DoorAxisSide.Left && (name.Contains("left") || name.Contains("esq")))
        {
            score += 35;
        }
        else if (side == DoorAxisSide.Right && (name.Contains("right") || name.Contains("dir")))
        {
            score += 35;
        }

        float sqrDistance = (candidate.position - anchor).sqrMagnitude;
        score -= Mathf.RoundToInt(sqrDistance * 6f);
        score -= GetHierarchyDepth(candidate);
        return score;
    }

    private static List<Transform> BuildDoorAxisSearchRoots(Transform controller, Transform currentAxis)
    {
        List<Transform> roots = new List<Transform>(6);
        AddUniqueRoot(roots, currentAxis != null ? currentAxis.parent : null);
        AddUniqueRoot(roots, controller.parent);
        AddUniqueRoot(roots, currentAxis != null && currentAxis.parent != null ? currentAxis.parent.parent : null);
        AddUniqueRoot(roots, controller.parent != null ? controller.parent.parent : null);
        AddUniqueRoot(roots, controller);
        AddUniqueRoot(roots, controller.root);
        return roots;
    }

    private static void AddUniqueRoot(List<Transform> roots, Transform candidate)
    {
        if (candidate == null || roots.Contains(candidate))
            return;

        roots.Add(candidate);
    }

    private static bool IsValidDoorAxis(Transform axis)
    {
        return axis != null &&
               !LooksLikeFrameOrSensor(axis.gameObject) &&
               HasDoorLeafGeometry(axis);
    }

    private static bool IsPotentialDoorAxis(Transform candidate)
    {
        if (candidate == null || LooksLikeFrameOrSensor(candidate.gameObject))
            return false;

        GameplayCollisionTuningCategory category = GameplayCollisionTuningRules.Classify(candidate.gameObject);
        if (category == GameplayCollisionTuningCategory.DoorFrame ||
            category == GameplayCollisionTuningCategory.FinalDoor)
        {
            return false;
        }

        return HasDoorLeafGeometry(candidate);
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
            if (category == GameplayCollisionTuningCategory.DoorLeaf &&
                HasRendererOrCollider(child.gameObject))
            {
                return true;
            }

            string name = child.name.ToLowerInvariant();
            if ((name.Contains("porta") || name.Contains("door")) &&
                HasRendererOrCollider(child.gameObject))
            {
                return true;
            }
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

    private static int GetHierarchyDepth(Transform transform)
    {
        int depth = 0;
        Transform current = transform;
        while (current.parent != null)
        {
            depth++;
            current = current.parent;
        }

        return depth;
    }

    private static Bounds ResolveLocalBounds(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            return meshFilter.sharedMesh.bounds;
        }

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.localBounds;
        }

        return new Bounds(Vector3.up * 0.5f, Vector3.one);
    }

    private static void ClearStaticFlags(GameObject obj, TuningReport report)
    {
        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);
        if (flags == 0)
            return;

        GameObjectUtility.SetStaticEditorFlags(obj, 0);
        EditorUtility.SetDirty(obj);
        report.ClearedStaticFlags++;
    }

    private static PhysicMaterial GetOrCreatePushablePhysicsMaterial(TuningReport report)
    {
        PhysicMaterial material = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(PushablePhysicsMaterialPath);
        if (material == null)
        {
            material = new PhysicMaterial("PushableLowFriction");
            AssetDatabase.CreateAsset(material, PushablePhysicsMaterialPath);
            report.CreatedPhysicsMaterials++;
        }

        if (!Mathf.Approximately(material.dynamicFriction, 0.25f) ||
            !Mathf.Approximately(material.staticFriction, 0.35f) ||
            !Mathf.Approximately(material.bounciness, 0f) ||
            material.frictionCombine != PhysicMaterialCombine.Minimum ||
            material.bounceCombine != PhysicMaterialCombine.Minimum)
        {
            material.dynamicFriction = 0.25f;
            material.staticFriction = 0.35f;
            material.bounciness = 0f;
            material.frictionCombine = PhysicMaterialCombine.Minimum;
            material.bounceCombine = PhysicMaterialCombine.Minimum;
            EditorUtility.SetDirty(material);
        }

        return material;
    }

    private static Scene FindLoadedScene(string path)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == path)
            {
                return scene;
            }
        }

        return default;
    }

    private static MonoBehaviour[] GetBehavioursInRoots(GameObject[] roots)
    {
        List<MonoBehaviour> behaviours = new List<MonoBehaviour>(128);
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] == null)
                continue;

            behaviours.AddRange(roots[i].GetComponentsInChildren<MonoBehaviour>(true));
        }

        return behaviours.ToArray();
    }

    private enum DoorAxisSide
    {
        Single,
        Left,
        Right
    }

    private sealed class DoorTuningContext
    {
        private const float NearbyFramePadding = 1.6f;
        private const float NearbyFrameMaxDistance = 2.35f;

        private readonly HashSet<GameObject> openableDoorParts = new HashSet<GameObject>();
        private readonly GameObject[] roots;

        private DoorTuningContext(GameObject[] roots)
        {
            this.roots = roots;
        }

        public static DoorTuningContext Create(GameObject[] roots)
        {
            DoorTuningContext context = new DoorTuningContext(roots);
            MonoBehaviour[] behaviours = GetBehavioursInRoots(roots);

            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (!IsOpenableDoorController(behaviour))
                    continue;

                context.RegisterDoorController(behaviour);
            }

            return context;
        }

        public bool IsOpenableDoorPart(GameObject obj)
        {
            return obj != null && openableDoorParts.Contains(obj);
        }

        public HashSet<GameObject> CopyOpenableDoorParts()
        {
            return new HashSet<GameObject>(openableDoorParts);
        }

        private void RegisterDoorController(MonoBehaviour controller)
        {
            Transform[] axes = ResolveDoorAxes(controller);
            for (int i = 0; i < axes.Length; i++)
            {
                RegisterDoorAxis(axes[i]);
            }
        }

        private void RegisterDoorAxis(Transform axis)
        {
            if (!IsValidDoorAxis(axis))
                return;

            Bounds axisBounds = ResolveWorldBounds(axis);
            Transform[] transforms = axis.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform child = transforms[i];
                if (child == null)
                    continue;

                GameplayCollisionTuningCategory category = GameplayCollisionTuningRules.Classify(child.gameObject);
                if (category == GameplayCollisionTuningCategory.DoorLeaf)
                {
                    openableDoorParts.Add(child.gameObject);
                }
            }

            RegisterNearbyFrames(axis, axisBounds);
        }

        private void RegisterNearbyFrames(Transform axis, Bounds axisBounds)
        {
            Bounds expanded = axisBounds;
            expanded.Expand(new Vector3(NearbyFramePadding, NearbyFramePadding, NearbyFramePadding));

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                    continue;

                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    Transform candidate = transforms[j];
                    if (candidate == null)
                        continue;

                    if (GameplayCollisionTuningRules.Classify(candidate.gameObject) != GameplayCollisionTuningCategory.DoorFrame)
                        continue;

                    Bounds frameBounds = ResolveWorldBounds(candidate);
                    bool intersectsDoor = expanded.Intersects(frameBounds);
                    bool closeToDoor = Vector3.Distance(frameBounds.center, axisBounds.center) <= NearbyFrameMaxDistance;
                    if (intersectsDoor || closeToDoor)
                    {
                        openableDoorParts.Add(candidate.gameObject);
                    }
                }
            }
        }

        private static bool IsOpenableDoorController(MonoBehaviour behaviour)
        {
            if (behaviour == null)
                return false;

            string typeName = behaviour.GetType().Name;
            if (typeName != "PortaInteligente1" &&
                typeName != "PortaDupla" &&
                typeName != "PortaInteligente" &&
                typeName != "PortaAutomatica")
            {
                return false;
            }

            if (GameplayCollisionTuningRules.Classify(behaviour.gameObject) == GameplayCollisionTuningCategory.FinalDoor)
                return false;

            return HasEnabledTriggerCollider(behaviour.gameObject);
        }

        private static bool HasEnabledTriggerCollider(GameObject obj)
        {
            Collider[] ownColliders = obj.GetComponents<Collider>();
            for (int i = 0; i < ownColliders.Length; i++)
            {
                Collider collider = ownColliders[i];
                if (collider != null && collider.enabled && collider.isTrigger)
                    return true;
            }

            Collider[] childColliders = obj.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < childColliders.Length; i++)
            {
                Collider collider = childColliders[i];
                if (collider != null && collider.enabled && collider.isTrigger)
                    return true;
            }

            return false;
        }

        private static Transform[] ResolveDoorAxes(MonoBehaviour controller)
        {
            string typeName = controller.GetType().Name;
            if (typeName == "PortaInteligente1")
            {
                return new[] { GetTransformReference(controller, "eixoDaPorta") };
            }

            if (typeName == "PortaDupla")
            {
                return new[]
                {
                    GetTransformReference(controller, "eixoEsquerdo"),
                    GetTransformReference(controller, "eixoDireito")
                };
            }

            return new[] { controller.transform };
        }

        private static Transform GetTransformReference(MonoBehaviour behaviour, string propertyName)
        {
            SerializedObject serialized = new SerializedObject(behaviour);
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null ? property.objectReferenceValue as Transform : null;
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
    }

    private sealed class TuningReport
    {
        public int CreatedPhysicsMaterials;
        public int DisabledSolidColliders;
        public int RestoredSolidColliders;
        public int AddedBoxColliders;
        public int UpdatedBoxColliders;
        public int AddedRigidbodies;
        public int UpdatedRigidbodies;
        public int AddedNavMeshModifiers;
        public int UpdatedNavMeshModifiers;
        public int RestoredNavMeshModifiers;
        public int ClearedStaticFlags;
        public int RenderersWithoutReflectionProbes;
        public int TunedDoorMaterials;
        public int CappedReflectionProbes;
        public int RepairedDoorAxes;
        public int OpenableDoorParts;
        public int BlockingDoorParts;

        public override string ToString()
        {
            return "GameplaySceneCollisionAndDoorTuner: " +
                   $"physicsMaterials={CreatedPhysicsMaterials}, " +
                   $"doorParts=openable:{OpenableDoorParts}/blocking:{BlockingDoorParts}, " +
                   $"colliders=disabled:{DisabledSolidColliders}/restored:{RestoredSolidColliders}, " +
                   $"boxColliders={AddedBoxColliders}/{UpdatedBoxColliders}, " +
                   $"rigidbodies={AddedRigidbodies}/{UpdatedRigidbodies}, " +
                   $"navModifiers={AddedNavMeshModifiers}/{UpdatedNavMeshModifiers}/restored:{RestoredNavMeshModifiers}, " +
                   $"doorAxesRepaired={RepairedDoorAxes}, " +
                   $"staticFlagsCleared={ClearedStaticFlags}, " +
                   $"renderersWithoutReflectionProbes={RenderersWithoutReflectionProbes}, " +
                   $"doorMaterials={TunedDoorMaterials}, " +
                   $"reflectionProbesCapped={CappedReflectionProbes}";
        }
    }
}
