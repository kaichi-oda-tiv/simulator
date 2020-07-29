/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.EditElement
{
    using System.Linq;
    using Agents;
    using Elements;
    using Managers;
    using UnityEngine;
    using UnityEngine.UI;

    public class AgentEditPanel : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private Dropdown agentSelectDropdown;
#pragma warning restore 0649

        private bool isInitialized;

        private ScenarioAgentSource agentSource;

        private ScenarioAgent agent;

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Deinitialize();
        }

        private void Initialize()
        {
            if (isInitialized)
                return;
            ScenarioManager.Instance.SelectedOtherElement += OnSelectedOtherElement;
            isInitialized = true;
            OnSelectedOtherElement(ScenarioManager.Instance.SelectedElement);
        }

        private void Deinitialize()
        {
            if (!isInitialized)
                return;
            var scenarioManager = ScenarioManager.Instance;
            if (scenarioManager != null)
                scenarioManager.SelectedOtherElement -= OnSelectedOtherElement;
            isInitialized = false;
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnSelectedOtherElement(ScenarioElement scenarioElement)
        {
            agent = scenarioElement as ScenarioAgent;
            if (agent == null)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            if (agentSource != agent.Source)
            {
                agentSource = agent.Source;
                agentSelectDropdown.options.Clear();
                agentSelectDropdown.AddOptions(
                    agentSource.AgentVariants.Select(variant => variant.IconSprite).ToList());
            }

            var variantId = agentSource.AgentVariants.IndexOf(agent.Variant);
            agentSelectDropdown.SetValueWithoutNotify(variantId);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            agent = null;
        }

        public void AgentSelectDropdownChanged(Dropdown dropdown)
        {
            agent.ChangeVariant(agentSource.AgentVariants[dropdown.value]);
        }
    }
}