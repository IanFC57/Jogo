using System;
using UnityEngine;

public enum GameplayCollisionTuningCategory
{
    Structural,
    DoorFrame,
    DoorLeaf,
    FinalDoor,
    Pushable,
    DecorationPassThrough
}

public static class GameplayCollisionTuningRules
{
    public const float DoorMaxSmoothness = 0.22f;
    public const float DoorMaxColorComponent = 0.72f;
    public const float ReflectionProbeMaxIntensity = 0.8f;
    public const float PushableDefaultMass = 18f;
    public const float PushableHeavyMass = 45f;
    public const float PushableDrag = 3.5f;
    public const float PushableAngularDrag = 8f;

    public static GameplayCollisionTuningCategory Classify(GameObject obj)
    {
        if (obj == null)
            return GameplayCollisionTuningCategory.Structural;

        string name = Normalize(obj.name);

        if (IsFinalDoor(obj, name))
            return GameplayCollisionTuningCategory.FinalDoor;

        if (LooksLikeDoorFrame(name))
            return GameplayCollisionTuningCategory.DoorFrame;

        if (LooksLikeDoorLeaf(name))
            return GameplayCollisionTuningCategory.DoorLeaf;

        if (LooksLikePushable(name))
            return GameplayCollisionTuningCategory.Pushable;

        if (LooksLikeDecorationPassThrough(name))
            return GameplayCollisionTuningCategory.DecorationPassThrough;

        return GameplayCollisionTuningCategory.Structural;
    }

    public static bool LooksLikeDoorMaterialName(string materialName)
    {
        string name = Normalize(materialName);
        return name.Contains("door") ||
               name.Contains("porta") ||
               name.Contains("maindoor") ||
               name.Contains("toiletdoor");
    }

    public static bool ShouldUseHeavyPushableMass(GameObject obj)
    {
        if (obj == null)
            return false;

        string name = Normalize(obj.name);
        return name.Contains("bench") ||
               name.Contains("wheel") ||
               name.Contains("table") ||
               name.Contains("morgue");
    }

    public static Color ClampDoorTint(Color color)
    {
        float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        if (max <= DoorMaxColorComponent || max <= 0f)
            return color;

        float scale = DoorMaxColorComponent / max;
        return new Color(color.r * scale, color.g * scale, color.b * scale, color.a);
    }

    public static bool IsSolidCollider(Collider collider)
    {
        return collider != null && collider.enabled && !collider.isTrigger;
    }

    public static bool LooksLikeDoorLeaf(string normalizedName)
    {
        string name = Normalize(normalizedName);
        if (!(name.Contains("door") || name.Contains("porta")))
            return false;

        return !name.Contains("frame") &&
               !name.Contains("moldura") &&
               !name.Contains("sensor") &&
               !name.Contains("trigger") &&
               !name.Contains("eixo") &&
               !name.Contains("fuga");
    }

    public static bool LooksLikeDoorFrame(string normalizedName)
    {
        string name = Normalize(normalizedName);
        return (name.Contains("door") || name.Contains("porta")) &&
               (name.Contains("frame") || name.Contains("moldura"));
    }

    public static bool LooksLikePushable(string normalizedName)
    {
        string name = Normalize(normalizedName);
        return name.Contains("chair") ||
               name.Contains("bench") ||
               name.Contains("box_v") ||
               name.Contains("wheelchair") ||
               name.Contains("morguewheelbed") ||
               name.Contains("tablesmall");
    }

    public static bool LooksLikeDecorationPassThrough(string normalizedName)
    {
        string name = Normalize(normalizedName);
        return name.Contains("mug") ||
               name.Contains("plate") ||
               name.Contains("pillow") ||
               name.Contains("paper") ||
               name.Contains("poster") ||
               name.Contains("map") ||
               name.Contains("lamp") ||
               name.Contains("singlelight") ||
               name.Contains("neon") ||
               name.Contains("suspension") ||
               name.Contains("heater") ||
               name.Contains("pipe") ||
               name.Contains("cupboard_door") ||
               name.Contains("case_door") ||
               name.Contains("mirror") ||
               name.Contains("medrackdoor") ||
               name.Contains("morguebox_door") ||
               name.Contains("fridge_door");
    }

    private static bool IsFinalDoor(GameObject obj, string normalizedName)
    {
        if (normalizedName.Contains("fuga"))
            return true;

        Component[] components = obj.GetComponentsInParent<Component>(true);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component != null && component.GetType().Name == "PortaDeFuga")
                return true;
        }

        components = obj.GetComponentsInChildren<Component>(true);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component != null && component.GetType().Name == "PortaDeFuga")
                return true;
        }

        return false;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.ToLowerInvariant();
    }
}
