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

    public static class ApiScenarioSerializer
    {
        public static ApiScenario SerializeScenario()
        {
            var scenarioData = new JSONArray();
            var scenarioManager = ScenarioManager.Instance;
            scenarioData.Add(ScenarioLoadMapCommand(scenarioManager.MapManager.CurrentMapName));
            var agents = scenarioManager.GetComponentsInChildren<ScenarioAgent>();
            foreach (var agent in agents)
            {
                scenarioData.Add(ScenarioAddAgentCommand(agent));
                scenarioData.Add(ScenarioWaypointsCommand(agent));
            }

            return new ApiScenario(scenarioData);
        }

        private static JSONNode ScenarioLoadMapCommand(string mapName)
        {
            var data = new JSONObject();
            data.Add("command", new JSONString("simulator/load_scene"));
            var arguments = new JSONObject();
            data.Add("arguments", arguments);
            arguments.Add("scene", new JSONString(mapName));
            return data;
        }

        private static JSONNode ScenarioAddAgentCommand(ScenarioAgent agent)
        {
            var data = new JSONObject();
            data.Add("command", new JSONString("simulator/add_agent"));
            var arguments = new JSONObject();
            data.Add("arguments", arguments);
            arguments.Add("uid", new JSONString(agent.Uid));
            arguments.Add("name", new JSONString(agent.Variant.name));
            arguments.Add("type", new JSONNumber(agent.Source.AgentTypeId));
            var state = new JSONObject();
            arguments.Add("state", state);
            var transform = new JSONObject();
            state.Add("transform", transform);
            var agentPosition = agent.transform.position;
            var position = new JSONObject().WriteVector3(agentPosition);
            transform.Add("position", position);
            var rotation = new JSONObject().WriteVector3(agent.transform.rotation.eulerAngles);
            transform.Add("rotation", rotation);
            var direction = agent.Waypoints.Count == 0
                ? Vector3.zero
                : (agent.Waypoints[0].transform.position - agentPosition).normalized;
            var velocity = new JSONObject().WriteVector3(direction);
            state.Add("velocity", velocity);
            return data;
        }

        private static JSONNode ScenarioWaypointsCommand(ScenarioAgent agent)
        {
            var data = new JSONObject();
            switch (agent.Source.AgentTypeId)
            {
                case 2:
                    //NPC
                    data.Add("command", new JSONString("vehicle/follow_waypoints"));
                    break;
                case 3:
                    //Pedestrian
                    data.Add("command", new JSONString("pedestrian/follow_waypoints"));
                    break;
            }

            var arguments = new JSONObject();
            data.Add("arguments", arguments);
            arguments.Add("uid", new JSONString(agent.Uid));
            arguments.Add("loop", new JSONBool(false));

            var waypoints = new JSONArray();
            arguments.Add("waypoints", waypoints);
            for (var i = 0; i < agent.Waypoints.Count; i++)
            {
                var scenarioWaypoint = agent.Waypoints[i];
                var waypoint = new JSONObject();
                var position = new JSONObject().WriteVector3(scenarioWaypoint.transform.position);
                waypoint.Add("position", position);
                waypoint.Add("idle", new JSONNumber(scenarioWaypoint.WaitTime));
                waypoint.Add("trigger_distance", new JSONNumber(0.0f));
                //NPC
                if (agent.Source.AgentTypeId == 2)
                {
                    waypoint.Add("deactivate", new JSONBool(false));
                    waypoint.Add("speed", new JSONNumber(scenarioWaypoint.Speed));
                    var hasNextWaypoint = i + 1 < agent.Waypoints.Count;
                    var nextWaypointPosition = hasNextWaypoint
                        ? agent.Waypoints[i + 1].transform.position
                        : Vector3.zero;
                    var angle = new JSONArray().WriteVector3(hasNextWaypoint
                        ? Quaternion.LookRotation(nextWaypointPosition - scenarioWaypoint.transform.position)
                            .eulerAngles
                        : Vector3.zero);
                    waypoint.Add("angle", angle);
                }

                waypoints.Add(waypoint);
            }

            return data;
        }
    }
}