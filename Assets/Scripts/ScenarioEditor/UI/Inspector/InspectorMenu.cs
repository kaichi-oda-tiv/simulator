/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.Inspector
{
    using System.Collections.Generic;
    using Network.Core;
    using UnityEngine;

    public class InspectorMenu : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private GameObject inspectorContent;

        [SerializeField]
        private InspectorMenuItem buttonSample;
#pragma warning restore 0649

        private List<IInspectorContentPanel> panels = new List<IInspectorContentPanel>();

        private IInspectorContentPanel activePanel;

        public void Start()
        {
            var availablePanels = inspectorContent.GetComponentsInChildren<IInspectorContentPanel>(true);
            for (var i = 0; i < availablePanels.Length; i++)
            {
                var availablePanel = availablePanels[i];
                panels.Add(availablePanel);
                if (i == 0) availablePanel.Show();
                else availablePanel.Hide();
                var panelMenuItem = Instantiate(buttonSample, buttonSample.transform.parent);
                panelMenuItem.Setup(availablePanel);
                panelMenuItem.gameObject.SetActive(true);
            }

            buttonSample.gameObject.SetActive(false);

            activePanel = availablePanels.Length > 0 ? availablePanels[0] : null;
        }

        public void ShowPanel(IInspectorContentPanel panel)
        {
            if (!panels.Contains(panel))
            {
                Log.Warning("Cannot show inspector panel which is not in the inspector content hierarchy.");
                return;
            }

            activePanel?.Hide();
            panel.Show();
            activePanel = panel;
        }
    }
}