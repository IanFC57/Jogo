using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.Android;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AndroidBrandingAndTypography
{
    private const string IconPath = "Assets/AppIcon/AsylumHorrorIcon.png";
    private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const float MainMenuButtonFontSize = 34f;

    [MenuItem("Tools/Android/Apply Branding And Typography")]
    public static void Apply()
    {
        ConfigureIconImporter();
        ConfigureAndroidIcons();
        ConvertBuildSceneTextToTextMeshPro();
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureIconImporter()
    {
        TextureImporter importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException($"Icon texture not found at {IconPath}.");
        }

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void ConfigureAndroidIcons()
    {
        Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (iconTexture == null)
        {
            throw new InvalidOperationException($"Icon texture failed to load at {IconPath}.");
        }

        PlatformIconKind[] iconKinds =
        {
            AndroidPlatformIconKind.Legacy,
            AndroidPlatformIconKind.Round,
            AndroidPlatformIconKind.Adaptive
        };

        foreach (PlatformIconKind kind in iconKinds)
        {
            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            for (int i = 0; i < icons.Length; i++)
            {
                for (int layer = 0; layer < icons[i].maxLayerCount; layer++)
                {
                    icons[i].SetTexture(iconTexture, layer);
                }
            }

            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
        }

        Debug.Log($"Android app icon configured from {IconPath}.");
    }

    private static void ConvertBuildSceneTextToTextMeshPro()
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset == null)
        {
            throw new InvalidOperationException($"TMP font asset not found at {FontPath}.");
        }

        string[] scenePaths = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        Scene activeScene = SceneManager.GetActiveScene();

        for (int i = 0; i < scenePaths.Length; i++)
        {
            Scene scene = GetLoadedSceneByPath(scenePaths[i]);
            bool closeSceneAfterProcessing = !scene.IsValid() || !scene.isLoaded;
            if (closeSceneAfterProcessing)
            {
                scene = EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Additive);
            }

            Text[] legacyTexts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int converted = 0;

            foreach (Text legacyText in legacyTexts)
            {
                if (legacyText == null || legacyText.gameObject.scene != scene)
                    continue;

                ConvertTextComponent(legacyText, fontAsset);
                converted++;
            }

            TextMeshProUGUI[] tmpTexts = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TextMeshProUGUI text in tmpTexts)
            {
                if (text == null || text.gameObject.scene != scene)
                    continue;

                ApplyStandardTypography(text, fontAsset);
            }

            ApplySceneSpecificLayout(scene, fontAsset);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Typography updated in {scenePaths[i]}: {converted} legacy Text components converted.");

            if (closeSceneAfterProcessing)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        if (activeScene.IsValid() && activeScene.isLoaded)
        {
            EditorSceneManager.SetActiveScene(activeScene);
        }
    }

    private static Scene GetLoadedSceneByPath(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath)
                return scene;
        }

        return default;
    }

    private static void ConvertTextComponent(Text legacyText, TMP_FontAsset fontAsset)
    {
        GameObject textObject = legacyText.gameObject;
        string content = legacyText.text;
        Color color = legacyText.color;
        int fontSize = legacyText.fontSize;
        FontStyle fontStyle = legacyText.fontStyle;
        TextAnchor alignment = legacyText.alignment;
        bool richText = legacyText.supportRichText;
        bool bestFit = legacyText.resizeTextForBestFit;
        int minSize = legacyText.resizeTextMinSize;
        int maxSize = legacyText.resizeTextMaxSize;
        HorizontalWrapMode horizontalOverflow = legacyText.horizontalOverflow;
        VerticalWrapMode verticalOverflow = legacyText.verticalOverflow;

        UnityEngine.Object.DestroyImmediate(legacyText, true);

        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = content;
        tmpText.color = color;
        tmpText.fontSize = Mathf.Max(1, fontSize);
        tmpText.fontStyle = ConvertFontStyle(fontStyle);
        tmpText.alignment = ConvertAlignment(alignment);
        tmpText.richText = richText;
        tmpText.enableAutoSizing = bestFit || IsGameplayHudText(textObject);
        tmpText.fontSizeMin = minSize > 0 ? minSize : Mathf.Max(8, fontSize - 4);
        tmpText.fontSizeMax = maxSize > 0 ? maxSize : Mathf.Max(fontSize + 8, fontSize);
        tmpText.textWrappingMode = horizontalOverflow == HorizontalWrapMode.Wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        tmpText.overflowMode = verticalOverflow == VerticalWrapMode.Overflow ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
        tmpText.raycastTarget = false;

        ApplyStandardTypography(tmpText, fontAsset);
    }

    private static void ApplyStandardTypography(TextMeshProUGUI text, TMP_FontAsset fontAsset)
    {
        text.font = fontAsset;
        text.extraPadding = true;

        if (IsButtonLabel(text.gameObject))
        {
            text.fontStyle |= FontStyles.Bold;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(10, text.fontSize * 0.65f);
            text.fontSizeMax = Mathf.Max(text.fontSize, text.fontSizeMax);
            text.alignment = TextAlignmentOptions.Center;
        }

        if (IsGameplayHudText(text.gameObject))
        {
            text.fontStyle |= FontStyles.Bold;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(8, text.fontSize * 0.75f);
            text.fontSizeMax = Mathf.Max(text.fontSize + 4, text.fontSizeMax);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
        }
    }

    private static void ApplySceneSpecificLayout(Scene scene, TMP_FontAsset fontAsset)
    {
        if (scene.path.EndsWith("MenuInicial.unity", StringComparison.OrdinalIgnoreCase))
        {
            ApplyMainMenuLayout(scene, fontAsset);
            return;
        }

        if (scene.path.EndsWith("Final.unity", StringComparison.OrdinalIgnoreCase))
        {
            ApplyVictoryLayout(scene, fontAsset);
            return;
        }

        if (!scene.path.EndsWith("JogoComMenu.unity", StringComparison.OrdinalIgnoreCase))
            return;

        ConfigureHudText(scene, fontAsset, "Texto_Vida",
            anchor: new Vector2(0f, 1f),
            pivot: new Vector2(0f, 1f),
            anchoredPosition: new Vector2(42f, -46f),
            size: new Vector2(460f, 52f),
            alignment: TextAlignmentOptions.TopLeft,
            fontSize: 34f,
            minSize: 30f,
            maxSize: 38f,
            wrapping: TextWrappingModes.NoWrap);

        ConfigureHudText(scene, fontAsset, "TextoAviso",
            anchor: new Vector2(0.5f, 1f),
            pivot: new Vector2(0.5f, 1f),
            anchoredPosition: new Vector2(0f, -24f),
            size: new Vector2(1180f, 104f),
            alignment: TextAlignmentOptions.Top,
            fontSize: 34f,
            minSize: 24f,
            maxSize: 38f,
            wrapping: TextWrappingModes.Normal);

        ConfigureActionButton(scene, fontAsset, "Botao_Recarregar", "Recarregar",
            anchoredPosition: new Vector2(-484f, 130f),
            size: new Vector2(315f, 129f));

        ConfigureActionButton(scene, fontAsset, "Botao_Atirar", "Atirar",
            anchoredPosition: new Vector2(-170f, 130f),
            size: new Vector2(285f, 129f));
    }

    private static void ApplyMainMenuLayout(Scene scene, TMP_FontAsset fontAsset)
    {
        ConfigureMainMenuButton(scene, fontAsset, "Button (Legacy)", "Come\u00e7ar jogo", new Vector2(0f, -125f));
        ConfigureMainMenuButton(scene, fontAsset, "Button (Legacy) (2)", "Sair", new Vector2(0f, -235f));
        SetSceneObjectActive(scene, "Button (Legacy) (1)", false);
    }

    private static void ConfigureMainMenuButton(Scene scene, TMP_FontAsset fontAsset, string buttonName, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = FindSceneObject(scene, buttonName);
        if (buttonObject == null)
            return;

        buttonObject.SetActive(true);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = new Vector2(430f, 82f);
            buttonRect.localScale = Vector3.one;
        }

        TextMeshProUGUI text = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text == null)
            return;

        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
            textRect.localScale = Vector3.one;
        }

        text.text = label;
        text.font = fontAsset;
        text.fontStyle |= FontStyles.Bold;
        text.fontSize = MainMenuButtonFontSize;
        text.enableAutoSizing = false;
        text.fontSizeMin = MainMenuButtonFontSize;
        text.fontSizeMax = MainMenuButtonFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        text.extraPadding = true;
    }

    private static void SetSceneObjectActive(Scene scene, string objectName, bool active)
    {
        GameObject sceneObject = FindSceneObject(scene, objectName);
        if (sceneObject != null)
        {
            sceneObject.SetActive(active);
        }
    }

    private static void ApplyVictoryLayout(Scene scene, TMP_FontAsset fontAsset)
    {
        Canvas canvas = FindSceneComponent<Canvas>(scene);
        if (canvas == null)
            return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        ConfigureVictoryText(
            canvasRect,
            fontAsset,
            "Texto_TituloFinal",
            "Fase concluida",
            new Vector2(0f, 220f),
            new Vector2(1120f, 96f),
            58f,
            new Color(0.86f, 0.08f, 0.06f, 1f),
            FontStyles.Bold);

        ConfigureVictoryText(
            canvasRect,
            fontAsset,
            "Texto_SubtituloFinal",
            "Voce atravessou a noite. O silencio ainda esta observando.",
            new Vector2(0f, 130f),
            new Vector2(1180f, 78f),
            31f,
            new Color(0.92f, 0.82f, 0.75f, 1f),
            FontStyles.Normal);

        MenuVitoria victoryMenu = FindSceneComponent<MenuVitoria>(scene);
        ConfigureVictoryButton(scene, fontAsset, victoryMenu, "Button (Legacy)", "Jogar novamente", "Reiniciar", new Vector2(0f, -130f));
        ConfigureVictoryButton(scene, fontAsset, victoryMenu, "Button (Legacy) (1)", "Menu inicial", "IrParaMenu", new Vector2(0f, -232f));
        ConfigureVictoryButton(scene, fontAsset, victoryMenu, "Button (Legacy) (2)", "Sair do jogo", "SairDoJogo", new Vector2(0f, -334f));
    }

    private static void ConfigureVictoryText(
        RectTransform parent,
        TMP_FontAsset fontAsset,
        string objectName,
        string content,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        Color color,
        FontStyles style)
    {
        TextMeshProUGUI text = EnsureTextObject(parent, objectName);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        text.text = content;
        text.font = fontAsset;
        text.color = color;
        text.fontStyle = style;
        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.fontSizeMin = fontSize;
        text.fontSizeMax = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        text.extraPadding = true;
    }

    private static TextMeshProUGUI EnsureTextObject(RectTransform parent, string objectName)
    {
        Transform existing = FindChildRecursive(parent, objectName);
        if (existing != null)
        {
            TextMeshProUGUI existingText = existing.GetComponent<TextMeshProUGUI>();
            if (existingText != null)
                return existingText;
        }

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.layer = parent.gameObject.layer;
        textObject.transform.SetParent(parent, false);
        return textObject.GetComponent<TextMeshProUGUI>();
    }

    private static void ConfigureVictoryButton(
        Scene scene,
        TMP_FontAsset fontAsset,
        MenuVitoria victoryMenu,
        string buttonName,
        string label,
        string methodName,
        Vector2 anchoredPosition)
    {
        GameObject buttonObject = FindSceneObject(scene, buttonName);
        if (buttonObject == null)
            return;

        buttonObject.SetActive(true);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = new Vector2(480f, 82f);
            buttonRect.localScale = Vector3.one;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button != null && victoryMenu != null)
        {
            ConfigureVictoryButtonAction(button, victoryMenu, methodName);
        }

        TextMeshProUGUI text = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text == null)
            return;

        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
            textRect.localScale = Vector3.one;
        }

        text.text = label;
        text.font = fontAsset;
        text.color = new Color(0.98f, 0.05f, 0.04f, 1f);
        text.fontStyle |= FontStyles.Bold;
        text.fontSize = 31f;
        text.enableAutoSizing = false;
        text.fontSizeMin = 31f;
        text.fontSizeMax = 31f;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        text.extraPadding = true;
    }

    private static void ConfigureVictoryButtonAction(Button button, MenuVitoria victoryMenu, string methodName)
    {
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }

        switch (methodName)
        {
            case nameof(MenuVitoria.Reiniciar):
                UnityEventTools.AddPersistentListener(button.onClick, victoryMenu.Reiniciar);
                break;
            case nameof(MenuVitoria.IrParaMenu):
                UnityEventTools.AddPersistentListener(button.onClick, victoryMenu.IrParaMenu);
                break;
            case nameof(MenuVitoria.SairDoJogo):
                UnityEventTools.AddPersistentListener(button.onClick, victoryMenu.SairDoJogo);
                break;
        }
    }

    private static void ConfigureHudText(
        Scene scene,
        TMP_FontAsset fontAsset,
        string objectName,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size,
        TextAlignmentOptions alignment,
        float fontSize,
        float minSize,
        float maxSize,
        TextWrappingModes wrapping)
    {
        GameObject textObject = FindSceneObject(scene, objectName);
        if (textObject == null)
            return;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (rect == null || text == null)
            return;

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        text.font = fontAsset;
        text.fontStyle |= FontStyles.Bold;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.alignment = alignment;
        text.textWrappingMode = wrapping;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.extraPadding = true;
    }

    private static void ConfigureActionButton(Scene scene, TMP_FontAsset fontAsset, string buttonName, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = FindSceneObject(scene, buttonName);
        if (buttonObject == null)
            return;

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = size;
            buttonRect.localScale = Vector3.one;
        }

        TextMeshProUGUI text = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text == null)
            return;

        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(-20f, -10f);
            textRect.localScale = Vector3.one;
        }

        text.text = label;
        text.font = fontAsset;
        text.fontStyle |= FontStyles.Bold;
        text.fontSize = 30f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 22f;
        text.fontSizeMax = 34f;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        text.extraPadding = true;
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindChildRecursive(roots[i].transform, objectName);
            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    private static T FindSceneComponent<T>(Scene scene) where T : Component
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            T component = roots[i].GetComponentInChildren<T>(true);
            if (component != null)
                return component;
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform parent, string objectName)
    {
        if (parent.name == objectName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), objectName);
            if (found != null)
                return found;
        }

        return null;
    }

    private static bool IsButtonLabel(GameObject textObject)
    {
        return textObject.GetComponentInParent<Button>(true) != null;
    }

    private static bool IsGameplayHudText(GameObject textObject)
    {
        return textObject.name == "Texto_Vida" ||
               textObject.name == "Texto_Balas" ||
               textObject.name == "TextoAviso";
    }

    private static FontStyles ConvertFontStyle(FontStyle style)
    {
        FontStyles converted = FontStyles.Normal;
        if (style == FontStyle.Bold || style == FontStyle.BoldAndItalic)
        {
            converted |= FontStyles.Bold;
        }

        if (style == FontStyle.Italic || style == FontStyle.BoldAndItalic)
        {
            converted |= FontStyles.Italic;
        }

        return converted;
    }

    private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter:
                return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:
                return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:
                return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter:
                return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight:
                return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft:
                return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:
                return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:
                return TextAlignmentOptions.BottomRight;
            default:
                return TextAlignmentOptions.Center;
        }
    }
}
