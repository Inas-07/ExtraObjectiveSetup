﻿using System.Text.Json.Serialization;
using UnityEngine;

namespace ExtraObjectiveSetup.Utils
{
    public class Vec3
    {
        [JsonPropertyOrder(-10)]
        public float x { get; set; }

        [JsonPropertyOrder(-10)]
        public float y { get; set; }

        [JsonPropertyOrder(-10)]
        public float z { get; set; }

        public Vector3 ToVector3() => new Vector3(x, y, z);

        public Quaternion ToQuaternion() => Quaternion.Euler(x, y, z);

        public Vec3() { }
    }
}
