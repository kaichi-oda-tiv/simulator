/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.EditElement
{
    using System.Collections;
    using Agents;
    using Elements;
    using Managers;
    using UnityEngine;
    using UnityEngine.UI;

    public class WaypointEditPanel : MonoBehaviour, IAddElementsHandler
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private GameObject speedPanel;

        [SerializeField]
        private GameObject waitTimePanel;

        [SerializeField]
        private InputField speedInput;

        [SerializeField]
        private InputField waitTimeInput;
#pragma warning restore 0649
        private bool isInitialized;

        private bool isEditing;

        private ScenarioWaypoint waypointInstance;

        private ScenarioAgent selectedAgent;

        private ScenarioWaypoint selectedWaypoint;

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
            ScenarioManager.Instance.SelectedOtherElement += OnSelectedOtherElement;
            isInitialized = true;
            OnSelectedOtherElement(ScenarioManager.Instance.SelectedElement);
        }

        private void Deinitialize()
        {
            if (!isInitialized)
                return;
            var scenarioManager = ScenarioManager.Instance;
            if (scenarioManager != null)
                scenarioManager.SelectedOtherElement -= OnSelectedOtherElement;
            isInitialized = false;
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnSelectedOtherElement(ScenarioElement selectedElement)
        {
            if (isEditing)
                ScenarioManager.Instance.inputManager.CancelAddingElements(this);

            selectedWaypoint = selectedElement as ScenarioWaypoint;
            if (selectedWaypoint != null)
                selectedAgent = selectedWaypoint.ParentAgent;
            else
                selectedAgent = selectedElement as ScenarioAgent;
            //Disable waypoints for ego vehicles
            if (selectedAgent == null || selectedAgent.Source.AgentTypeId == 1)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                speedPanel.SetActive(selectedWaypoint != null);
                waitTimePanel.SetActive(selectedWaypoint != null);
                if (selectedWaypoint != null)
                {
                    speedInput.text = selectedWaypoint.Speed.ToString("F");
                    waitTimeInput.text = selectedWaypoint.WaitTime.ToString("F");
                }

                //Layout rebuild is required after one frame when content changes size
                if (gameObject.activeInHierarchy)
                    StartCoroutine(DelayedLayoutRebuild());
            }
        }

        private IEnumerator DelayedLayoutRebuild()
        {
            yield return null;
            var rectTransform = transform.parent;
            var layoutGroup = rectTransform.GetComponent<VerticalLayoutGroup>();
            while (layoutGroup == null)
            {
                rectTransform = rectTransform.parent;
                if (rectTransform == null)
                    break;
                layoutGroup = rectTransform.GetComponent<VerticalLayoutGroup>();
            }

            if (layoutGroup != null)
            {
                layoutGroup.CalculateLayoutInputVertical();
                layoutGroup.SetLayoutVertical();
            }
        }

        public void Add()
        {
            ScenarioManager.Instance.inputManager.StartAddingElements(this);
        }

        void IAddElementsHandler.AddingStarted(Vector3 dragPosition)
        {
            if (selectedAgent == null)
            {
                Debug.LogWarning("Cannot add waypoints if no agent or waypoint is selected.");
                ScenarioManager.Instance.inputManager.CancelAddingElements(this);
                return;
            }

            isEditing = true;

            var mapWaypointPrefab = ScenarioManager.Instance.waypointsManager.waypointPrefab;
            waypointInstance = ScenarioManager.Instance.prefabsPools.GetInstance(mapWaypointPrefab)
                .GetComponent<ScenarioWaypoint>();
            if (waypointInstance == null)
            {
                Debug.LogWarning("Cannot add waypoints. Add waypoint component to the prefab.");
                ScenarioManager.Instance.inputManager.CancelAddingElements(this);
                ScenarioManager.Instance.prefabsPools.ReturnInstance(waypointInstance.gameObject);
                return;
            }

            waypointInstance.transform.position = dragPosition;
            selectedAgent.AddWaypoint(waypointInstance, selectedWaypoint);
        }

        void IAddElementsHandler.AddingMoved(Vector3 dragPosition)
        {
            waypointInstance.transform.position = dragPosition;
            selectedAgent.WaypointPositionChanged(waypointInstance);
        }

        void IAddElementsHandler.AddElement(Vector3 dragPosition)
        {
            var previousWaypoint = waypointInstance;
            var mapWaypointPrefab = ScenarioManager.Instance.waypointsManager.waypointPrefab;
            waypointInstance = ScenarioManager.Instance.prefabsPools.GetInstance(mapWaypointPrefab)
                .GetComponent<ScenarioWaypoint>();
            waypointInstance.transform.position = dragPosition;
            selectedAgent.AddWaypoint(waypointInstance, previousWaypoint);
        }

        void IAddElementsHandler.AddingCancelled(Vector3 dragPosition)
        {
            if (waypointInstance != null)
                waypointInstance.Destroy();
            waypointInstance = null;
            isEditing = false;
        }

        public void ChangeWaypointSpeed(InputField inputField)
        {
            if (selectedWaypoint != null && float.TryParse(inputField.text, out var value))
                selectedWaypoint.Speed = value;
        }

        public void ChangeWaypointWaitTime(InputField inputField)
        {
            if (selectedWaypoint != null && float.TryParse(inputField.text, out var value))
                selectedWaypoint.WaitTime = value;
        }
    }
}