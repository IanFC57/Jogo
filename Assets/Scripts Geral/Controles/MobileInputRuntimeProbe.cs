using UnityEngine;
using System.Collections;

public sealed class MobileInputRuntimeProbe : MonoBehaviour
{
    private bool wasTouching;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!Debug.isDebugBuild || !Application.isMobilePlatform)
            return;

        if (FindFirstObjectByType<MobileInputRuntimeProbe>(FindObjectsInactive.Include) != null)
            return;

        GameObject probe = new GameObject(nameof(MobileInputRuntimeProbe));
        DontDestroyOnLoad(probe);
        probe.AddComponent<MobileInputRuntimeProbe>();
    }

    private void LateUpdate()
    {
        bool touching = Input.touchCount > 0;

        if (!wasTouching && touching)
        {
            LogSnapshot("BEGIN");
        }
        else if (wasTouching && !touching)
        {
            LogSnapshot("END");
            StartCoroutine(LogPostTouchSnapshot());
        }

        wasTouching = touching;
    }

    private IEnumerator LogPostTouchSnapshot()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        LogSnapshot("POST");
    }

    private static void LogSnapshot(string label)
    {
        CameraMobile cameraMobile = FindFirstObjectByType<CameraMobile>(FindObjectsInactive.Exclude);
        Transform cameraTransform = cameraMobile != null ? cameraMobile.transform : null;
        Transform bodyTransform = cameraMobile != null ? cameraMobile.corpoDoJogador : null;
        WeaponSystem weaponSystem = FindFirstObjectByType<WeaponSystem>(FindObjectsInactive.Exclude);
        Weapon weapon = weaponSystem != null ? weaponSystem.CurrentWeapon : null;
        InventarioJogador inventory = FindFirstObjectByType<InventarioJogador>(FindObjectsInactive.Exclude);

        Vector3 bodyPosition = bodyTransform != null ? bodyTransform.position : Vector3.zero;
        float bodyYaw = bodyTransform != null ? NormalizeAngle(bodyTransform.eulerAngles.y) : 0f;
        float cameraPitch = cameraTransform != null ? NormalizeAngle(cameraTransform.localEulerAngles.x) : 0f;
        float cameraRoll = cameraTransform != null ? NormalizeAngle(cameraTransform.localEulerAngles.z) : 0f;
        int currentAmmo = weapon != null ? weapon.CurrentAmmo : -1;
        int capacity = weapon != null ? weapon.AmmoCapacity : -1;
        int reserveAmmo = inventory != null ? inventory.balasNoBolso : -1;

        Debug.Log(
            "MOBILE_INPUT_PROBE " +
            "label=" + label +
            " touches=" + Input.touchCount +
            " bodyX=" + bodyPosition.x.ToString("F3") +
            " bodyY=" + bodyPosition.y.ToString("F3") +
            " bodyZ=" + bodyPosition.z.ToString("F3") +
            " yaw=" + bodyYaw.ToString("F3") +
            " pitch=" + cameraPitch.ToString("F3") +
            " roll=" + cameraRoll.ToString("F3") +
            " ammo=" + currentAmmo +
            " capacity=" + capacity +
            " reserve=" + reserveAmmo);
    }

    private static float NormalizeAngle(float angle)
    {
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }
}
