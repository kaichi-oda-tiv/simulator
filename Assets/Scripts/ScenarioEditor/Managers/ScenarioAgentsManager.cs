/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Managers
{
    using System;
    using System.Collections.Generic;
    using Agents;
    using UnityEngine;
    using Utilities;

    public class ScenarioAgentsManager : MonoBehaviour
    {
        public List<ScenarioAgentSource> Sources { get; } = new List<ScenarioAgentSource>();

        public List<ScenarioAgent> Agents { get; } = new List<ScenarioAgent>();

        public void Initialize()
        {
            var interfaceType = typeof(ScenarioAgentSource);
            var types = ReflectionCache.FindTypes((type) => !type.IsAbstract && interfaceType.IsAssignableFrom(type));
            for (var i = 0; i < types.Count; i++)
            {
                var agentSource = Activator.CreateInstance(types[i]) as ScenarioAgentSource;
                if (agentSource == null) continue;
                agentSource.Initialize();
                foreach (var agentVariant in agentSource.AgentVariants)
                {
                    //Force rendering a texture of the agent variant
                    var sprite = agentVariant.IconSprite;
                }

                Sources.Add(agentSource);
            }
        }

        public void Deinitialize()
        {
            Sources.Clear();
            Agents.Clear();
        }

        public void RegisterAgent(ScenarioAgent agent)
        {
            Agents.Add(agent);
        }

        public void UnregisterAgent(ScenarioAgent agent)
        {
            Agents.Remove(agent);
        }
    }
}