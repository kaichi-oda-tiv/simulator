/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.MapEdit
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MapSelectButton : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private MapSelectPanel mapSelectPanel;

        [SerializeField]
        private Button uiButton;

        [SerializeField]
        private Text nameText;
#pragma warning restore 0649
        private bool isCurrentMap;

        private string mapName;

        public string MapName
        {
            get => mapName;
            private set => mapName = value;
        }

        public void Setup(string map)
        {
            MapName = map;
            nameText.text = map;
        }

        public void SelectMap()
        {
            if (!isCurrentMap)
                mapSelectPanel.SelectMap(MapName);
        }

        public void MarkAsCurrent()
        {
            uiButton.interactable = false;
            isCurrentMap = true;
        }

        public void UnmarkCurrent()
        {
            uiButton.interactable = true;
            isCurrentMap = false;
        }
    }
}