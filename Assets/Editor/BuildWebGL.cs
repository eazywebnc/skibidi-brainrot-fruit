using UnityEditor;
using UnityEngine;

public static class BuildWebGL
{
    [MenuItem("Build/WebGL")]
    public static void Build()
    {
        string[] scenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Game.unity"
        };

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(buildOptions);
        Debug.Log("WebGL build completed!");
    }
}
