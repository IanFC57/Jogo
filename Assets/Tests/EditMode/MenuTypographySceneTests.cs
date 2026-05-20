#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuTypographySceneTests
{
    private const string ScenePath = "Assets/Scenes/MenuInicial.unity";
    private const float ExpectedMenuFontSize = 34f;

    [SetUp]
    public void LoadMenuScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [Test]
    public void FunctionalMenuButtonsUseTheSameFixedFontSize()
    {
        List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button.onClick.GetPersistentEventCount() == 0)
                continue;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            Assert.NotNull(label, $"O botao funcional {button.name} precisa de label TextMeshPro.");
            labels.Add(label);
        }

        Assert.AreEqual(2, labels.Count, "A tela inicial deve ter apenas os botoes funcionais Comecar jogo e Sair.");

        foreach (TextMeshProUGUI label in labels)
        {
            Assert.AreEqual(ExpectedMenuFontSize, label.fontSize, 0.01f, $"{label.name} precisa manter o mesmo tamanho visual.");
            Assert.IsFalse(label.enableAutoSizing, $"{label.name} nao pode usar autosizing na tela inicial.");
            Assert.AreEqual(ExpectedMenuFontSize, label.fontSizeMin, 0.01f);
            Assert.AreEqual(ExpectedMenuFontSize, label.fontSizeMax, 0.01f);
            Assert.AreEqual(Vector3.one, label.rectTransform.localScale, $"{label.name} nao pode compensar tamanho por escala local.");
        }
    }

    [Test]
    public void UnusedDuplicateMenuButtonIsInactive()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Button duplicate = null;

        foreach (Button button in buttons)
        {
            if (button.name == "Button (Legacy) (1)")
            {
                duplicate = button;
                break;
            }
        }

        Assert.NotNull(duplicate, "A cena ainda contem o antigo botao duplicado para evitar recriacao acidental.");
        Assert.IsFalse(duplicate.gameObject.activeSelf, "O botao duplicado sem acao deve ficar inativo na tela inicial.");
    }
}
#endif
