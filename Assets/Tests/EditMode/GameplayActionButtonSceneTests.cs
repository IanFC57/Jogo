#if UNITY_EDITOR
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameplayActionButtonSceneTests
{
    private const string ScenePath = "Assets/Scenes/JogoComMenu.unity";

    [SetUp]
    public void LoadGameplayScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [Test]
    public void FireAndReloadButtonsAreFiftyPercentLarger()
    {
        AssertActionButton("Botao_Recarregar", new Vector2(315f, 129f), new Vector2(-484f, 130f));
        AssertActionButton("Botao_Atirar", new Vector2(285f, 129f), new Vector2(-170f, 130f));
    }

    private static void AssertActionButton(string objectName, Vector2 expectedSize, Vector2 expectedPosition)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        Assert.NotNull(buttonObject, $"{objectName} precisa existir na HUD mobile.");

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        Assert.NotNull(rect);
        Assert.AreEqual(expectedSize.x, rect.sizeDelta.x, 0.01f);
        Assert.AreEqual(expectedSize.y, rect.sizeDelta.y, 0.01f);
        Assert.AreEqual(expectedPosition.x, rect.anchoredPosition.x, 0.01f);
        Assert.AreEqual(expectedPosition.y, rect.anchoredPosition.y, 0.01f);
        Assert.AreEqual(Vector3.one, rect.localScale);

        Assert.NotNull(buttonObject.GetComponent<Button>(), $"{objectName} precisa continuar clicavel.");
        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        Assert.NotNull(label, $"{objectName} precisa ter label TextMeshPro.");
        Assert.GreaterOrEqual(label.fontSizeMax, 34f);
    }
}
#endif
