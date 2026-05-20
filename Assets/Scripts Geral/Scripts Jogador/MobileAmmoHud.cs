using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public sealed class MobileAmmoHud : MonoBehaviour
{
    private const string HudObjectName = "Texto_Balas";
    private const int MinAmmoFontSize = 24;
    private const int MaxAmmoFontSize = 34;
    private static readonly Vector2 AmmoTextSize = new Vector2(760f, 44f);
    private static readonly Vector2 AmmoTextOffset = new Vector2(0f, -52f);
    private static bool initialized;

    private TMP_Text ammoTextTmp;
    private Text ammoText;
    private TMP_Text healthTextTmp;
    private Text healthText;
    private WeaponSystem weaponSystem;
    private InventarioJogador inventory;
    private Weapon currentWeapon;
    private int lastCurrentAmmo = int.MinValue;
    private int lastCapacity = int.MinValue;
    private int lastReserve = int.MinValue;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (initialized) return;

        initialized = true;
        SceneManager.sceneLoaded += (_, _) => EnsureHudRunner();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureHudRunner()
    {
        if (Object.FindFirstObjectByType<MobileAmmoHud>(FindObjectsInactive.Include) != null)
            return;

        GameObject runner = new GameObject(nameof(MobileAmmoHud));
        runner.AddComponent<MobileAmmoHud>();
    }

    private void Update()
    {
        ResolveReferences();
        UpdateAmmoText();
    }

    private void ResolveReferences()
    {
        if (inventory == null)
        {
            inventory = Object.FindFirstObjectByType<InventarioJogador>(FindObjectsInactive.Exclude);
        }

        if (weaponSystem == null)
        {
            weaponSystem = Object.FindFirstObjectByType<WeaponSystem>(FindObjectsInactive.Exclude);
        }

        Weapon weapon = weaponSystem != null ? weaponSystem.CurrentWeapon : null;
        if (weapon == null)
        {
            weapon = FindActivePlayerWeapon();
        }

        if (currentWeapon != weapon)
        {
            currentWeapon = weapon;
            lastCurrentAmmo = int.MinValue;
        }

        if (currentWeapon != null && Application.isMobilePlatform)
        {
            currentWeapon.showCurrentAmmo = false;
        }

        if (ammoText == null && ammoTextTmp == null)
        {
            EnsureAmmoText();
        }
    }

    private void EnsureAmmoText()
    {
        if (healthText == null)
        {
            PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Exclude);
            if (playerHealth != null)
            {
                healthText = playerHealth.textoDeVida;
                healthTextTmp = playerHealth.textoDeVidaTmp;
            }
        }

        if (healthTextTmp == null && healthText != null)
        {
            healthTextTmp = healthText.GetComponent<TMP_Text>();
        }

        if (healthText == null && healthTextTmp != null)
        {
            healthText = healthTextTmp.GetComponent<Text>();
        }

        Transform healthTransform = healthTextTmp != null ? healthTextTmp.transform : healthText != null ? healthText.transform : null;
        if (healthTransform == null) return;

        Transform parent = healthTransform.parent;
        Transform existing = parent != null ? parent.Find(HudObjectName) : null;
        if (existing != null)
        {
            ammoTextTmp = existing.GetComponent<TMP_Text>();
            ammoText = existing.GetComponent<Text>();
            if (ammoTextTmp != null || ammoText != null)
            {
                ConfigureAmmoText();
                return;
            }
        }

        GameObject textObject = new GameObject(HudObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        ammoTextTmp = textObject.GetComponent<TMP_Text>();
        ConfigureAmmoText();
    }

    private void ConfigureAmmoText()
    {
        if ((ammoText == null && ammoTextTmp == null) || (healthText == null && healthTextTmp == null)) return;

        Transform healthTransform = healthTextTmp != null ? healthTextTmp.transform : healthText.transform;
        Transform ammoTransform = ammoTextTmp != null ? ammoTextTmp.transform : ammoText.transform;
        RectTransform healthRect = healthTransform as RectTransform;
        RectTransform ammoRect = ammoTransform as RectTransform;
        if (healthRect != null && ammoRect != null)
        {
            ammoRect.anchorMin = healthRect.anchorMin;
            ammoRect.anchorMax = healthRect.anchorMax;
            ammoRect.pivot = healthRect.pivot;
            ammoRect.sizeDelta = AmmoTextSize;
            ammoRect.anchoredPosition = healthRect.anchoredPosition + AmmoTextOffset;
            ammoRect.localScale = Vector3.one;
        }

        if (ammoTextTmp != null)
        {
            ammoTextTmp.font = healthTextTmp != null ? healthTextTmp.font : ammoTextTmp.font;
            ammoTextTmp.fontSize = Mathf.Clamp(healthTextTmp != null ? healthTextTmp.fontSize * 0.72f : MaxAmmoFontSize, MinAmmoFontSize, MaxAmmoFontSize);
            ammoTextTmp.fontStyle = healthTextTmp != null ? healthTextTmp.fontStyle : FontStyles.Bold;
            ammoTextTmp.color = healthTextTmp != null ? healthTextTmp.color : healthText != null ? healthText.color : Color.white;
            ammoTextTmp.alignment = TextAlignmentOptions.TopLeft;
            ammoTextTmp.textWrappingMode = TextWrappingModes.NoWrap;
            ammoTextTmp.overflowMode = TextOverflowModes.Overflow;
            ammoTextTmp.enableAutoSizing = true;
            ammoTextTmp.fontSizeMin = MinAmmoFontSize;
            ammoTextTmp.fontSizeMax = MaxAmmoFontSize;
            ammoTextTmp.raycastTarget = false;
        }

        if (ammoText != null)
        {
            ammoText.font = healthText != null ? healthText.font : ammoText.font;
            ammoText.fontSize = Mathf.Clamp(healthText != null ? healthText.fontSize : MaxAmmoFontSize, MinAmmoFontSize, MaxAmmoFontSize);
            ammoText.fontStyle = healthText != null ? healthText.fontStyle : FontStyle.Bold;
            ammoText.color = healthText != null ? healthText.color : Color.white;
            ammoText.alignment = TextAnchor.UpperLeft;
            ammoText.horizontalOverflow = HorizontalWrapMode.Wrap;
            ammoText.verticalOverflow = VerticalWrapMode.Overflow;
            ammoText.resizeTextForBestFit = true;
            ammoText.resizeTextMinSize = MinAmmoFontSize;
            ammoText.resizeTextMaxSize = MaxAmmoFontSize;
            ammoText.raycastTarget = false;
        }
    }

    private void UpdateAmmoText()
    {
        if ((ammoText == null && ammoTextTmp == null) || currentWeapon == null) return;

        int reserve = inventory != null ? inventory.balasNoBolso : 0;
        if (lastCurrentAmmo == currentWeapon.CurrentAmmo &&
            lastCapacity == currentWeapon.AmmoCapacity &&
            lastReserve == reserve)
        {
            return;
        }

        lastCurrentAmmo = currentWeapon.CurrentAmmo;
        lastCapacity = currentWeapon.AmmoCapacity;
        lastReserve = reserve;
        string text = "Balas: " + currentWeapon.CurrentAmmo + "/" + currentWeapon.AmmoCapacity + "  Reserva: " + reserve;

        if (ammoTextTmp != null)
        {
            ammoTextTmp.text = text;
        }

        if (ammoText != null)
        {
            ammoText.text = text;
        }
    }

    private static Weapon FindActivePlayerWeapon()
    {
        Weapon[] weapons = Object.FindObjectsByType<Weapon>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < weapons.Length; i++)
        {
            Weapon weapon = weapons[i];
            if (weapon != null && weapon.playerWeapon && weapon.gameObject.activeInHierarchy)
            {
                return weapon;
            }
        }

        return null;
    }
}
