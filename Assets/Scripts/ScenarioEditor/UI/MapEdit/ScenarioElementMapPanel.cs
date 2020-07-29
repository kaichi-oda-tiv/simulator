/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.MapEdit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Elements;
    using Managers;
    using UnityEngine;
    using Utilities;

    [RequireComponent(typeof(RectTransform))]
    public class ScenarioElementMapPanel : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private Vector2 offsetFromAgent;

        [SerializeField]
        private Camera renderingCamera;

        [SerializeField]
        private GameObject agentEditButtonPrefab;
#pragma warning restore 0649

        private bool isInitialized;

        private ScenarioElement element;

        private IElementMapEdit[] agentEdits;

        private List<ElementEditButton> buttons = new List<ElementEditButton>();

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Deinitialize();
        }

        private void Initialize()
        {
            if (isInitialized)
                return;
            ScenarioManager.Instance.SelectedOtherElement += SelectedOtherElement;
            var interfaceType = typeof(IElementMapEdit);
            var agentEditType =
                ReflectionCache.FindTypes((type) => !type.IsAbstract && interfaceType.IsAssignableFrom(type));
            agentEdits = new IElementMapEdit[agentEditType.Count];
            for (var i = 0; i < agentEditType.Count; i++)
            {
                var buttonType = agentEditType[i];
                agentEdits[i] = Activator.CreateInstance(buttonType) as IElementMapEdit;
            }

            isInitialized = true;
            SelectedOtherElement(ScenarioManager.Instance.SelectedElement);
        }

        private void Deinitialize()
        {
            if (!isInitialized)
                return;
            var scenarioManager = ScenarioManager.Instance;
            if (scenarioManager != null)
                scenarioManager.SelectedOtherElement -= SelectedOtherElement;
            isInitialized = false;
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void SelectedOtherElement(ScenarioElement scenarioElement)
        {
            element = scenarioElement;
            if (element == null)
                Hide();
            else
                Show();
        }

        private void Show()
        {
            PrepareButtonsForAgent();
            gameObject.SetActive(true);
            StartCoroutine(FollowAgent());
        }

        private void PrepareButtonsForAgent()
        {
            var buttonId = 0;
            foreach (var agentEdit in agentEdits)
            {
                if (agentEdit.TargetTypes.Any(targetType => targetType.IsInstanceOfType(element)))
                {
                    if (buttonId >= buttons.Count)
                    {
                        var newButton = Instantiate(agentEditButtonPrefab, transform);
                        buttons.Add(newButton.GetComponent<ElementEditButton>());
                    }

                    buttons[buttonId++].Initialize(agentEdit);
                    agentEdit.CurrentElement = element;
                }
            }

            //Disable unused buttons
            for (var i = buttonId; i < buttons.Count; i++)
                buttons[i].gameObject.SetActive(false);
        }

        private IEnumerator FollowAgent()
        {
            var rectPosition = transform as RectTransform;
            if (rectPosition == null)
                throw new ArgumentException($"{GetType().Name} requires {nameof(RectTransform)} component.");
            while (gameObject.activeSelf)
            {
                Vector2 newPosition = renderingCamera.WorldToScreenPoint(element.transform.position);
                newPosition += offsetFromAgent;
                rectPosition.anchoredPosition = newPosition;
                yield return null;
            }
        }

        private void Hide()
        {
            gameObject.SetActive(false);
            element = null;
        }
    }
}