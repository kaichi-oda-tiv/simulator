/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Managers
{
    using System.Collections.Generic;
    using Elements;
    using UnityEngine;

    public class ScenarioWaypointsManager : MonoBehaviour
    {
        public Material waypointPathMaterial;

        public GameObject waypointPrefab;

        public List<ScenarioWaypoint> Waypoints { get; } = new List<ScenarioWaypoint>();

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
            Waypoints.Clear();
        }

        public void RegisterWaypoint(ScenarioWaypoint agent)
        {
            Waypoints.Add(agent);
        }

        public void UnregisterWaypoint(ScenarioWaypoint agent)
        {
            Waypoints.Remove(agent);
        }
    }
}