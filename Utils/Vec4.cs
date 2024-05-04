﻿using System.Text.Json.Serialization;
using UnityEngine;

namespace ExtraObjectiveSetup.Utils
{
    public class Vec4: Vec3
    {
        [JsonPropertyOrder(-9)]
        public float w { get; set; } = 0;

        public Vector4 ToVector4() => new Vector4(x, y, z, w);

        public Vec4() { }
    }
}
