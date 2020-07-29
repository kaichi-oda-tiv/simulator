/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Data.Serializer
{
    using System.Text;
    using Agents;
    using Managers;
    using UnityEngine;

    public static class PythonScenarioSerializer
    {
        private static int IndentLevel = 0;

        public static PythonScenario SerializeScenario()
        {
            var stringBuilder = new StringBuilder();
            var scenarioManager = ScenarioManager.Instance;
            AppendSimulationInit(stringBuilder);
            AppendScenarioLoadMap(stringBuilder, scenarioManager.MapManager.CurrentMapName);
            var agents = scenarioManager.GetComponentsInChildren<ScenarioAgent>();
            foreach (var agent in agents)
            {
                switch (agent.Source.AgentTypeId)
                {
                    //Ego
                    case 1:
                        AppendScenarioAddAgent(stringBuilder, agent, "EGO");
                        break;
                    //NPC
                    case 2:
                        AppendScenarioAddAgent(stringBuilder, agent, "NPC");
                        AppendScenarioWaypoints(stringBuilder, agent);
                        break;
                    //Pedestrian
                    case 3:
                        AppendScenarioAddAgent(stringBuilder, agent, "PEDESTRIAN");
                        AppendScenarioWaypoints(stringBuilder, agent);
                        break;
                }
            }

            AppendSimulationStart(stringBuilder);

            return new PythonScenario(stringBuilder.ToString());
        }

        private static void AppendLine(StringBuilder stringBuilder, string line)
        {
            for (int i = 0; i < IndentLevel; i++)
                stringBuilder.Append("	");
            stringBuilder.Append(line);
            stringBuilder.Append("\n");
        }

        private static void AppendSimulationInit(StringBuilder stringBuilder)
        {
            AppendLine(stringBuilder, "#!/usr/bin/env python3");
            AppendLine(stringBuilder, "#");
            AppendLine(stringBuilder, "# Copyright (c) 2019 LG Electronics, Inc.");
            AppendLine(stringBuilder, "#");
            AppendLine(stringBuilder, "# This software contains code licensed as described in LICENSE.");
            AppendLine(stringBuilder, "#");
            AppendLine(stringBuilder, "");
            AppendLine(stringBuilder, "import os");
            AppendLine(stringBuilder, "import lgsvl");
            AppendLine(stringBuilder, "");
            AppendLine(stringBuilder,
                "sim = lgsvl.Simulator(os.environ.get(\"SIMULATOR_HOST\", \"127.0.0.1\"), 8181);");
        }

        private static void AppendSimulationStart(StringBuilder stringBuilder)
        {
            AppendLine(stringBuilder, "sim.run()");
        }

        private static void AppendScenarioLoadMap(StringBuilder stringBuilder, string mapName)
        {
            AppendLine(stringBuilder, $"if sim.current_scene == \"{mapName}\":");
            IndentLevel++;
            AppendLine(stringBuilder, "sim.reset();");
            IndentLevel--;
            AppendLine(stringBuilder, "else:");
            IndentLevel++;
            AppendLine(stringBuilder, $"sim.load(\"{mapName}\")");
            IndentLevel--;
            AppendLine(stringBuilder, "");
        }

        private static void AppendScenarioAddAgent(StringBuilder stringBuilder, ScenarioAgent agent, string agentType)
        {
            var position = agent.TransformToDrag.position;
            var rotation = agent.TransformToRotate.rotation.eulerAngles;
            AppendLine(stringBuilder, "state = lgsvl.AgentState()");
            AppendLine(stringBuilder, $"state.transform.position = lgsvl.Vector{position}");
            AppendLine(stringBuilder, $"state.transform.rotation = lgsvl.Vector{rotation}");
            AppendLine(stringBuilder,
                $"agent = sim.add_agent(\"{agent.Variant.name}\", lgsvl.AgentType.{agentType}, state)");
            AppendLine(stringBuilder, "");
        }

        private static void AppendScenarioWaypoints(StringBuilder stringBuilder, ScenarioAgent agent)
        {
            AppendLine(stringBuilder, "waypoints = []");
            var angle = Vector3.zero;
            for (var i = 0; i < agent.Waypoints.Count; i++)
            {
                var waypoint = agent.Waypoints[i];
                var hasNextWaypoint = i + 1 < agent.Waypoints.Count;
                var nextWaypointPosition = hasNextWaypoint
                    ? agent.Waypoints[i + 1].transform.position
                    : Vector3.zero;
                var position = waypoint.transform.position;
                angle = hasNextWaypoint
                    ? Quaternion.LookRotation(nextWaypointPosition - position).eulerAngles
                    : angle;
                AppendLine(stringBuilder,
                    $"wp = lgsvl.DriveWaypoint(lgsvl.Vector{position}, {waypoint.Speed}, lgsvl.Vector{angle}, {waypoint.WaitTime})");
                AppendLine(stringBuilder, "waypoints.append(wp)");
            }

            AppendLine(stringBuilder, "agent.follow(waypoints)");
            AppendLine(stringBuilder, "");
        }
    }
}