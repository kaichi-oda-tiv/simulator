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
    using Elements;
    using Input;
    using Map;
    using UI.FileEdit;
    using UnityEngine;
    using Utilities;

    public class ScenarioManager : MonoBehaviour
    {
        private static ScenarioManager instance;

        public static ScenarioManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<ScenarioManager>();
                return instance;
            }
            private set
            {
                if (instance == value)
                    return;
                if (instance != null && value != null)
                    throw new ArgumentException($"Instance of {instance.GetType().Name} is already set.");
                instance = value;
            }
        }

        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private Camera scenarioCamera;

        [SerializeField]
        private GameObject loadingPanel;
#pragma warning restore 0649

        private bool isInitialized;

        private Vector3 cameraInitialPosition;

        private Quaternion cameraInitialRotation;

        private ScenarioElement selectedElement;

        private MapHolder cachedMapHolder;

        public InputManager inputManager;

        public ObjectsShotCapture objectsShotCapture;

        public PrefabsPools prefabsPools;

        public SelectFileDialog selectFileDialog;

        public ScenarioAgentsManager agentsManager;

        public ScenarioWaypointsManager waypointsManager;

        public Camera ScenarioCamera => scenarioCamera;

        public ScenarioMapManager MapManager { get; } = new ScenarioMapManager();

        public ScenarioElement SelectedElement
        {
            get => selectedElement;
            set
            {
                if (selectedElement == value)
                    return;
                selectedElement = value;
                SelectedOtherElement?.Invoke(selectedElement);
            }
        }

        public event Action<ScenarioElement> SelectedOtherElement;

        private void Start()
        {
            if (scenarioCamera == null)
                throw new ArgumentException("Scenario camera reference is required in the ScenarioManager.");
            var cameraTransform = scenarioCamera.transform;
            cameraInitialPosition = cameraTransform.position;
            cameraInitialRotation = cameraTransform.rotation;
            if (Instance == null || Instance == this)
            {
                Initialize();
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            Deinitialize();
            Instance = null;
        }

        public void Initialize()
        {
            if (isInitialized)
                return;
            MapManager.MapChanged += OnMapLoaded;
            MapManager.LoadMap();
            agentsManager.Initialize();
            waypointsManager.Initialize();
            Time.timeScale = 0.0f;
            isInitialized = true;
        }

        public void Deinitialize()
        {
            if (!isInitialized)
                return;
            MapManager.MapChanged -= OnMapLoaded;
            MapManager.UnloadMap();
            waypointsManager.Deinitialize();
            agentsManager.Deinitialize();
            Time.timeScale = 1.0f;
            isInitialized = false;
        }

        public void ResetScenario()
        {
            SelectedElement = null;
            var agents = agentsManager.Agents;
            for (var i = agents.Count - 1; i >= 0; i--)
            {
                var agent = agents[i];
                agent.Destroy();
            }

            var waypoints = agentsManager.Agents;
            for (var i = waypoints.Count - 1; i >= 0; i--)
            {
                var waypoint = waypoints[i];
                waypoint.Destroy();
            }
        }

        public void ShowLoadingPanel()
        {
            loadingPanel.gameObject.SetActive(true);
        }

        public void HideLoadingPanel()
        {
            loadingPanel.gameObject.SetActive(false);
        }

        public void OnMapLoaded(string mapName)
        {
            var cameraTransform = ScenarioCamera.transform;
            cameraTransform.position = cameraInitialPosition;
            cameraTransform.rotation = cameraInitialRotation;
            inputManager.RecacheCameraRotation();
            StartCoroutine(DelayedHideLoadingPanel());
        }

        private IEnumerator DelayedHideLoadingPanel()
        {
            //Wait one frame so every gameobject can initialize with Start method
            yield return null;
            HideLoadingPanel();
        }

        // public void EnableSimulationManager()
        // {
        // 	if (SimulatorManager.InstanceAvailable)
        // 	{
        // 		if (cachedMapHolder != null)
        // 			cachedMapHolder.gameObject.SetActive(true);
        // 		if (SimulatorManager.InstanceAvailable)
        // 		{
        // 			SimulatorManager.Instance.gameObject.SetActive(true);
        // 			SimulatorManager.Instance.TimeManager.TimeScaleSemaphore.Unlock();
        // 		}
        // 	}
        // }

        // public void DisableSimulationManager()
        // {
        // 	if (!SimulatorManager.InstanceAvailable)
        // 	{
        // 		Debug.LogError("Cannot enter scenario editor when Simulator has not started.");
        // 		return;
        // 	}
        //
        // 	ScenarioMapName = Loader.Instance.SimConfig.MapName;
        // 	SimulatorManager.Instance.gameObject.SetActive(false);
        // 	cachedMapHolder = FindObjectOfType<MapHolder>();
        // 	cachedMapHolder.gameObject.SetActive(false);
        // 	SimulatorManager.Instance.TimeManager.TimeScaleSemaphore.Lock();
        // 	StartCoroutine(FixAdditiveSceneLightning());
        // }
    }
}