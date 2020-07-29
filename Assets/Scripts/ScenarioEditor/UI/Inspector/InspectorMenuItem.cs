/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.Inspector
{
    using UnityEngine;
    using UnityEngine.UI;

    public class InspectorMenuItem : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private InspectorMenu inspectorMenu;

        [SerializeField]
        private Text nameText;
#pragma warning restore 0649

        private IInspectorContentPanel panel;

        public void Setup(IInspectorContentPanel panel)
        {
            this.panel = panel;
            nameText.text = panel.MenuItemTitle;
        }

        public void ShowPanel()
        {
            inspectorMenu.ShowPanel(panel);
        }
    }
}