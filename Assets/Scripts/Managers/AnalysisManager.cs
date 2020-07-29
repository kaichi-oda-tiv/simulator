/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Simulator.Sensors;
using System.Collections;
using Simulator.Database.Services;

namespace Simulator.Analysis
{
    public class AnalysisManager : MonoBehaviour
    {
        public enum AnalysisStatusType
        {
            InProgress,
            Success,
            Failed,
            Error,
        };
        private AnalysisStatusType Status = AnalysisStatusType.Error;

        private JsonSerializerSettings SerializerSettings;
        private SimulationConfig SimConfig;
        private List<SensorBase> Sensors = new List<SensorBase>();
        private List<Hashtable> AnalysisEvents = new List<Hashtable>();
        private JArray Results = new JArray();

        private bool Init = false;

        public struct CollisionTotalData
        {
            public int Ego;
            public int Npc;
            public int Ped;
        }
        private CollisionTotalData CollisionTotals;
        private DateTime AnalysisStart;

        private void Awake()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
        }

        private void Start()
        {
            if (!SimulatorManager.Instance.IsAPI)
            {
                AnalysisInit();
            }
        }

        private void OnDestroy()
        {
            AnalysisSave();
            AnalysisSend();
        }

        private void FixedUpdate()
        {
            if (SimConfig == null || !SimConfig.CurrentTestId.HasValue)
            {
                return; // Development mode or generate report false
            }

            foreach (var sensor in Sensors)
            {
                sensor.OnAnalyze();
            }
        }

        public void AnalysisInit()
        {
            if (Init)
            {
                return;
            }

            Sensors.Clear();
            AnalysisEvents.Clear();
            CollisionTotals.Ego = 0;
            CollisionTotals.Npc = 0;
            CollisionTotals.Ped = 0;
            Status = AnalysisStatusType.InProgress;
            AnalysisStart = DateTime.Now;

            SimConfig = Loader.Instance?.SimConfig;
            if (SimConfig == null || !SimConfig.CurrentTestId.HasValue)
            {
                return; // Development mode or generate report false
            }

            if (SimulatorManager.Instance.IsAPI)
            {
                SimConfig.Agents = SimulatorManager.Instance.AgentManager.ActiveAgents.ToArray();
            }

            foreach (var agent in SimConfig.Agents)
            {
                Array.ForEach(agent.AgentGO.GetComponentsInChildren<SensorBase>(), sensorBase =>
                {
                    Sensors.Add(sensorBase);

                    if (sensorBase is VideoRecordingSensor recorder)
                    {
                        recorder.StartRecording();
                    }
                });
            }
            Init = true;
        }

