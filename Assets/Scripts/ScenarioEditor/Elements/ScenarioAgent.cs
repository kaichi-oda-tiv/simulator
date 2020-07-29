/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Agents
{
    using System.Collections.Generic;
    using Elements;
    using Managers;
    using UnityEngine;

    public class ScenarioAgent : ScenarioElement
    {
        private static Vector3 lineRendererPositionOffset = new Vector3(0.0f, 0.5f, 0.0f);

        private ScenarioAgentSource source;

        private AgentVariant variant;

        private GameObject modelInstance;

        private LineRenderer lineRenderer;

        private Transform waypointsParent;

        private List<ScenarioWaypoint> waypoints = new List<ScenarioWaypoint>();

        public Transform WaypointsParent
        {
            get
            {
                if (waypointsParent == null)
                {
                    var newGameObject = new GameObject("Waypoints");
                    waypointsParent = newGameObject.transform;
                    waypointsParent.SetParent(transform);
                    waypointsParent.localPosition = Vector3.zero;
                    lineRenderer = newGameObject.AddComponent<LineRenderer>();
                    lineRenderer.material = ScenarioManager.Instance.waypointsManager.waypointPathMaterial;
                    lineRenderer.useWorldSpace = false;
                    lineRenderer.positionCount = 1;
                    lineRenderer.SetPosition(0, lineRendererPositionOffset);
                    lineRenderer.sortingLayerName = "Ignore Raycast";
                    lineRenderer.widthMultiplier = 0.2f;
                }

                return waypointsParent;
            }
        }

        public override Transform TransformToRotate => modelInstance.transform;

        public ScenarioAgentSource Source => source;

        public AgentVariant Variant => variant;

        public List<ScenarioWaypoint> Waypoints => waypoints;

        public void Setup(ScenarioAgentSource agentSource, AgentVariant agentVariant)
        {
            source = agentSource;
            ChangeVariant(agentVariant);
            ScenarioManager.Instance.agentsManager.RegisterAgent(this);
        }

        public void ChangeVariant(AgentVariant newVariant)
        {
            var position = Vector3.zero;
            var rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            if (modelInstance != null)
            {
                position = modelInstance.transform.localPosition;
                rotation = modelInstance.transform.localRotation;
                source.ReturnModelInstance(modelInstance);
            }

            variant = newVariant;
            modelInstance = source.GetModelInstance(variant);
            modelInstance.transform.SetParent(transform);
            modelInstance.transform.localPosition = position;
            modelInstance.transform.localRotation = rotation;
        }

        public override void Selected()
        {
        }

        public override void Destroy()
        {
            if (modelInstance != null)
                source.ReturnModelInstance(modelInstance);
            for (var i = waypoints.Count - 1; i >= 0; i--) waypoints[i].Destroy();

            ScenarioManager.Instance.agentsManager.UnregisterAgent(this);
            Destroy(gameObject);
        }

        public void AddWaypoint(ScenarioWaypoint waypoint, ScenarioWaypoint previousWaypoint = null)
        {
            var index = previousWaypoint == null ? waypoints.Count : waypoints.IndexOf(previousWaypoint) + 1;
            waypoints.Insert(index, waypoint);
            waypoint.ParentAgent = this;
            var waypointTransform = waypoint.transform;
            waypointTransform.SetParent(WaypointsParent);
            lineRenderer.positionCount = waypoints.Count + 1;
            for (var i = index; i < waypoints.Count; i++)
            {
                var position = lineRendererPositionOffset + waypoints[i].transform.localPosition;
                lineRenderer.SetPosition(i + 1, position);
            }
        }

        public void AddWaypoint(ScenarioWaypoint waypoint, int index)
        {
            if (index > waypoints.Count)
                index = waypoints.Count;
            waypoints.Insert(index, waypoint);
            waypoint.ParentAgent = this;
            var waypointTransform = waypoint.transform;
            waypointTransform.SetParent(WaypointsParent);
            lineRenderer.positionCount = waypoints.Count + 1;
            for (var i = index; i < waypoints.Count; i++)
            {
                var position = lineRendererPositionOffset + waypoints[i].transform.localPosition;
                lineRenderer.SetPosition(i + 1, position);
            }
        }

        public void RemoveWaypoint(ScenarioWaypoint waypoint)
        {
            var index = waypoints.IndexOf(waypoint);
            waypoints.Remove(waypoint);
            for (var i = index; i < waypoints.Count; i++)
            {
                var position = lineRendererPositionOffset + waypoints[i].transform.transform.localPosition;
                lineRenderer.SetPosition(i + 1, position);
            }

            lineRenderer.positionCount = waypoints.Count + 1;
        }

        public void WaypointPositionChanged(ScenarioWaypoint waypoint)
        {
            var index = waypoints.IndexOf(waypoint);
            var position = lineRendererPositionOffset + waypoint.transform.transform.localPosition;
            lineRenderer.SetPosition(index + 1, position);
        }
    }
}