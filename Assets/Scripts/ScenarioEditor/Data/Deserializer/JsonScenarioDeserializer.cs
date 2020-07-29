/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Data.Deserializer
{
    using Agents;
    using Elements;
    using Managers;
    using Network.Core;
    using SimpleJSON;
    using UnityEngine;

    public static class JsonScenarioDeserializer
    {
        public static void DeserializeScenario(JSONNode json)
        {
            if (!DeserializeMap(json))
                return;
            DeserializeAgents(json);
        }

        private static bool DeserializeMap(JSONNode data)
        {
            var map = data["map"];
            if (map == null)
                return false;
            var mapName = map["name"];
            if (mapName == null)
                return false;
            if (ScenarioManager.Instance.MapManager.CurrentMapName != mapName)
            {
                var mapManager = ScenarioManager.Instance.MapManager;
                if (mapManager.MapExists(mapName))
                    mapManager.LoadMap(mapName, (loadedMapName) => { DeserializeScenario(data); });
                else
                    Log.Error($"Loaded scenario requires map {mapName} which is not available in the database.");
                return false;
            }

            return true;
        }

        private static void DeserializeAgents(JSONNode data)
        {
            var agents = data["agents"] as JSONArray;
            if (agents == null)
                return;
            foreach (var agentNode in agents.Children)
            {
                var agentType = agentNode["type"];
                var agentSource =
                    ScenarioManager.Instance.agentsManager.Sources.Find(source => source.AgentTypeId == agentType);
                if (agentSource == null)
                {
                    Log.Error(
                        $"Error while deserializing Scenario. Agent type '{agentType}' could not be found in Simulator.");
                    continue;
                }

                var variantName = agentNode["variant"];
                var variant = agentSource.AgentVariants.Find(sourceVariant => sourceVariant.name == variantName);
                if (variant == null)
                {
                    Log.Error(
                        $"Error while deserializing Scenario. Agent variant '{variantName}' could not be found in Simulator.");
                    continue;
                }

                var agentInstance = agentSource.GetAgentInstance(variant);
                agentInstance.Uid = agentNode["uid"];
                var transformNode = agentNode["transform"];
                agentInstance.transform.position = transformNode["position"].ReadVector3();
                agentInstance.transform.rotation = Quaternion.Euler(transformNode["rotation"].ReadVector3());

                DeserializeWaypoints(agentNode, agentInstance);
            }
        }

        private static void DeserializeWaypoints(JSONNode data, ScenarioAgent scenarioAgent)
        {
            var waypoints = data["waypoints"] as JSONArray;
            if (waypoints == null)
                return;

            foreach (var waypointNode in waypoints.Children)
            {
                var mapWaypointPrefab = ScenarioManager.Instance.waypointsManager.waypointPrefab;
                var waypointInstance = ScenarioManager.Instance.prefabsPools.GetInstance(mapWaypointPrefab)
                    .GetComponent<ScenarioWaypoint>();
                waypointInstance.transform.position = waypointNode["position"].ReadVector3();
                waypointInstance.WaitTime = waypointNode["wait_time"];
                waypointInstance.Speed = waypointNode["speed"];
                int index = waypointNode["ordinal_number"];
                //TODO sort waypoints
                scenarioAgent.AddWaypoint(waypointInstance, index);
            }
        }
    }
}