/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

﻿namespace Simulator.Bridge.Ros2.Autoware
{
    [MessageType("autoware_auto_msgs/VehicleControlCommand")]
    public struct VehicleControlCommand
    {
        public Time stamp;
        public float long_accel_mps2;
        public float front_wheel_angle_rad;
        public double rear_wheel_angle_rad;
    }

    [MessageType("autoware_auto_msgs/VehicleStateCommand")]
    public struct VehicleStateCommand
    {
        public Time stamp;
        public byte blinker;
        public byte headlight;
        public byte wiper;
        public byte gear;
        public byte mode;
        public bool hand_brake;
        public bool horn;
    }

    [MessageType("autoware_auto_msgs/VehicleStateReport")]
    public struct VehicleStateReport
    {
        public Time stamp;
        public byte fuel;
        public byte blinker;
        public byte headlight;
        public byte wiper;
        public byte gear;
        public byte mode;
        public bool hand_brake;
        public bool horn;
    }

    [MessageType("autoware_auto_msgs/VehicleOdometry")]
    public struct VehicleOdometry
    {
        public Time stamp;
        public float velocity_mps;
        public float front_wheel_angle_rad;
        public float rear_wheel_angle_rad;
    }

    [MessageType("autoware_auto_msgs/RawControlCommand")]
    public struct RawControlCommand
    {
        public Time stamp;
        public uint throttle;
        public uint brake;
        public int front_steer;
        public int rear_steer;
    }
}
