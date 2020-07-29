/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using Simulator.Bridge;
using Simulator.Sensors.UI;
using Simulator.Utilities;
using System.Collections;
using UnityEngine;

namespace Simulator.Sensors
{
    [SensorType("Analysis", new System.Type[] { })]
    public class AnalysisSensor : SensorBase
    {
        private AgentController AgentController;
        private Rigidbody RB;
        private IVehicleDynamics Dynamics;
        private VehicleActions Actions;

        private float Distance = 0f;
        private Vector3 PrevPos = new Vector3(0f, 0f, 0f);

        private float SpeedMin = float.MaxValue;
        private float SpeedMax = 0f;
        private float SpeedAvg = 0f;
        private float SpeedTotal = 0f;
        private float PrevSpeed = 0f;
        private int SpeedCount = 0;
        private float Speed = 0f;

        private Vector3 Acceleration = new Vector3(0f, 0f, 0f);
        private Vector3 LastLocalAcceleration = new Vector3(0f, 0f, 0f);
        private Vector3 Velocity = new Vector3(0f, 0f, 0f);
        private Vector3 LastLocalVelocity = new Vector3(0f, 0f, 0f);
        private Quaternion Rotation = Quaternion.identity;
        private float AngularVelocity = 0f;
        private float AngularAcceleration = 0f;
        private float Slip = 0f;
        private float AccelLongMin = float.MaxValue;
        private float AccelLongMax = 0f;
        private float AccelLatMin = float.MaxValue;
        private float AccelLatMax = 0f;

        private Vector3 Jerk = new Vector3(0f, 0f, 0f);
        private float JerkLongMin = float.MaxValue;
        private float JerkLongMax = 0f;
        private float JerkLatMin = float.MaxValue;
        private float JerkLatMax = 0f;

        private float SteerAngleMax = 0f;
        private float SteerAngle = 0f;
        private float PrevSteerAngle = 0f;

        private void Awake()
        {
            AgentController = GetComponentInParent<AgentController>();
            Dynamics = GetComponentInParent<IVehicleDynamics>();
            Actions = GetComponentInParent<VehicleActions>();
            RB = GetComponentInParent<Rigidbody>();
        }

        public override void OnBridgeSetup(IBridge bridge)
        {
            //
        }

        public override void OnVisualize(Visualizer visualizer)
        {
            //
        }

        public override void OnVisualizeToggle(bool state)
        {
            //
        }

        public override void OnAnalyze()
        {
            CalculateAnalysisValues();
        }

        public override void SetAnalysisData()
        {
            SensorAnalysisData = new Hashtable
            {
                { "Distance", Distance },
                { "SpeedMin", SpeedMin },
                { "SpeedMax", SpeedMax },
                { "SpeedAvg", SpeedAvg },
                { "AccelLongMin", AccelLongMin },
                { "AccelLongMax", AccelLongMax },
                { "AccelLatMin", AccelLatMin },
                { "AccelLatMax", AccelLatMax },
                { "JerkLongMin", JerkLongMin },
                { "JerkLongMax", JerkLongMax },
                { "JerkLatMin", JerkLatMin },
                { "JerkLatMax", JerkLatMax },
                { "SteerAngleMax", SteerAngleMax },
            };
        }

