// BatchSetup.cs — Unity Editor batch-mode script for Skibidi Brainrot Fruit
// Run via: unity -batchmode -executeMethod SkibidiBrainrotFruit.Editor.BatchSetup.Run -quit -logFile -
//
// Creates: URP pipeline asset, placeholder materials/meshes, all prefabs,
//          MainMenu + Game scenes, GameConfig ScriptableObject, tags, and build settings.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using TMPro;

namespace SkibidiBrainrotFruit.Editor
{
    public static class BatchSetup
    {
        // ── Paths ──────────────────────────────────────────────────────────
        private const string Root            = "Assets";
        private const string ArtDir          = Root + "/Art";
        private const string MaterialDir     = ArtDir + "/Materials";
        private const string PrefabDir       = Root + "/Prefabs";
        private const string ObstaclePrefDir = PrefabDir + "/Obstacles";
        private const string TrackPrefDir    = PrefabDir + "/Track";
        private const string SceneDir        = Root + "/Scenes";
        private const string ResourceDir     = Root + "/Resources";
        private const string RenderDir       = Root + "/Settings/Rendering";

        // ── Colors ─────────────────────────────────────────────────────────
        private static readonly Color TrackColor     = new Color(0.35f, 0.35f, 0.40f);
        private static readonly Color PlayerColor    = new Color(1f, 0.4f, 0.1f);     // orange fruit
        private static readonly Color ObstacleColor  = new Color(0.8f, 0.15f, 0.15f);
        private static readonly Color CoinColor      = new Color(1f, 0.85f, 0.1f);
        private static readonly Color SkyColor       = new Color(0.45f, 0.75f, 1f);

        // ── Entry Point ────────────────────────────────────────────────────
        [MenuItem("SkibidiBrainrotFruit/Run Full Setup")]
        public static void Run()
        {
            Debug.Log("[BatchSetup] Starting full project setup...");

            EnsureDirectories();
            EnsureTags();

            var urpAsset      = CreateURPAsset();
            var materials     = CreateMaterials(urpAsset);
            var gameConfig    = CreateGameConfig();
            var coinPrefab    = CreateCoinPrefab(materials["Coin"]);
            var obstaclePrefabs = CreateObstaclePrefabs(materials["Obstacle"]);
            var chunkPrefabs  = CreateTrackChunkPrefabs(materials["Track"]);
            var playerPrefab  = CreatePlayerPrefab(materials["Player"], gameConfig);
            var explosionPrefab = CreateExplosionParticlePrefab();

            CreateMainMenuScene(gameConfig);
            CreateGameScene(gameConfig, playerPrefab, chunkPrefabs, obstaclePrefabs,
                            coinPrefab, explosionPrefab);

            SetBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[BatchSetup] Setup complete! Build settings configured.");
        }

        // ── Directory setup ────────────────────────────────────────────────
        private static void EnsureDirectories()
        {
            string[] dirs = {
                ArtDir, MaterialDir, PrefabDir, ObstaclePrefDir, TrackPrefDir,
                SceneDir, ResourceDir, RenderDir
            };
            foreach (var d in dirs)
            {
                if (!AssetDatabase.IsValidFolder(d))
                {
                    string parent = Path.GetDirectoryName(d).Replace("\\", "/");
                    string folder = Path.GetFileName(d);
                    AssetDatabase.CreateFolder(parent, folder);
                }
            }
        }

        // ── Tags ───────────────────────────────────────────────────────────
        private static void EnsureTags()
        {
            AddTag("Obstacle");
            // "Player" tag is built-in
        }

