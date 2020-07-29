/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.EditElement
{
    using Inspector;
    using UnityEngine;

    public class EditElementPanel : MonoBehaviour, IInspectorContentPanel
    {
        public string MenuItemTitle => "Edit";

        void IInspectorContentPanel.Show()
        {
            gameObject.SetActive(true);
        }

        void IInspectorContentPanel.Hide()
        {
            gameObject.SetActive(false);
        }
    }
}