/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.MapEdit
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ElementEditButton : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private Text titleText;
#pragma warning restore 0649

        private IElementMapEdit currentMapEditTarget;

        private ScenarioElementMapPanel parentPanel;

        private void Start()
        {
            parentPanel = GetComponentInParent<ScenarioElementMapPanel>();
        }

        public void Initialize(IElementMapEdit targetMapEdit)
        {
            currentMapEditTarget = targetMapEdit;
            titleText.text = targetMapEdit.Title;
        }

        public void Pressed()
        {
            currentMapEditTarget.Edit();
        }
    }
}