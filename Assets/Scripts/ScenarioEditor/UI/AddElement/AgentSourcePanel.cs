/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.AddElement
{
    using Agents;
    using UnityEngine;
    using UnityEngine.UI;

    public class AgentSourcePanel : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private Text title;

        [SerializeField]
        private RawImage image;
#pragma warning restore 0649

        private ScenarioAgentSource agentSource;

        public void Initialize(ScenarioAgentSource source)
        {
            agentSource = source;
            title.text = source?.AgentTypeName;
            Texture texture = null;
            if (source != null && source.AgentVariants.Count > 0)
                texture = source.AgentVariants[0].IconTexture;
            image.texture = texture;
        }

        private void OnDestroy()
        {
            agentSource?.Deinitialize();
        }

        public void DragNewAgent()
        {
            agentSource.DragNewAgent();
        }
    }
}