/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System;
using System.Linq;
using Simulator.Bridge.Data;
using Simulator.Bridge.Ros2.LGSVL;
using Simulator.Bridge.Ros2.Autoware;
using Unity.Mathematics;

namespace Simulator.Bridge.Ros2
{
    static class Conversions
    {
        static readonly DateTime GpsEpoch = new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);

        public static CompressedImage ConvertFrom(ImageData data)
        {
            return new CompressedImage()
            {
                header = new Header()
                {
                    stamp = Convert(data.Time),
                    frame_id = data.Frame,
                },
                format = "jpeg",
                data = new PartialByteArray()
                {
                    Array = data.Bytes,
                    Length = data.Length,
                },
            };
        }

        public static Detection2DArray ConvertFrom(Detected2DObjectData data)
        {
            return new Detection2DArray()
            {
                header = new Header()
                {
                    stamp = Convert(data.Time),
                    frame_id = data.Frame,
                },
                detections = data.Data.Select(d => new Detection2D()
                {
                    id = d.Id,
                    label = d.Label,
                    score = (float)d.Score,
                    bbox = new BoundingBox2D()
                    {
                        x = d.Position.x,
                        y = d.Position.y,
                        width = d.Scale.x,
                        height = d.Scale.y
                    },
                    velocity = new Twist()
                    {
                        linear = ConvertToVector(d.LinearVelocity),
                        angular = ConvertToVector(d.AngularVelocity),
                    }
                }).ToList(),
            };
        }

        public static Detected2DObjectArray ConvertTo(Detection2DArray data)
        {
            return new Detected2DObjectArray()
            {
                Data = data.detections.Select(obj =>
                    new Detected2DObject()
                    {
                        Id = obj.id,
                        Label = obj.label,
                        Score = obj.score,
                        Position = new UnityEngine.Vector2(obj.bbox.x, obj.bbox.y),
                        Scale = new UnityEngine.Vector2(obj.bbox.width, obj.bbox.height),
                        LinearVelocity = new UnityEngine.Vector3((float)obj.velocity.linear.x, 0, 0),
                        AngularVelocity = new UnityEngine.Vector3(0, 0, (float)obj.velocity.angular.z),
                    }).ToArray(),
            };
        }

        public static Detection3DArray ConvertFrom(Detected3DObjectData data)
        {
            return new Detection3DArray()
            {
                header = new Header()
                {
                    stamp = Convert(data.Time),
                    frame_id = data.Frame,
                },
                detections = data.Data.Select(d => new Detection3D()
                {
                    id = d.Id,
                    label = d.Label,
                    score = (float)d.Score,
                    bbox = new BoundingBox3D()
                    {
                        position = new Pose()
                        {
                            position = ConvertToPoint(d.Position),
                            orientation = Convert(d.Rotation),
                        },
                        size = ConvertToVector(d.Scale),
                    },
                    velocity = new Twist()
                    {
                        linear = ConvertToVector(d.LinearVelocity),
                        angular = ConvertToVector(d.AngularVelocity),
                    }
                }).ToList(),
            };
        }

        public static SignalArray ConvertFrom(SignalDataArray data)
        {
            return new SignalArray()
            {
                header = new Header()
                {
                    stamp = Convert(data.Time),
                    frame_id = data.Frame,
                },
                signals = data.Data.Select(d => new Signal()
                {
                    id = d.Id,
                    label = d.Label,
                    score = (float)d.Score,
                    bbox = new BoundingBox3D()
                    {
                        position = new Pose()
                        {
                            position = ConvertToPoint(d.Position),
                            orientation = Convert(d.Rotation),
                        },
                        size = ConvertToVector(d.Scale),
                    }
                }).ToList(),
            };
        }

        public static VehicleStateReport ConvertFrom(CanBusData data)
        {
            // No fuel supported in Simulator side
            byte fuel = 0;

            // Blinker
            // BLINKER_OFF = 0, BLINKER_LEFT = 1, BLINKER_RIGHT = 2, BLINKER_HAZARD = 3
            // No Hazard Light in Simulator side
            var blinker = data.HazardLights ? 3 : data.LeftTurnSignal ? 1 : data.RightTurnSignal ? 2 : 0;

            // Headlight
            // HEADLIGHT_OFF = 0, HEADLIGHT_ON = 1, HEADLIGHT_HIGH = 2
            var headlight = data.LowBeamSignal ? 1 : data.HighBeamSignal ? 2 : 0;

            // Wiper
            // WIPER_OFF = 0, WIPER_LOW = 1, WIPER_HIGH = 2, WIPER_CLEAN = 3
            // No WIPER_HIGH and WIPER_CLEAN in Simulator side
            var wiper = data.Wipers ? 1 : 0;

            // Gear
            // GEAR_DRIVE = 0, GEAR_REVERSE = 1, GEAR_PARK = 2, GEAR_LOW = 3, GEAR_NEUTRAL = 4
            // No GEAR_PARK, GEAR_LOW, GEAR_NEUTRAL in Simulator side
            var gear = data.InReverse ? 1 : 0;

            // Mode
            // No information about mode in Simulator side.
            byte mode = 0;

            // Hand Brake
            bool handBrake = false;

            // Horn
            bool horn = false;

            return new VehicleStateReport()
            {
                stamp = Convert(data.Time),
                fuel = fuel,
                blinker = (byte)blinker,
                headlight = (byte)headlight,
                wiper = (byte)wiper,
                gear = (byte)gear,
                mode = mode,
                hand_brake = handBrake,
                horn = horn,
            };
        }