        private void CalculateAnalysisValues()
        {
            Distance += Vector3.Distance(transform.position, PrevPos) / 1000;
            PrevPos = transform.position;

            Vector3 posDelta = RB.velocity;
            Vector3 velocityDelta = (posDelta - Velocity) / Time.fixedDeltaTime;
            Vector3 accelDelta = (velocityDelta - Acceleration) / Time.fixedDeltaTime;
            Speed = RB.velocity.magnitude;
            Jerk = accelDelta;
            Acceleration = velocityDelta;
            Velocity = posDelta;

            float angleDelta = Quaternion.Angle(Rotation, transform.rotation) / Time.fixedDeltaTime;
            float angularVelocityDelta = (angleDelta - AngularVelocity) / Time.fixedDeltaTime;
            AngularAcceleration = angularVelocityDelta;
            AngularVelocity = angleDelta;
            Rotation = transform.rotation;
            Slip = Vector3.Angle(RB.velocity, transform.forward);
            SteerAngle = Dynamics.WheelAngle;

            // Compute local acceleration and jerk
            var localVelocity = transform.InverseTransformDirection(RB.velocity);
            localVelocity.Set(localVelocity.z, -localVelocity.x, localVelocity.y);
            var localAcceleration = (localVelocity - LastLocalVelocity) / Time.fixedDeltaTime;
            LastLocalVelocity = localVelocity;
            var localGravity = transform.InverseTransformDirection(Physics.gravity);
            localAcceleration -= new Vector3(localGravity.z, -localGravity.x, localGravity.y);

            var localJerk = (localAcceleration - LastLocalAcceleration) / Time.fixedDeltaTime;
            LastLocalAcceleration = localAcceleration;

            float AccelLong = localAcceleration.x;
            float AccelLat = localAcceleration.y;
            UpdateMinMax(AccelLong, ref AccelLongMin, ref AccelLongMax);
            UpdateMinMax(AccelLat, ref AccelLatMin, ref AccelLatMax);

            float JerkLong = localJerk.x;
            float JerkLat = localJerk.y;
            UpdateMinMax(JerkLong, ref JerkLongMin, ref JerkLongMax);
            UpdateMinMax(JerkLat, ref JerkLatMin, ref JerkLatMax);

            // min max avg speed with TODO hack for SuddenBrake
            SpeedTotal += Speed;
            SpeedCount++;
            SpeedAvg = SpeedTotal / SpeedCount;
            UpdateMinMax(Speed, ref SpeedMin, ref SpeedMax);

            if (Mathf.Abs(PrevSpeed - Speed) > 10)
            {
                SuddenBrakeEvent(AgentController.GTID);
            }
            PrevSpeed = Speed;

            // steer angle max and TODO hack sudden steer
            if (SteerAngle > SteerAngleMax)
            {
                SteerAngleMax = SteerAngle;
            }
            if (Mathf.Abs(PrevSteerAngle - SteerAngle) > 10)
            {
                SuddenSteerEvent(AgentController.GTID);
            }
            PrevSteerAngle = SteerAngle;
        }

        private void UpdateMinMax(float value, ref float min, ref float max)
        {
            if (value < min)
            {
                min = value;
            }

            if (value > max)
            {
                max = value;
            }
        }

        private void SuddenBrakeEvent(uint id)
        {
            Hashtable data = new Hashtable
            {
                { "Id", id },
                { "Type", "SuddenBrake" },
                { "SuddenBrake", true },
            };
            SimulatorManager.Instance.AnalysisManager.AddEvent(data);
        }

        private void SuddenSteerEvent(uint id)
        {
            Hashtable data = new Hashtable
            {
                { "Id", id },
                { "Type", "SuddenSteer" },
                { "SuddenSteer", true },
            };
            SimulatorManager.Instance.AnalysisManager.AddEvent(data);
        }

        private void TrafficViolationEvent(uint id)
        {
            Hashtable data = new Hashtable
            {
                { "Id", id },
                { "Type", "TrafficViolation" },
                { "TrafficViolation", true },
            };
            SimulatorManager.Instance.AnalysisManager.AddEvent(data);
        }

        private void StopLineViolationEvent(uint id)
        {
            Hashtable data = new Hashtable
            {
                { "Id", id },
                { "Type", "StopLineViolation" },
                { "StopLineViolation", true },
            };
            SimulatorManager.Instance.AnalysisManager.AddEvent(data);
        }

        private void SpeedViolationEvent(uint id)
        {
            Hashtable data = new Hashtable
            {
                { "Id", id },
                { "Type", "SpeedViolation" },
                { "SpeedViolation", true },
            };
            SimulatorManager.Instance.AnalysisManager.AddEvent(data);
        }

        private void LaneViolationEvent(uint id)
        {
            Hashtable data = new Hashtable
            {
                { "Id", id },
                { "Type", "LaneViolation" },
                { "LaneViolation", true },
            };
            SimulatorManager.Instance.AnalysisManager.AddEvent(data);
        }
    }
}