        private static void AddTag(string tag)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
            }

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"[BatchSetup] Added tag: {tag}");
        }

        // ── URP Pipeline ──────────────────────────────────────────────────
        private static UniversalRenderPipelineAsset CreateURPAsset()
        {
            string path = RenderDir + "/SkibidiURP.asset";
            var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (existing != null) { SetGraphicsSettings(existing); return existing; }

            var data = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(data, RenderDir + "/SkibidiURP_Renderer.asset");

            var urp = UniversalRenderPipelineAsset.Create(data);
            urp.renderScale = 1f;
            urp.msaaSampleCount = 4;
            urp.supportsHDR = false;
            AssetDatabase.CreateAsset(urp, path);

            SetGraphicsSettings(urp);
            Debug.Log("[BatchSetup] Created URP pipeline asset.");
            return urp;
        }

        private static void SetGraphicsSettings(UniversalRenderPipelineAsset urp)
        {
            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;
        }

        // ── Materials ──────────────────────────────────────────────────────
        private static Dictionary<string, Material> CreateMaterials(
            UniversalRenderPipelineAsset urp)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                      ?? Shader.Find("Standard");

            var mats = new Dictionary<string, Material>();
            mats["Track"]    = GetOrCreateMat("M_Track",    TrackColor,    shader);
            mats["Player"]   = GetOrCreateMat("M_Player",   PlayerColor,   shader);
            mats["Obstacle"] = GetOrCreateMat("M_Obstacle", ObstacleColor, shader);
            mats["Coin"]     = GetOrCreateMat("M_Coin",     CoinColor,     shader);
            return mats;
        }

        private static Material GetOrCreateMat(string name, Color color, Shader shader)
        {
            string path = MaterialDir + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            mat = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        // ── GameConfig ScriptableObject ────────────────────────────────────
        private static Core.GameConfig CreateGameConfig()
        {
            string path = ResourceDir + "/GameConfig.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Core.GameConfig>(path);
            if (existing != null) return existing;

            var cfg = ScriptableObject.CreateInstance<Core.GameConfig>();
            AssetDatabase.CreateAsset(cfg, path);
            Debug.Log("[BatchSetup] Created GameConfig ScriptableObject.");
            return cfg;
        }

        // ── Prefabs: Coin ──────────────────────────────────────────────────
        private static GameObject CreateCoinPrefab(Material mat)
        {
            string path = PrefabDir + "/Coin.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Coin";
            go.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
            go.GetComponent<Renderer>().sharedMaterial = mat;

            // Add Coin script component + trigger collider
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1f;

            go.AddComponent<Coins.Coin>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log("[BatchSetup] Created Coin prefab.");
            return prefab;
        }

        // ── Prefabs: Obstacles ─────────────────────────────────────────────
        private static Dictionary<string, GameObject> CreateObstaclePrefabs(Material mat)
        {
            var prefabs = new Dictionary<string, GameObject>();
            prefabs["LowWall"]     = CreateObstacle("LowWall",     mat, new Vector3(2.5f, 1f,  0.5f));
            prefabs["HighWall"]    = CreateObstacle("HighWall",    mat, new Vector3(2.5f, 2.5f, 0.5f));
            prefabs["LaneBlock"]   = CreateObstacle("LaneBlock",   mat, new Vector3(2.5f, 3f,  0.5f));
            prefabs["MovingBlock"] = CreateObstacle("MovingBlock", mat, new Vector3(1.5f, 1.5f, 1.5f));
            return prefabs;
        }

        private static GameObject CreateObstacle(string name, Material mat, Vector3 scale)
        {
            string path = ObstaclePrefDir + "/" + name + ".prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.tag = "Obstacle";
            go.transform.localScale = scale;
            go.transform.position = new Vector3(0, scale.y * 0.5f, 0);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.AddComponent<Obstacles.Obstacle>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[BatchSetup] Created obstacle prefab: {name}");
            return prefab;
        }

        // ── Prefabs: Track Chunks ──────────────────────────────────────────
        private static GameObject[] CreateTrackChunkPrefabs(Material mat)
        {
            var chunks = new List<GameObject>();

            // Create 3 chunk variants
            for (int i = 0; i < 3; i++)
            {
                string chunkName = $"TrackChunk_{i:D2}";
                string path = TrackPrefDir + "/" + chunkName + ".prefab";

                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (existing != null) { chunks.Add(existing); continue; }

                var go = new GameObject(chunkName);

                // Floor plane (30m long track, 9m wide = 3 lanes * 3m)
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Floor";
                floor.transform.SetParent(go.transform);
                floor.transform.localScale = new Vector3(9f, 0.2f, 30f);
                floor.transform.localPosition = new Vector3(0f, -0.1f, 15f);
                floor.GetComponent<Renderer>().sharedMaterial = mat;

                // Left wall
                var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftWall.name = "WallLeft";
                leftWall.transform.SetParent(go.transform);
                leftWall.transform.localScale = new Vector3(0.3f, 2f, 30f);
                leftWall.transform.localPosition = new Vector3(-4.65f, 1f, 15f);
                leftWall.GetComponent<Renderer>().sharedMaterial = mat;

                // Right wall
                var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightWall.name = "WallRight";
                rightWall.transform.SetParent(go.transform);
                rightWall.transform.localScale = new Vector3(0.3f, 2f, 30f);
                rightWall.transform.localPosition = new Vector3(4.65f, 1f, 15f);
                rightWall.GetComponent<Renderer>().sharedMaterial = mat;

                // Add TrackChunk component
                go.AddComponent<Track.TrackChunk>();

                var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                Object.DestroyImmediate(go);
                chunks.Add(prefab);
                Debug.Log($"[BatchSetup] Created track chunk: {chunkName}");
            }

            return chunks.ToArray();
        }

        // ── Prefabs: Player ────────────────────────────────────────────────
        private static GameObject CreatePlayerPrefab(Material mat, Core.GameConfig config)
        {
            string path = PrefabDir + "/Player.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("Player");
            go.tag = "Player";

            // Fruit model (sphere placeholder)
            var fruitModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruitModel.name = "FruitModel";
            fruitModel.transform.SetParent(go.transform);
            fruitModel.transform.localPosition = new Vector3(0, 0.5f, 0);
            fruitModel.GetComponent<Renderer>().sharedMaterial = mat;
            // Remove mesh collider from the visual — CharacterController handles physics
            var meshCol = fruitModel.GetComponent<Collider>();
            if (meshCol != null) Object.DestroyImmediate(meshCol);

            // CharacterController
            var cc = go.AddComponent<CharacterController>();
            cc.center = new Vector3(0, 0.5f, 0);
            cc.height = 1f;
            cc.radius = 0.4f;
            cc.slopeLimit = 0;
            cc.stepOffset = 0.1f;

            // PlayerController
            var pc = go.AddComponent<Player.PlayerController>();
            SetSerializedField(pc, "_config", config);
            SetSerializedField(pc, "_fruitModel", fruitModel.transform);

            // NearMissDetector (trigger collider for near-miss detection)
            var nmd = go.AddComponent<Effects.NearMissDetector>();
            SetSerializedField(nmd, "_config", config);
            var nearMissTrigger = go.AddComponent<SphereCollider>();
            nearMissTrigger.isTrigger = true;
            nearMissTrigger.radius = 1.5f;
            nearMissTrigger.center = new Vector3(0, 0.5f, 0);

            // DeathEffect
            var de = go.AddComponent<Effects.DeathEffect>();
            SetSerializedField(de, "_fruitModel", fruitModel.transform);

            // PlayerInputHandler
            var pih = go.AddComponent<Input.PlayerInputHandler>();
            SetSerializedField(pih, "_player", pc);
            SetSerializedField(pih, "_config", config);

            // CoinCollector
            go.AddComponent<Coins.CoinCollector>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log("[BatchSetup] Created Player prefab.");
            return prefab;
        }

        // ── Prefabs: Explosion Particle ────────────────────────────────────
        private static GameObject CreateExplosionParticlePrefab()
        {
            string path = PrefabDir + "/ExplosionParticle.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("ExplosionParticle");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.4f;
            main.startSpeed = 8f;
            main.startSize = 0.3f;
            main.startColor = PlayerColor;
            main.maxParticles = 30;
            main.loop = false;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log("[BatchSetup] Created ExplosionParticle prefab.");
            return prefab;
        }

        // ── Scene: MainMenu ────────────────────────────────────────────────
        private static void CreateMainMenuScene(Core.GameConfig config)
        {
            string path = SceneDir + "/MainMenu.unity";
            if (File.Exists(path)) { Debug.Log("[BatchSetup] MainMenu scene already exists."); return; }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SkyColor;
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<AudioListener>();

            // Directional Light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
            lightGo.AddComponent<UniversalAdditionalLightData>();

            // GameManager (singleton, persists)
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManagement.GameManager>();
            SetSerializedField(gm, "_config", config);

            // EventSystem (for UI)
            var eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSys.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Canvas
            var canvas = CreateUICanvas("MenuCanvas");

            // Title text
            var titleGo = CreateTMPText(canvas.transform, "TitleText", "SKIBIDI\nBRAINROT FRUIT",
                                        72, TextAlignmentOptions.Center,
                                        new Vector2(0, 120), new Vector2(800, 200));

            // Play button
            var playBtn = CreateButton(canvas.transform, "PlayButton", "PLAY",
                                       new Vector2(0, -40), new Vector2(300, 80));
            var menuUI = canvas.gameObject.AddComponent<UI.MainMenuUI>();

            // Settings panel (hidden by default)
            var settingsPanel = new GameObject("SettingsPanel");
            settingsPanel.transform.SetParent(canvas.transform, false);
            var settingsRect = settingsPanel.AddComponent<RectTransform>();
            settingsRect.anchoredPosition = Vector2.zero;
            settingsRect.sizeDelta = new Vector2(600, 400);
            var settingsBg = settingsPanel.AddComponent<UnityEngine.UI.Image>();
            settingsBg.color = new Color(0, 0, 0, 0.85f);
            settingsPanel.SetActive(false);

            SetSerializedField(menuUI, "_settingsPanel", settingsPanel);

            // Wire play button
            var btnComp = playBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btnComp.onClick,
                new UnityEngine.Events.UnityAction(menuUI.OnPlayClicked));

            // Settings button
            var settingsBtn = CreateButton(canvas.transform, "SettingsButton", "SETTINGS",
                                           new Vector2(0, -140), new Vector2(300, 60));
            var settingsBtnComp = settingsBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                settingsBtnComp.onClick,
                new UnityEngine.Events.UnityAction(menuUI.OnSettingsClicked));

            EditorSceneManager.SaveScene(scene, path);
            Debug.Log("[BatchSetup] Created MainMenu scene.");
        }

        // ── Scene: Game ────────────────────────────────────────────────────
        private static void CreateGameScene(
            Core.GameConfig config,
            GameObject playerPrefab,
            GameObject[] chunkPrefabs,
            Dictionary<string, GameObject> obstaclePrefabs,
            GameObject coinPrefab,
            GameObject explosionPrefab)
        {
            string path = SceneDir + "/Game.unity";
            if (File.Exists(path)) { Debug.Log("[BatchSetup] Game scene already exists."); return; }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SkyColor;
            camGo.transform.position = new Vector3(0, 5, -8);
            camGo.transform.rotation = Quaternion.Euler(20, 0, 0);
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<AudioListener>();

            // ScreenShake on camera
            var screenShake = camGo.AddComponent<Effects.ScreenShake>();
            SetSerializedField(screenShake, "_config", config);

            // Directional Light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
            lightGo.AddComponent<UniversalAdditionalLightData>();

            // GameSceneInit
            var initGo = new GameObject("GameSceneInit");
            initGo.AddComponent<GameManagement.GameSceneInit>();

            // EventSystem
            var eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSys.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Player (instance from prefab)
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = Vector3.zero;

            // Wire explosion prefab on DeathEffect
            var deathEffect = player.GetComponent<Effects.DeathEffect>();
            if (deathEffect != null)
                SetSerializedField(deathEffect, "_explosionParticlePrefab", explosionPrefab);

            // TrackGenerator
            var trackGo = new GameObject("TrackGenerator");
            var trackGen = trackGo.AddComponent<Track.TrackGenerator>();
            SetSerializedField(trackGen, "_config", config);
            SetSerializedField(trackGen, "_playerTransform", player.transform);
            SetSerializedFieldArray(trackGen, "_chunkPrefabs", chunkPrefabs);

            // ObstacleSpawner
            var obstGo = new GameObject("ObstacleSpawner");
            var obstSpawner = obstGo.AddComponent<Obstacles.ObstacleSpawner>();
            SetSerializedField(obstSpawner, "_config", config);
            SetSerializedField(obstSpawner, "_playerTransform", player.transform);
            SetSerializedField(obstSpawner, "_lowWallPrefab",     obstaclePrefabs["LowWall"]);
            SetSerializedField(obstSpawner, "_highWallPrefab",    obstaclePrefabs["HighWall"]);
            SetSerializedField(obstSpawner, "_laneBlockPrefab",   obstaclePrefabs["LaneBlock"]);
            SetSerializedField(obstSpawner, "_movingBlockPrefab", obstaclePrefabs["MovingBlock"]);

            // CoinSpawner
            var coinGo = new GameObject("CoinSpawner");
            var coinSpawner = coinGo.AddComponent<Coins.CoinSpawner>();
            SetSerializedField(coinSpawner, "_config", config);
            SetSerializedField(coinSpawner, "_playerTransform", player.transform);
            SetSerializedField(coinSpawner, "_coinPrefab", coinPrefab);

            // ScoreManager
            var scoreGo = new GameObject("ScoreManager");
            var scoreMgr = scoreGo.AddComponent<Scoring.ScoreManager>();
            SetSerializedField(scoreMgr, "_player", player.GetComponent<Player.PlayerController>());

            // ── UI Canvas (HUD + GameOver) ─────────────────────────────────
            var canvas = CreateUICanvas("GameCanvas");

            // HUD
            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(canvas.transform, false);
            var hudRect = hudGo.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            var scoreText = CreateTMPText(hudGo.transform, "ScoreText", "0m",
                                          36, TextAlignmentOptions.TopLeft,
                                          new Vector2(20, -20), new Vector2(200, 50));
            var coinText  = CreateTMPText(hudGo.transform, "CoinText", "0",
                                          36, TextAlignmentOptions.TopRight,
                                          new Vector2(-20, -20), new Vector2(200, 50));
            // Anchor coin text to top-right
            var coinRect = coinText.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(1, 1);
            coinRect.anchorMax = new Vector2(1, 1);
            coinRect.pivot = new Vector2(1, 1);

            // Anchor score text to top-left
            var scoreRect = scoreText.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.pivot = new Vector2(0, 1);

            // Pause panel
            var pausePanel = CreatePanel(hudGo.transform, "PausePanel", new Color(0, 0, 0, 0.7f));
            CreateTMPText(pausePanel.transform, "PausedText", "PAUSED",
                          48, TextAlignmentOptions.Center, new Vector2(0, 40), new Vector2(400, 80));
            var resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "RESUME",
                                         new Vector2(0, -40), new Vector2(250, 60));
            pausePanel.SetActive(false);

            // Pause button
            var pauseBtn = CreateButton(hudGo.transform, "PauseButton", "||",
                                        new Vector2(-50, -50), new Vector2(60, 60));
            var pauseBtnRect = pauseBtn.GetComponent<RectTransform>();
            pauseBtnRect.anchorMin = new Vector2(1, 1);
            pauseBtnRect.anchorMax = new Vector2(1, 1);
            pauseBtnRect.pivot = new Vector2(1, 1);

            var hud = hudGo.AddComponent<UI.HUDController>();
            SetSerializedField(hud, "_scoreText", scoreText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hud, "_coinText", coinText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hud, "_pausePanel", pausePanel);

            // Wire buttons
            var pauseBtnComp = pauseBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                pauseBtnComp.onClick,
                new UnityEngine.Events.UnityAction(hud.OnPauseButtonClicked));
            var resumeBtnComp = resumeBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                resumeBtnComp.onClick,
                new UnityEngine.Events.UnityAction(hud.OnResumeButtonClicked));

            // GameOver UI
            var gameOverGo = new GameObject("GameOverUI");
            gameOverGo.transform.SetParent(canvas.transform, false);

            var goPanel = CreatePanel(gameOverGo.transform, "GameOverPanel", new Color(0, 0, 0, 0.8f));
            var goScoreText    = CreateTMPText(goPanel.transform, "ScoreText", "0m",
                                               48, TextAlignmentOptions.Center,
                                               new Vector2(0, 80), new Vector2(400, 60));
            var goHighText     = CreateTMPText(goPanel.transform, "HighScoreText", "Best: 0m",
                                               32, TextAlignmentOptions.Center,
                                               new Vector2(0, 20), new Vector2(400, 50));
            var goCoinsText    = CreateTMPText(goPanel.transform, "CoinsText", "0 coins",
                                               28, TextAlignmentOptions.Center,
                                               new Vector2(0, -30), new Vector2(400, 40));
            var retryBtn       = CreateButton(goPanel.transform, "RetryButton", "RETRY",
                                              new Vector2(0, -100), new Vector2(250, 60));
            var menuBtn        = CreateButton(goPanel.transform, "MenuButton", "MENU",
                                              new Vector2(0, -180), new Vector2(250, 60));
            goPanel.SetActive(false);

            var goUI = gameOverGo.AddComponent<UI.GameOverUI>();
            SetSerializedField(goUI, "_panel", goPanel);
            SetSerializedField(goUI, "_scoreText", goScoreText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(goUI, "_highScoreText", goHighText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(goUI, "_coinsText", goCoinsText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(goUI, "_scoreManager", scoreMgr);

            var retryBtnComp = retryBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                retryBtnComp.onClick,
                new UnityEngine.Events.UnityAction(goUI.OnRetryClicked));
            var menuBtnComp = menuBtn.GetComponent<UnityEngine.UI.Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                menuBtnComp.onClick,
                new UnityEngine.Events.UnityAction(goUI.OnMenuClicked));

            EditorSceneManager.SaveScene(scene, path);
            Debug.Log("[BatchSetup] Created Game scene.");
        }

        // ── Build Settings ─────────────────────────────────────────────────
        private static void SetBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(SceneDir + "/MainMenu.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Game.unity", true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[BatchSetup] Build settings: MainMenu (0), Game (1).");
        }

        // ── UI Helpers ─────────────────────────────────────────────────────
        private static Canvas CreateUICanvas(string name)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateTMPText(Transform parent, string name, string text,
            float fontSize, TextAlignmentOptions align, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.enableAutoSizing = false;

            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            var btn = go.AddComponent<UnityEngine.UI.Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.4f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.2f);
            btn.colors = colors;

            CreateTMPText(go.transform, "Label", label, 28, TextAlignmentOptions.Center,
                          Vector2.zero, size);

            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = bgColor;
            return go;
        }

        // ── Reflection helpers for serialized private fields ───────────────
        private static void SetSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[BatchSetup] Field '{fieldName}' not found on {target.GetType().Name}");
            }
        }

        private static void SetSerializedFieldArray(Object target, string fieldName, GameObject[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null && prop.isArray)
            {
                prop.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++)
                {
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[BatchSetup] Array field '{fieldName}' not found on {target.GetType().Name}");
            }
        }
    }
}
#endif
