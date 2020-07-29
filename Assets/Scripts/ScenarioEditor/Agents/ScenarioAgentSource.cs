/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Agents
{
    using System.Collections.Generic;
    using Input;
    using UnityEngine;

    public abstract class ScenarioAgentSource : IDragHandler
    {
        public abstract string AgentTypeName { get; }

        public abstract int AgentTypeId { get; }

        public abstract List<AgentVariant> AgentVariants { get; }

        public abstract void Initialize();

        public abstract void Deinitialize();

        public abstract GameObject GetModelInstance(AgentVariant variant);

        public abstract ScenarioAgent GetAgentInstance(AgentVariant variant);

        public abstract void ReturnModelInstance(GameObject instance);

        public abstract void DragNewAgent();

        public abstract void DragStarted(Vector3 dragPosition);

        public abstract void DragMoved(Vector3 dragPosition);

        public abstract void DragFinished(Vector3 dragPosition);

        public abstract void DragCancelled(Vector3 dragPosition);
    }
}