        public void AnalysisSave()
        {
            if (!Init)
            {
                return;
            }

            SimConfig = Loader.Instance?.SimConfig;
            if (SimConfig == null || !SimConfig.CurrentTestId.HasValue)
            {
                return; // Development mode or generate report false
            }

            if (Status != AnalysisStatusType.InProgress)
            {
                return;
            }

            SetStatus();

            var simulationConfigJS = JsonConvert.SerializeObject(SimConfig, SerializerSettings);
            var configJO = JObject.Parse(simulationConfigJS);
            var agents = (JArray)configJO["Agents"];
            foreach (var agent in agents)
            {
                var agentObj = agent as JObject;
                JToken token = JToken.Parse(agent["Sensors"].ToString());
                agentObj["Sensors"] = token;
            }

            JObject resultsJO = new JObject();
            JArray agentsJA = new JArray();
            foreach (var agent in SimConfig.Agents)
            {
                JObject agentJO = new JObject();
                agentJO.Add("Name", agent.Name);
                JObject sensorsJO = new JObject();
                Array.ForEach(agent.AgentGO.GetComponentsInChildren<SensorBase>(), sensorBase =>
                {
                    sensorBase.SetAnalysisData();
                    if (sensorBase.SensorAnalysisData != null)
                    {
                        var sData = JsonConvert.SerializeObject(sensorBase.SensorAnalysisData, SerializerSettings);
                        JToken token = JToken.Parse(sData);
                        sensorsJO.Add(sensorBase.GetType().ToString(), token);
                    }

                    if (sensorBase is VideoRecordingSensor recorder)
                    {
                        if (recorder.StopRecording())
                        {
                            agentJO.Add("VideoCapture", recorder.GetFileName());
                        }
                    }
                });
                agentJO.Add("Sensors", sensorsJO);

                JArray eventsJA = new JArray();
                foreach (var hashtable in AnalysisEvents)
                {
                    if (hashtable.ContainsKey("Id"))
                    {
                        if ((uint)hashtable["Id"] == agent.GTID)
                        {
                            var sData = JsonConvert.SerializeObject(hashtable, SerializerSettings);
                            JToken token = JToken.Parse(sData);
                            eventsJA.Add(token);
                        }
                    }
                }
                agentJO.Add("Events", eventsJA);
                agentsJA.Add(agentJO);
            }

            resultsJO.Add("Status", Status.ToString());
            resultsJO.Add("StartTime", AnalysisStart);
            resultsJO.Add("StopTime", DateTime.Now);
            resultsJO.Add("Duration", SimulatorManager.Instance.GetSessionElapsedTime());
            resultsJO.Add("Agents", agentsJA);
            resultsJO.Add("EgoTotal", SimConfig.Agents.Length);
            resultsJO.Add("CollisionTotals", JToken.Parse(JsonConvert.SerializeObject(CollisionTotals, SerializerSettings)));
            // TODO api callbacks
            configJO.Add("Results", resultsJO);
            Results.Add(configJO);
            Init = false;
        }

        private void AnalysisSend()
        {
            if (SimConfig == null || !SimConfig.CurrentTestId.HasValue)
            {
                return; // Development mode or generate report false
            }

            if (Status == AnalysisStatusType.InProgress)
            {
                return;
            }

            ITestResultService testResultService = new TestResultService();
            testResultService.CompleteTest(SimConfig.CurrentTestId.Value, Status == AnalysisStatusType.Success, Results.Count, Results.ToString(Formatting.None));
        }

        public void AddEvent(Hashtable data)
        {
            AnalysisEvents.Add(data);
        }

        public void IncrementEgoCollision(uint id, Vector3 location, Vector3 egoVelocity, Vector3 otherVelocity, string otherType)
        {
            CollisionTotals.Ego++;
            var data = new Hashtable
            {
                { "Id", id },
                { "Type", "EgoCollision" },
                { "Time", SimulatorManager.Instance.GetSessionElapsedTime() },
                { "Location", location },
                { "OtherType", otherType },
                { "EgoVelocity", egoVelocity },
                { "OtherVelocity", otherVelocity },
                { "EgoCollisionTotal", CollisionTotals.Ego },
                { "Status", AnalysisStatusType.Failed },
            };
            AddEvent(data);
        }

        public void IncrementNPCCollision()
        {
            CollisionTotals.Npc++;
        }

        public void IncrementPedCollision()
        {
            CollisionTotals.Ped++;
        }

        public void AddErrorEvent(string error)
        {
            var data = new Hashtable
            {
                { "Error", error},
                { "Status", AnalysisStatusType.Error },
            };
            AddEvent(data);
        }

        private void SetStatus()
        {
            Status = AnalysisStatusType.Success;
            foreach (var hashtable in AnalysisEvents)
            {
                if (hashtable.ContainsValue(AnalysisStatusType.Failed))
                {
                    Status = AnalysisStatusType.Failed;
                }

                // Error always overwrites test case fail
                if (hashtable.ContainsValue(AnalysisStatusType.Error))
                {
                    Status = AnalysisStatusType.Error;
                    break;
                }
            }

            // cycle every status to convert to string for json
            foreach (var hashtable in AnalysisEvents)
            {
                if (hashtable.ContainsKey("Status"))
                {
                    hashtable["Status"] = hashtable["Status"].ToString();
                }
            }
        }
    }
}
