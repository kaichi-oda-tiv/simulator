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
    using Managers;

    public class ElementMapRemove : IElementMapEdit
    {
        public string Title { get; } = "Remove";
        public Type[] TargetTypes { get; } = {typeof(ScenarioElement)};
        public ScenarioElement CurrentElement { get; set; }

        public void Edit()
        {
            ScenarioManager.Instance.SelectedElement = null;
            CurrentElement.Destroy();
        }
    }
}