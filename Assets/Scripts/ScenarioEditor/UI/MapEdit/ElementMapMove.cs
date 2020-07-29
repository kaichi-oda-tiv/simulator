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

    public class ElementMapMove : IElementMapEdit
    {
        public string Title { get; } = "Move";
        public Type[] TargetTypes { get; } = {typeof(IDragHandler)};
        public ScenarioElement CurrentElement { get; set; }

        public void Edit()
        {
            if (CurrentElement == null)
                throw new ArgumentException("Current agent has to be set by external script before editing.");
            ScenarioManager.Instance.inputManager.StartDraggingElement(CurrentElement as IDragHandler);
        }
    }
}