/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Agents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;
    using PetaPoco;
    using Database;
    using FMU;
    using Managers;
    using UnityEngine;
    using YamlDotNet.Serialization;

    public class ScenarioEgoAgentSource : ScenarioAgentSource
    {
        public override string AgentTypeName => "EgoAgent";

        public override int AgentTypeId => 1;

        public override List<AgentVariant> AgentVariants { get; } = new List<AgentVariant>();

        private GameObject draggedInstance;

        public override void Initialize()
        {
            var vehicles = ListModels<VehicleModel>();
            var vehicleModels = vehicles as VehicleModel[] ?? vehicles.ToArray();
            for (var i = 0; i < vehicleModels.Length; i++)
            {
                var vehicleModel = vehicleModels[i];
                var prefab = GetVehiclePrefab(vehicleModel);
                var egoAgent = new AgentVariant()
                {
                    source = this,
                    name = vehicleModel.Name,
                    prefab = prefab
                };
                AgentVariants.Add(egoAgent);
            }
        }

        public override void Deinitialize()
        {
        }

        public override GameObject GetModelInstance(AgentVariant variant)
        {
            var instance = ScenarioManager.Instance.prefabsPools.GetInstance(variant.prefab);
            instance.GetComponent<VehicleController>().enabled = false;
            return instance;
        }

        public override ScenarioAgent GetAgentInstance(AgentVariant variant)
        {
            var newGameObject = new GameObject(AgentTypeName);
            newGameObject.transform.SetParent(ScenarioManager.Instance.transform);
            var scenarioAgent = newGameObject.AddComponent<ScenarioAgent>();
            scenarioAgent.Setup(this, variant);
            return scenarioAgent;
        }

        public override void ReturnModelInstance(GameObject instance)
        {
            ScenarioManager.Instance.prefabsPools.ReturnInstance(instance);
        }

        public override void DragNewAgent()
        {
            ScenarioManager.Instance.inputManager.StartDraggingElement(this);
        }

        public override void DragStarted(Vector3 dragPosition)
        {
            draggedInstance = GetModelInstance(AgentVariants[0]);
            draggedInstance.transform.SetParent(ScenarioManager.Instance.transform);
            draggedInstance.transform.SetPositionAndRotation(dragPosition, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }

        public override void DragMoved(Vector3 dragPosition)
        {
            draggedInstance.transform.position = dragPosition;
        }

        public override void DragFinished(Vector3 dragPosition)
        {
            var agent = GetAgentInstance(AgentVariants[0]);
            agent.transform.SetPositionAndRotation(draggedInstance.transform.position,
                draggedInstance.transform.rotation);
            ScenarioManager.Instance.prefabsPools.ReturnInstance(draggedInstance);
            draggedInstance = null;
        }

        public override void DragCancelled(Vector3 dragPosition)
        {
            ScenarioManager.Instance.prefabsPools.ReturnInstance(draggedInstance);
            draggedInstance = null;
        }

        /// <summary>
        /// Lists models of given type from the database
        /// </summary>
        /// <typeparam name="T">Type of models to list</typeparam>
        /// <returns>Models of given type from the database</returns>
        private IEnumerable<T> ListModels<T>()
        {
            using (var db = DatabaseManager.Open())
            {
                var sql = Sql.Builder
                    .OrderBy("id");

                return db.Fetch<T>(sql);
            }
        }

        private GameObject GetVehiclePrefab(VehicleModel vehicleModel)
        {
            var bundlePath = vehicleModel.LocalPath;

            using (ZipFile zip = new ZipFile(bundlePath))
            {
                Manifest manifest;
                ZipEntry entry = zip.GetEntry("manifest");
                using (var ms = zip.GetInputStream(entry))
                {
                    int streamSize = (int) entry.Size;
                    byte[] buffer = new byte[streamSize];
                    streamSize = ms.Read(buffer, 0, streamSize);
                    manifest = new Deserializer().Deserialize<Manifest>(
                        Encoding.UTF8.GetString(buffer, 0, streamSize));
                }

                if (manifest.bundleFormat != BundleConfig.Versions[BundleConfig.BundleTypes.Vehicle])
                {
                    throw new Exception(
                        "Out of date Vehicle AssetBundle. Please check content website for updated bundle or rebuild the bundle.");
                }

                AssetBundle textureBundle = null;

                if (zip.FindEntry($"{manifest.assetGuid}_vehicle_textures", true) != -1)
                {
                    var texStream = zip.GetInputStream(
                        zip.GetEntry($"{manifest.assetGuid}_vehicle_textures"));
                    textureBundle = AssetBundle.LoadFromStream(texStream, 0, 1 << 20);
                }

                string platform =
                    SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows
                        ? "windows"
                        : "linux";
                var mapStream = zip.GetInputStream(
                    zip.GetEntry($"{manifest.assetGuid}_vehicle_main_{platform}"));
                var vehicleBundle = AssetBundle.LoadFromStream(mapStream, 0, 1 << 20);

                if (vehicleBundle == null)
                {
                    throw new Exception(
                        $"Failed to load '{bundlePath}' vehicle asset bundle");
                }

                try
                {
                    var vehicleAssets = vehicleBundle.GetAllAssetNames();
                    if (vehicleAssets.Length != 1)
                    {
                        throw new Exception(
                            $"Unsupported '{bundlePath}' vehicle asset bundle, only 1 asset expected");
                    }

                    textureBundle?.LoadAllAssets();

                    if (manifest.fmuName != "")
                    {
                        var fmuDirectory = Path.Combine(
                            Application.persistentDataPath,
                            manifest.assetName);
                        if (platform == "windows")
                        {
                            var dll = zip.GetEntry($"{manifest.fmuName}_windows.dll");
                            if (dll == null)
                            {
                                throw new ArgumentException(
                                    $"{manifest.fmuName}.dll not found in Zip");
                            }

                            using (Stream s = zip.GetInputStream(dll))
                            {
                                byte[] buffer = new byte[4096];
                                Directory.CreateDirectory(fmuDirectory);
                                var path = Path.Combine(
                                    Application.persistentDataPath,
                                    manifest.assetName,
                                    $"{manifest.fmuName}.dll");
                                using (FileStream streamWriter = File.Create(path))
                                {
                                    StreamUtils.Copy(s, streamWriter, buffer);
                                }

                                vehicleBundle
                                    .LoadAsset<GameObject>(vehicleAssets[0])
                                    .GetComponent<VehicleFMU>()
                                    .FMUData.Path = path;
                            }
                        }
                        else
                        {
                            var dll = zip.GetEntry($"{manifest.fmuName}_linux.so");
                            if (dll == null)
                            {
                                throw new ArgumentException(
                                    $"{manifest.fmuName}.so not found in Zip");
                            }

                            using (Stream s = zip.GetInputStream(dll))
                            {
                                byte[] buffer = new byte[4096];
                                Directory.CreateDirectory(fmuDirectory);
                                var path = Path.Combine(
                                    Application.persistentDataPath,
                                    manifest.assetName,
                                    $"{manifest.fmuName}.so");
                                using (FileStream streamWriter = File.Create(path))
                                {
                                    StreamUtils.Copy(s, streamWriter, buffer);
                                }

                                vehicleBundle
                                    .LoadAsset<GameObject>(vehicleAssets[0])
                                    .GetComponent<VehicleFMU>()
                                    .FMUData.Path = path;
                            }
                        }
                    }

                    var prefab = vehicleBundle.LoadAsset<GameObject>(vehicleAssets[0]);
                    return prefab;
                }
                finally
                {
                    if (vehicleBundle != null) vehicleBundle.Unload(false);
                    if (textureBundle != null) textureBundle.Unload(false);
                }
            }
        }
    }
}