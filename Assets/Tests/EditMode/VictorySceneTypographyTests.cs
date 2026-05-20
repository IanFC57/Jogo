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
    public void VictorySceneHasClearCompletionCopy()
    {
        AssertText("Texto_TituloFinal", "Fase concluida", 58f);
        AssertText("Texto_SubtituloFinal", "Voce atravessou a noite. O silencio ainda esta observando.", 31f);
    }

    [Test]
    public void VictoryButtonsUseImprovedLabelsAndActions()
    {
        AssertButton("Button (Legacy)", "Jogar novamente", "Reiniciar", new Vector2(0f, -130f));
        AssertButton("Button (Legacy) (1)", "Menu inicial", "IrParaMenu", new Vector2(0f, -232f));
        AssertButton("Button (Legacy) (2)", "Sair do jogo", "SairDoJogo", new Vector2(0f, -334f));
    }

    private static void AssertText(string objectName, string expectedText, float expectedFontSize)
    {
        GameObject textObject = GameObject.Find(objectName);
        Assert.NotNull(textObject, $"{objectName} precisa existir na tela de encerramento.");

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        Assert.NotNull(text);
        Assert.AreEqual(expectedText, text.text);
        Assert.AreEqual(expectedFontSize, text.fontSize, 0.01f);
        Assert.IsFalse(text.enableAutoSizing);
        Assert.AreEqual(Vector3.one, text.rectTransform.localScale);
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
        Assert.AreEqual(new Vector2(480f, 82f), rect.sizeDelta);
        Assert.AreEqual(expectedPosition.x, rect.anchoredPosition.x, 0.01f);
        Assert.AreEqual(expectedPosition.y, rect.anchoredPosition.y, 0.01f);
        Assert.AreEqual(Vector3.one, rect.localScale);

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        Assert.NotNull(label);
        Assert.AreEqual(expectedLabel, label.text);
        Assert.AreEqual(31f, label.fontSize, 0.01f);
        Assert.IsFalse(label.enableAutoSizing);
    }
}
#endif
