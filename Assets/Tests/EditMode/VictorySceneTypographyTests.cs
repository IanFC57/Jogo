#if UNITY_EDITOR
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class VictorySceneTypographyTests
{
    private const string ScenePath = "Assets/Scenes/Final.unity";

    [SetUp]
    public void LoadVictoryScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [Test]
    public void VictorySceneShowsOnlyButtonTextFromUnity()
    {
        AssertInactiveOrMissing("Texto_TituloFinal");
        AssertInactiveOrMissing("Texto_SubtituloFinal");

        TextMeshProUGUI[] texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            Assert.NotNull(
                texts[i].GetComponentInParent<Button>(),
                $"{texts[i].name} nao deve aparecer como texto solto na tela final; esta tela deve mostrar apenas botoes de UI.");
        }
    }

    [Test]
    public void VictoryButtonsUseImprovedLabelsAndActions()
    {
        AssertButton("Button (Legacy)", "Jogar novamente", "Reiniciar", new Vector2(-480f, -283f));
        AssertButton("Button (Legacy) (1)", "Menu inicial", "IrParaMenu", new Vector2(0f, -283f));
        AssertButton("Button (Legacy) (2)", "Sair do jogo", "SairDoJogo", new Vector2(480f, -283f));
    }

    private static void AssertInactiveOrMissing(string objectName)
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == objectName)
            {
                Assert.IsFalse(transforms[i].gameObject.activeSelf, $"{objectName} deve ficar inativo; o texto valido ja esta na imagem de fundo.");
                return;
            }
        }
    }

    private static void AssertButton(string objectName, string expectedLabel, string expectedMethod, Vector2 expectedPosition)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        Assert.NotNull(buttonObject, $"{objectName} precisa existir na tela de encerramento.");
        Assert.IsTrue(buttonObject.activeSelf);

        Button button = buttonObject.GetComponent<Button>();
        Assert.NotNull(button);
        Assert.AreEqual(1, button.onClick.GetPersistentEventCount());
        Assert.AreEqual(expectedMethod, button.onClick.GetPersistentMethodName(0));

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        Assert.NotNull(rect);
        Assert.AreEqual(new Vector2(430f, 82f), rect.sizeDelta);
        Assert.AreEqual(expectedPosition.x, rect.anchoredPosition.x, 0.01f);
        Assert.AreEqual(expectedPosition.y, rect.anchoredPosition.y, 0.01f);
        Assert.AreEqual(Vector3.one, rect.localScale);

        Image image = buttonObject.GetComponent<Image>();
        Assert.NotNull(image);
        Assert.AreEqual(0f, image.color.a, 0.01f, $"{objectName} nao deve ter fundo preto/opaco.");

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        Assert.NotNull(label);
        Assert.AreEqual(expectedLabel, label.text);
        Assert.AreEqual(34f, label.fontSize, 0.01f);
        Assert.AreEqual(Color.white, label.color);
        Assert.IsFalse(label.enableAutoSizing);
    }
}
#endif
