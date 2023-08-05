using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using LevelGeneration;
using System;

namespace ExtraObjectiveSetup.Instances
{
    public sealed class TerminalInstanceManager: InstanceManager<LG_ComputerTerminal>
    {
        public static TerminalInstanceManager Current { get; private set; } = new();

        public override (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GetGlobalZoneIndex(LG_ComputerTerminal instance)
        {
            if(instance.SpawnNode == null)
            {
                throw new ArgumentException("LG_ComputerTerminal.SpawnNode == null! This is a reactor terminal.");
            }

            return (instance.SpawnNode.m_dimension.DimensionIndex, instance.SpawnNode.LayerType, instance.SpawnNode.m_zone.LocalIndex);
        }

        public override uint Register(LG_ComputerTerminal instance)
        {
            if (instance.SpawnNode == null)
            {
                EOSLogger.Error("Trying to register reactor terminal. Use TerminalInstanceManager.RegisterReactorTerminal instead.");
                return INVALID_INSTANCE_INDEX;
            }

            else return Register(GetGlobalZoneIndex(instance), instance);
        }

        /// <summary>
        /// Register reactor terminal, whose SpawnNode is null
        /// This method is only called in EOSExt.Reactor
        /// </summary>
        /// <param name="reactor"></param>
        /// <returns></returns>
        public uint RegisterReactorTerminal(LG_WardenObjective_Reactor reactor)
        {
            if(reactor.m_terminal == null)
            {
                EOSLogger.Error($"RegisterReactorTerminal: reactor has no terminal");
                return INVALID_INSTANCE_INDEX;
            }

            uint index = Register((reactor.SpawnNode.m_dimension.DimensionIndex, reactor.SpawnNode.LayerType, reactor.SpawnNode.m_zone.LocalIndex), reactor.m_terminal);
            EOSLogger.Debug($"Registered reactor terminal, {reactor.PublicName} - {reactor.m_terminal.PublicName}, instance index: {index}");
            return index;
        }

        private void Clear()
        {

        }

        private TerminalInstanceManager() 
        {
            LevelAPI.OnLevelCleanup += Clear;
        }
    
        static TerminalInstanceManager() { }
    }
}
