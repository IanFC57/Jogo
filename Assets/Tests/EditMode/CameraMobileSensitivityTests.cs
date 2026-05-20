#if UNITY_EDITOR
using System.IO;
using NUnit.Framework;

public sealed class CameraMobileSensitivityTests
{
    private const string ScenePath = "Assets/Scenes/JogoComMenu.unity";

    [Test]
    public void DefaultTouchSensitivityIsTwentyPercentHigher()
    {
        Assert.AreEqual(0.144f, MobileCameraRules.DefaultTouchSensitivity, 0.0001f);
    }

    [Test]
    public void GameplaySceneUsesIncreasedTouchSensitivity()
    {
        string sceneYaml = File.ReadAllText(ScenePath);

        StringAssert.Contains("sensibilidadeToque: 0.144", sceneYaml);
    }
}
#endif
