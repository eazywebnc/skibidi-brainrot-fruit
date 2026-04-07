using UnityEditor;
using UnityEngine;

namespace SkibidiBrainrotFruit.Editor
{
    public static class WebGLBuilder
    {
        [MenuItem("Build/WebGL Build")]
        public static void Build()
        {
            string[] scenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Game.unity"
            };

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Build/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(options);
        }
    }
}
