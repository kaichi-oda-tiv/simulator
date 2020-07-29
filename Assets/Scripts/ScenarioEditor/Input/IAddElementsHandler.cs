/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Agents
{
    using UnityEngine;

    public interface IAddElementsHandler
    {
        void AddingStarted(Vector3 dragPosition);

        void AddingMoved(Vector3 dragPosition);

        void AddElement(Vector3 dragPosition);

        void AddingCancelled(Vector3 dragPosition);
    }
}