        public static NavSatFix ConvertFrom(GpsData data)
        {
            return new NavSatFix()
            {
                header = new Header()
                {
                    stamp = Convert(data.Time),
                    frame_id = data.Frame,
                },
                status = new NavSatStatus()
                {
                    status = NavFixStatus.STATUS_FIX,
                    service = GpsServisType.SERVICE_GPS,
                },
                latitude = data.Latitude,
                longitude = data.Longitude,
                altitude = data.Altitude,

                position_covariance = new double[]
                {
                    0.0001, 0, 0,
                    0, 0.0001, 0,
                    0, 0, 0.0001,
                },

                position_covariance_type = CovarianceType.COVARIANCE_TYPE_DIAGONAL_KNOWN
            };
        }

        public static Odometry ConvertFrom(GpsOdometryData data)
        {
            return new Odometry()
            {
                header = new Header()
                {
                    stamp = Convert(data.Time),
                    frame_id = data.Frame,
                },
                child_frame_id = data.ChildFrame,
                pose = new PoseWithCovariance()
                {
                    pose = new Pose()
                    {
                        position = new Point()
                        {
                            x = data.Easting + (data.IgnoreMapOrigin ? -500000 : 0),
                            y = data.Northing,
                            z = data.Altitude,
                        },
                        orientation = Convert(data.Orientation),
                    }
                },
                twist = new TwistWithCovariance()
                {
                    twist = new Twist()
                    {
                        linear = new Vector3()
                        {
                            x = data.ForwardSpeed,
                            y = 0.0,
                            z = 0.0,
                        },
                        angular = new Vector3()
                        {
                            x = 0.0,
                            y = 0.0,
                            z = -data.AngularVelocity.y,
                        }
                    },
                }
            };
        }

        public static VehicleOdometry ConvertFrom(VehicleOdometryData data)
        {
            return new VehicleOdometry()
            {
                stamp = Convert(data.Time),
                velocity_mps = data.Speed,
                front_wheel_angle_rad = UnityEngine.Mathf.Deg2Rad * data.SteeringAngleFront,
                rear_wheel_angle_rad = UnityEngine.Mathf.Deg2Rad * data.SteeringAngleBack,
            };
        }

        public static Detected3DObjectArray ConvertTo(Detection3DArray data)
        {
            return new Detected3DObjectArray()
            {
                Data = data.detections.Select(obj =>
                    new Detected3DObject()
                    {
                        Id = obj.id,
                        Label = obj.label,
                        Score = obj.score,
                        Position = Convert(obj.bbox.position.position),
                        Rotation = Convert(obj.bbox.position.orientation),
                        Scale = Convert(obj.bbox.size),
                        LinearVelocity = Convert(obj.velocity.linear),
                        AngularVelocity = Convert(obj.velocity.angular),
                    }).ToArray(),
            };
        }

        public static VehicleControlData ConvertTo(RawControlCommand data)
        {
            return new VehicleControlData()
            {
                Acceleration = (float)data.throttle / 100,
                Breaking = (float)data.brake / 100,
                SteerAngle = (float)data.front_steer / 100,
            };
        }

        public static VehicleStateData ConvertTo(VehicleStateCommand data)
        {
            return new VehicleStateData()
            {
                Blinker = data.blinker,
                HeadLight = data.headlight,
                Wiper = data.wiper,
                Gear = data.gear,
                Mode = data.mode,
                HandBrake = data.hand_brake,
                Horn = data.horn,
            };
        }

        public static Imu ConvertFrom(ImuData data)
        {
            return new Imu()
            {
                header = new Header()
                {
                    stamp = Convert(data.Time),
                    frame_id = data.Frame,
                },

                orientation = Convert(data.Orientation),
                orientation_covariance = new double[9] { 0.0001, 0, 0, 0, 0.0001, 0, 0, 0, 0.0001 },
                angular_velocity = ConvertToVector(data.AngularVelocity),
                angular_velocity_covariance = new double[9] { 0.0001, 0, 0, 0, 0.0001, 0, 0, 0, 0.0001 },
                linear_acceleration = ConvertToVector(data.Acceleration),
                linear_acceleration_covariance = new double[9] { 0.0001, 0, 0, 0, 0.0001, 0, 0, 0, 0.0001 },
            };
        }

        public static Clock ConvertFrom(ClockData data)
        {
            return new Clock()
            {
                clock = Convert(data.Clock),
            };
        }

        static Point ConvertToPoint(UnityEngine.Vector3 v)
        {
            return new Point() { x = v.x, y = v.y, z = v.z };
        }

        static Point ConvertToPoint(double3 d)
        {
            return new Point() { x = d.x, y = d.y, z = d.z };
        }

        static Vector3 ConvertToVector(UnityEngine.Vector3 v)
        {
            return new Vector3() { x = v.x, y = v.y, z = v.z };
        }

        static Quaternion Convert(UnityEngine.Quaternion q)
        {
            return new Quaternion() { x = q.x, y = q.y, z = q.z, w = q.w };
        }

        static double3 Convert(Point p)
        {
            return new double3(p.x, p.y, p.z);
        }

        static UnityEngine.Vector3 Convert(Vector3 v)
        {
            return new UnityEngine.Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        static UnityEngine.Quaternion Convert(Quaternion q)
        {
            return new UnityEngine.Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
        }

        public static Time Convert(double unixEpochSeconds)
        {
            long nanosec = (long)(unixEpochSeconds * 1e9);

            return new Time()
            {
                secs = (int)(nanosec / 1000000000),
                nsecs = (uint)(nanosec % 1000000000),
            };
        }
    }
}
