/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.MapEdit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Inspector;
    using Managers;
    using UnityEngine;

    public class MapSelectPanel : MonoBehaviour, IInspectorContentPanel
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private MapSelectButton buttonSample;
#pragma warning restore 0649

        private List<MapSelectButton> buttons = new List<MapSelectButton>();

        private MapSelectButton currentMapButton;

        public string MenuItemTitle => "Map";

        public void Start()
        {
            var mapManager = ScenarioManager.Instance.MapManager;
            mapManager.MapChanged += OnMapLoaded;
            var availableMaps = ScenarioManager.Instance.MapManager.ListMaps();
            var currentMapName = ScenarioManager.Instance.MapManager.CurrentMapName;
            for (var i = 0; i < availableMaps.Count; i++)
            {
                var availableMap = availableMaps[i];
                var mapSelectButton = Instantiate(buttonSample, buttonSample.transform.parent);
                mapSelectButton.Setup(availableMap.Name);
                mapSelectButton.gameObject.SetActive(true);
                if (currentMapName == availableMap.Name)
                {
                    mapSelectButton.MarkAsCurrent();
                    currentMapButton = mapSelectButton;
                }

                buttons.Add(mapSelectButton);
            }

            buttonSample.gameObject.SetActive(false);
        }

        public void OnDestroy()
        {
            if (ScenarioManager.Instance != null)
                ScenarioManager.Instance.MapManager.MapChanged -= OnMapLoaded;
        }

        private void OnMapLoaded(string mapName)
        {
            currentMapButton.UnmarkCurrent();
            var mapCorrespondingButton = buttons.Find((button) => button.MapName == mapName);
            if (mapCorrespondingButton == null)
                throw new ArgumentException("Could not find button corresponding to loaded map.");
            mapCorrespondingButton.MarkAsCurrent();
            currentMapButton = mapCorrespondingButton;
        }

        void IInspectorContentPanel.Show()
        {
            gameObject.SetActive(true);
        }

        void IInspectorContentPanel.Hide()
        {
            gameObject.SetActive(false);
        }

        public void SelectMap(string mapName)
        {
            ScenarioManager.Instance.ShowLoadingPanel();
            //Delay selecting map so the loading panel can initialize
            StartCoroutine(DelayedSelectMap(mapName));
        }

        private IEnumerator DelayedSelectMap(string mapName)
        {
            yield return null;
            ScenarioManager.Instance.ResetScenario();
            ScenarioManager.Instance.MapManager.LoadMap(mapName);
        }
    }
}