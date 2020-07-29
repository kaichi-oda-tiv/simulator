/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.MapEdit
{
    using System;
    using Elements;
    using Input;
    using Managers;

    public class ElementMapRotate : IElementMapEdit
    {
        public string Title { get; } = "Rotate";
        public Type[] TargetTypes { get; } = {typeof(IRotateHandler)};
        public ScenarioElement CurrentElement { get; set; }

        public void Edit()
        {
            if (CurrentElement == null)
                throw new ArgumentException("Current agent has to be set by external script before editing.");
            ScenarioManager.Instance.inputManager.StartRotatingElement(CurrentElement as IRotateHandler);
        }
    }
}