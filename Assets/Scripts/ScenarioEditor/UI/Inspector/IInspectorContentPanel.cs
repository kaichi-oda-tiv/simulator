/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.Inspector
{
    public interface IInspectorContentPanel
    {
        string MenuItemTitle { get; }

        void Show();

        void Hide();
    }
}