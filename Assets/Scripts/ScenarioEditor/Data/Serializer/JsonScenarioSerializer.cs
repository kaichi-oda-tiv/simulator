/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Data.Serializer
{
    using Agents;
    using Managers;
    using SimpleJSON;
    using UnityEngine;

    public static class JsonScenarioSerializer
    {
        public static JsonScenario SerializeScenario()
        {
            var scenarioData = new JSONObject();
            var scenarioManager = ScenarioManager.Instance;
            scenarioData.Add("version", new JSONString("0.01"));
            AddMapNode(scenarioData, scenarioManager.MapManager.CurrentMapName);
            var agents = scenarioManager.GetComponentsInChildren<ScenarioAgent>();
            foreach (var agent in agents)
            {
                AddAgentNode(scenarioData, agent);
            }

            return new JsonScenario(scenarioData);
        }

        private static void AddMapNode(JSONObject data, string mapName)
        {
            var map = new JSONObject();
            data.Add("map", map);
            map.Add("name", new JSONString(mapName));
        }

        private static void AddAgentNode(JSONObject data, ScenarioAgent scenarioAgent)
        {
            var agents = data.GetValueOrDefault("agents", new JSONArray());
            if (!data.HasKey("agents"))
                data.Add("agents", agents);
            var agent = new JSONObject();
            agents.Add(agent);
            agent.Add("uid", new JSONString(scenarioAgent.Uid));
            agent.Add("variant", new JSONString(scenarioAgent.Variant.name));
            agent.Add("type", new JSONNumber(scenarioAgent.Source.AgentTypeId));
            var transform = new JSONObject();
            agent.Add("transform", transform);
            var position = new JSONObject().WriteVector3(scenarioAgent.TransformToDrag.position);
            transform.Add("position", position);
            var rotation = new JSONObject().WriteVector3(scenarioAgent.TransformToRotate.rotation.eulerAngles);
            transform.Add("rotation", rotation);
            AddWaypointsNodes(agent, scenarioAgent);
        }

        private static void AddWaypointsNodes(JSONObject data, ScenarioAgent scenarioAgent)
        {
            var waypoints = data.GetValueOrDefault("waypoints", new JSONArray());
            if (!data.HasKey("waypoints"))
                data.Add("waypoints", waypoints);

            var angle = Vector3.zero;
            for (var i = 0; i < scenarioAgent.Waypoints.Count; i++)
            {
                var scenarioWaypoint = scenarioAgent.Waypoints[i];
                var waypoint = new JSONObject();
                var position = new JSONObject().WriteVector3(scenarioWaypoint.transform.position);
                var hasNextWaypoint = i + 1 < scenarioAgent.Waypoints.Count;
                angle = hasNextWaypoint
                    ? Quaternion.LookRotation(scenarioAgent.Waypoints[i + 1].transform.position - position).eulerAngles
                    : angle;
                waypoint.Add("ordinal_number", new JSONNumber(i));
                waypoint.Add("position", position);
                waypoint.Add("angle", angle);
                waypoint.Add("wait_time", new JSONNumber(scenarioWaypoint.WaitTime));
                waypoint.Add("speed", new JSONNumber(scenarioWaypoint.Speed));
                waypoints.Add(waypoint);
            }
        }
    }
}