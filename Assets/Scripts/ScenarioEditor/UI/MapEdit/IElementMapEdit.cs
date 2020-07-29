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

    public interface IElementMapEdit
    {
        string Title { get; }
        Type[] TargetTypes { get; }

        ScenarioElement CurrentElement { get; set; }

        void Edit();
    }
}