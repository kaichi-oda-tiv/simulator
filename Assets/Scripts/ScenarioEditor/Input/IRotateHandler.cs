/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Input
{
    using UnityEngine;

    public interface IRotateHandler
    {
        void RotationStarted(Vector2 viewportPosition);

        void RotationChanged(Vector2 viewportPosition);

        void RotationFinished(Vector2 viewportPosition);

        void RotationCancelled(Vector2 viewportPosition);
    }
}