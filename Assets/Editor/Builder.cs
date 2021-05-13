using System;
using UnityEditor;
using UnityEditor.Build.Reporting;


class Builder
{
    static string WinBuildFolder = "Build/Win";
    static string LinuxBuildFolder = "Build/Linux";

    readonly struct Scene
    {
        public Scene(string name_, string scenePath_)
        {
            name = name_;
            scenePath = scenePath_;
        }
        public string name {get; }
        public string scenePath {get; }
    }
    static void BuildAllScenes()
    {
        Scene[] scenes =
        {
            new Scene("SimpleParkingScene", "Assets/Scenes/SimpleParkingScene.unity"),
            new Scene("MediumParkingSceneSingleAgent", "Assets/Scenes/MediumParkingSceneSingleAgent.unity"),
            new Scene("MediumParkingSceneMultiAgent", "Assets/Scenes/MediumParkingSceneMultiAgent.unity"),
            new Scene("MediumParkingSceneSingleAgent3VS", "Assets/Scenes/MediumParkingSceneSingleAgent3VS.unity"),
            new Scene("MediumParkingSceneMultiAgent3VS", "Assets/Scenes/MediumParkingSceneMultiAgent3VS.unity"),
        };

        // build for windows64
        foreach(Scene scene in scenes)
        {
            BuildPlayerOptions buildPlayerOptions = PlayerOptionsWindows64(scene);
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            BuildSummary summary = report.summary; 
            if (summary.result == BuildResult.Succeeded)
            {
                Console.WriteLine("Build {0} succeeded", scene.name);
            }

            if (summary.result == BuildResult.Failed)
            {
                Console.WriteLine("Build {0} failed", scene.name);
            }

        }

        // build for linux64Headless
        foreach(Scene scene in scenes)
        {
            BuildPlayerOptions buildPlayerOptions = PlayerOptionsLinux64Headless(scene);
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            BuildSummary summary = report.summary; 
            if (summary.result == BuildResult.Succeeded)
            {
                Console.WriteLine("Build {0}H succeeded", scene.name);
            }

            if (summary.result == BuildResult.Failed)
            {
                Console.WriteLine("Build {0}H failed", scene.name);
            }
        }
    }

    static BuildPlayerOptions PlayerOptionsWindows64(Scene scene)
    {
        BuildOptions buildOptions = BuildOptions.None;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        buildPlayerOptions.scenes = new string[]{ scene.scenePath };
        buildPlayerOptions.options = buildOptions;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.locationPathName = String.Format("{0}/{1}/{1}.exe", WinBuildFolder, scene.name);

        return buildPlayerOptions;
    }

    static BuildPlayerOptions PlayerOptionsLinux64Headless(Scene scene)
    {
        BuildOptions buildOptions = BuildOptions.EnableHeadlessMode;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        buildPlayerOptions.scenes = new string[]{ scene.scenePath };
        buildPlayerOptions.options = buildOptions;
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.locationPathName = String.Format("{0}/{1}H/{1}H.x86_64", LinuxBuildFolder, scene.name);

        return buildPlayerOptions;
    }
}
