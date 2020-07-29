/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Agents
{
    using System.Collections.Generic;
    using Managers;
    using UnityEngine;

    public class ScenarioPedestrianAgentSource : ScenarioAgentSource
    {
        public override string AgentTypeName => "PedestrianAgent";

        public override int AgentTypeId => 3;

        public override List<AgentVariant> AgentVariants { get; } = new List<AgentVariant>();

        private GameObject draggedInstance;

        public override void Initialize()
        {
            var pedestrianManager = Loader.Instance.SimulatorManagerPrefab.pedestrianManagerPrefab;
            var pedestriansInSimulation = pedestrianManager.pedModels;
            for (var i = 0; i < pedestriansInSimulation.Count; i++)
            {
                var pedestrian = pedestriansInSimulation[i];
                var egoAgent = new AgentVariant()
                {
                    source = this,
                    name = pedestrian.name,
                    prefab = pedestrian
                };
                AgentVariants.Add(egoAgent);
            }
        }

        public override void Deinitialize()
        {
        }

        public override GameObject GetModelInstance(AgentVariant variant)
        {
            var instance = ScenarioManager.Instance.prefabsPools.GetInstance(variant.prefab);
            if (instance.GetComponent<BoxCollider>() == null)
            {
                var collider = instance.AddComponent<BoxCollider>();
                var b = new Bounds(instance.transform.position, Vector3.zero);
                foreach (Renderer r in instance.GetComponentsInChildren<Renderer>())
                    b.Encapsulate(r.bounds);
                collider.center = b.center - instance.transform.position;
                collider.size = b.size;
            }
            
            if (instance.GetComponent<Rigidbody>() == null)
            {
                var rigidbody = instance.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidbody.isKinematic = true;
            }

            return instance;
        }

        public override ScenarioAgent GetAgentInstance(AgentVariant variant)
        {
            var newGameObject = new GameObject(AgentTypeName);
            newGameObject.transform.SetParent(ScenarioManager.Instance.transform);
            var scenarioAgent = newGameObject.AddComponent<ScenarioAgent>();
            scenarioAgent.Setup(this, variant);
            return scenarioAgent;
        }

        public override void ReturnModelInstance(GameObject instance)
        {
            ScenarioManager.Instance.prefabsPools.ReturnInstance(instance);
        }

        public override void DragNewAgent()
        {
            ScenarioManager.Instance.inputManager.StartDraggingElement(this);
        }

        public override void DragStarted(Vector3 dragPosition)
        {
            draggedInstance = ScenarioManager.Instance.prefabsPools.GetInstance(AgentVariants[0].prefab);
            draggedInstance.transform.SetParent(ScenarioManager.Instance.transform);
            draggedInstance.transform.SetPositionAndRotation(dragPosition, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }

        public override void DragMoved(Vector3 dragPosition)
        {
            draggedInstance.transform.position = dragPosition;
        }

        public override void DragFinished(Vector3 dragPosition)
        {
            var agent = GetAgentInstance(AgentVariants[0]);
            agent.transform.SetPositionAndRotation(draggedInstance.transform.position,
                draggedInstance.transform.rotation);
            ScenarioManager.Instance.prefabsPools.ReturnInstance(draggedInstance);
            draggedInstance = null;
        }

        public override void DragCancelled(Vector3 dragPosition)
        {
            ScenarioManager.Instance.prefabsPools.ReturnInstance(draggedInstance);
            draggedInstance = null;
        }
    }
}