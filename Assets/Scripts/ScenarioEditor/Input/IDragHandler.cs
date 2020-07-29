/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Input
{
    using UnityEngine;

    public interface IDragHandler
    {
        void DragStarted(Vector3 dragPosition);

        void DragMoved(Vector3 dragPosition);

        void DragFinished(Vector3 dragPosition);

        void DragCancelled(Vector3 dragPosition);
    }
}