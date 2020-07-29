/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Managers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Database;
    using ICSharpCode.SharpZipLib.Zip;
    using PetaPoco;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using YamlDotNet.Serialization;

    public class ScenarioMapManager
    {
        private const string MapPersistenceKey = "Simulator/ScenarioEditor/MapManager/MapName";
        private string loadedSceneName;

        public string CurrentMapName { get; private set; }

        public Bounds CurrentMapBounds { get; private set; }

        public event Action<string> MapChanged;

        public List<MapModel> ListMaps()
        {
            using (var db = DatabaseManager.Open())
            {
                var sql = Sql.Builder.From("maps");
                return db.Fetch<MapModel>(sql);
            }
        }

        public bool MapExists(string name)
        {
            using (var db = DatabaseManager.Open())
            {
                var sql = Sql.Builder.From("maps").Where("name = @0", name);
                var map = db.FirstOrDefault<MapModel>(sql);
                return map != null;
            }
        }

        public void LoadMap(string mapName = null, Action<string> callback = null)
        {
            ScenarioManager.Instance.ShowLoadingPanel();
            if (!string.IsNullOrEmpty(loadedSceneName))
                UnloadMap();
            using (var db = DatabaseManager.Open())
            {
                var name = string.IsNullOrEmpty(mapName) ? PlayerPrefs.GetString(MapPersistenceKey, null) : mapName;
                if (string.IsNullOrEmpty(name))
                {
                    var sql = Sql.Builder.From("maps");
                    var map = db.FirstOrDefault<MapModel>(sql);

                    Loader.Instance.StartCoroutine(LoadMapAssets(map, map.Name, callback));
                }
                else
                {
                    var sql = Sql.Builder.From("maps").Where("name = @0", name);
                    var map = db.FirstOrDefault<MapModel>(sql);
                    if (map == null)
                    {
                        sql = Sql.Builder.From("maps");
                        map = db.First<MapModel>(sql);
                        name = map.Name;
                        Debug.LogWarning($"Environment '{name}' is not available. Loading '{map.Name}' instead.");
                    }

                    Loader.Instance.StartCoroutine(LoadMapAssets(map, name, callback));
                }
            }
        }

        public void UnloadMap()
        {
            if (string.IsNullOrEmpty(loadedSceneName)) return;
            SceneManager.UnloadSceneAsync(loadedSceneName);
            loadedSceneName = null;
        }

        private IEnumerator LoadMapAssets(MapModel map, string name, Action<string> callback)
        {
            AssetBundle textureBundle = null;
            AssetBundle mapBundle = null;

            ZipFile zip = new ZipFile(map.LocalPath);
            try
            {
                Manifest manifest;
                ZipEntry entry = zip.GetEntry("manifest");
                using (var ms = zip.GetInputStream(entry))
                {
                    int streamSize = (int) entry.Size;
                    byte[] buffer = new byte[streamSize];
                    streamSize = ms.Read(buffer, 0, streamSize);
                    manifest = new Deserializer().Deserialize<Manifest>(Encoding.UTF8.GetString(buffer));
                }

                if (manifest.bundleFormat != BundleConfig.Versions[BundleConfig.BundleTypes.Environment])
                {
                    Debug.LogError(
                        "Out of date Map AssetBundle. Please check content website for updated bundle or rebuild the bundle.");
                    yield break;
                }

                if (zip.FindEntry($"{manifest.assetGuid}_environment_textures", true) != -1)
                {
                    var texStream = zip.GetInputStream(zip.GetEntry($"{manifest.assetGuid}_environment_textures"));
                    textureBundle = AssetBundle.LoadFromStream(texStream, 0, 1 << 20);
                }

                string platform = SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows
                    ? "windows"
                    : "linux";
                var mapStream =
                    zip.GetInputStream(zip.GetEntry($"{manifest.assetGuid}_environment_main_{platform}"));
                mapBundle = AssetBundle.LoadFromStream(mapStream, 0, 1 << 20);

                if (mapBundle == null)
                {
                    Debug.LogError($"Failed to load environment from '{map.Name}' asset bundle");
                    yield break;
                }

                textureBundle?.LoadAllAssets();

                var scenes = mapBundle.GetAllScenePaths();
                if (scenes.Length != 1)
                {
                    Debug.LogError($"Unsupported environment in '{map.Name}' asset bundle, only 1 scene expected");
                    yield break;
                }

                var sceneName = Path.GetFileNameWithoutExtension(scenes[0]);

                loadedSceneName = sceneName;
                var loader = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                yield return new WaitUntil(() => loader.isDone);
                var scene = SceneManager.GetSceneByName(sceneName);
                SceneManager.SetActiveScene(scene);
                SIM.LogAPI(SIM.API.SimulationLoad, sceneName);

                if (Loader.Instance.SimConfig != null)
                {
                    Loader.Instance.SimConfig.MapName = name;
                    Loader.Instance.SimConfig.MapUrl = map.Url;
                }

                CurrentMapName = name;
                CurrentMapBounds = CalculateMapBounds(scene);
                // FixShaders(scene);
                MapChanged?.Invoke(name);
                callback?.Invoke(name);
            }
            finally
            {
                textureBundle?.Unload(false);
                mapBundle?.Unload(false);
                zip.Close();
            }
        }

        private Bounds CalculateMapBounds(Scene scene)
        {
            var gameObjectsOnScene = scene.GetRootGameObjects();
            var b = new Bounds(Vector3.zero, Vector3.zero);
            for (var i = 0; i < gameObjectsOnScene.Length; i++)
            {
                var gameObjectOnScene = gameObjectsOnScene[i];
                foreach (Renderer r in gameObjectOnScene.GetComponentsInChildren<Renderer>())
                {
                    b.Encapsulate(r.bounds);
                }
            }

            //Add margin to the bounds
            b.size += Vector3.one * 10;
            return b;
        }

        // private void FixShaders(Scene scene)
        // {
        // 	var gameObjectsOnScene = scene.GetRootGameObjects();
        // 	for (var i = 0; i < gameObjectsOnScene.Length; i++)
        // 	{
        // 		var gameObjectOnScene = gameObjectsOnScene[i];
        // 		foreach (Renderer r in gameObjectOnScene.GetComponentsInChildren<Renderer>())
        // 			foreach (var material in r.materials)
        // 				material.shader = Shader.Find(material.shader.name);;
        // 	}
        // }
    }
}