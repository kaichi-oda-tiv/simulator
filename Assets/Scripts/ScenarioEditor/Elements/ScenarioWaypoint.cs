/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Elements
{
    using Agents;
    using Managers;

    public class ScenarioWaypoint : ScenarioElement
    {
        private ScenarioAgent parentAgent;

        public ScenarioAgent ParentAgent
        {
            get => parentAgent;
            set => parentAgent = value;
        }

        public float Speed { get; set; } = 6.0f;

        public float WaitTime { get; set; }

        private void OnEnable()
        {
            ScenarioManager.Instance.waypointsManager.RegisterWaypoint(this);
        }

        private void OnDisable()
        {
            ScenarioManager.Instance.waypointsManager.UnregisterWaypoint(this);
        }

        public override void Selected()
        {
        }

        public override void Destroy()
        {
            ParentAgent.RemoveWaypoint(this);
            ScenarioManager.Instance.prefabsPools.ReturnInstance(gameObject);
        }

        protected override void OnDragged()
        {
            base.OnDragged();
            parentAgent.WaypointPositionChanged(this);
        }
    }
}