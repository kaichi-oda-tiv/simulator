/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.AddElement
{
    using Inspector;
    using Managers;
    using UnityEngine;

    public class AddAgentsPanel : MonoBehaviour, IInspectorContentPanel
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private GameObject agentSourcePanelPrefab;
#pragma warning restore 0649

        public string MenuItemTitle => "Add";

        private void Start()
        {
            var sources = ScenarioManager.Instance.agentsManager.Sources;
            for (var i = 0; i < sources.Count; i++)
            {
                var newPanel = Instantiate(agentSourcePanelPrefab, transform);
                var agentSourcePanel = newPanel.GetComponent<AgentSourcePanel>();
                agentSourcePanel.Initialize(sources[i]);
            }
        }

        void IInspectorContentPanel.Show()
        {
            gameObject.SetActive(true);
        }

        void IInspectorContentPanel.Hide()
        {
            gameObject.SetActive(false);
        }
    }
}