﻿using System;
using System.Linq;
using System.Text.Json.Serialization;
using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.Utils;
using Il2CppInterop.Runtime;
using Il2CppSystem.Runtime.InteropServices;
namespace ExtraObjectiveSetup.JSON
{
    public static class InjectLibUtil
    {
        public const string PLUGIN_GUID = "GTFO.InjectLib";

        public static JsonConverter InjectLibConnector { get; private set; } = null;

        public static bool IsLoaded { get; private set; } = false;

        static InjectLibUtil()
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
            {
                try
                {
                    var ddAsm = info?.Instance?.GetType()?.Assembly ?? null;

                    if (ddAsm is null)
                        throw new Exception("Assembly is Missing!");

                    var types = ddAsm.GetTypes();
                    var converterType = types.First(t => t.Name == "InjectLibConnector");
                    if (converterType is null)
                        throw new Exception("Unable to Find InjectLibConnector Class");

                    InjectLibConnector = (JsonConverter)Activator.CreateInstance(converterType);
                    IsLoaded = true;
                }
                catch (Exception e)
                {
                    EOSLogger.Error($"Exception thrown while reading data from GTFO.AWO: {e}");
                }
            }
        }

    }
}
