using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using System.Collections.Generic;
using LevelGeneration;
using System;
using System.Collections.Immutable;
using ChainedPuzzles;
using Il2CppSystem.Runtime.Remoting.Messaging;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using ExtraObjectiveSetup.Tweaks.TerminalTweak;

namespace ExtraObjectiveSetup.Instances
{
    public sealed class TerminalInstanceManager: InstanceManager<LG_ComputerTerminal>
    {
        public enum TerminalWardenEvents 
        { 
            EOSSetTerminalCommand = 600,
            EOSToggleTerminalState = 601,
        }


        public static TerminalInstanceManager Current { get; private set; } = new();

        public static ImmutableList<TERM_Command> UNIQUE_CMDS { get; } = new List<TERM_Command>() {
            TERM_Command.UniqueCommand1,
            TERM_Command.UniqueCommand2,
            TERM_Command.UniqueCommand3,
            TERM_Command.UniqueCommand4,
            TERM_Command.UniqueCommand5,
        }.ToImmutableList();


        // key: ChainedPuzzleInstance.Pointer
        private Dictionary<IntPtr, LG_ComputerTerminal> UniqueCommandChainPuzzles { get; } = new();

        private Dictionary<IntPtr, TerminalWrapper> Wrappers { get; } = new();

        public override (eDimensionIndex dim, LG_LayerType layer, eLocalZoneIndex zone) GetGlobalZoneIndex(LG_ComputerTerminal instance)
        {
            if(instance.SpawnNode == null)
            {
                if(instance.ConnectedReactor != null)
                {
                    var node = instance.ConnectedReactor.SpawnNode;
                    return (node.m_dimension.DimensionIndex, node.LayerType, node.m_zone.LocalIndex);
                }
                else
                {
                    throw new ArgumentException("LG_ComputerTerminal: both SpawnNode and ConnectedReactor are null!");
                }
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

        public void SetupTerminalWrapper(LG_ComputerTerminal terminal)
        {
            if(Wrappers.ContainsKey(terminal.Pointer))
            {
                EOSLogger.Error($"TerminalInstanceManager: {terminal.ItemKey} is already setup with wrapper...");
                return;
            }
                
            uint allotedID = EOSNetworking.AllotReplicatorID();
            if (allotedID == EOSNetworking.INVALID_ID)
            {
                EOSLogger.Error($"TerminalStateManager: replicator ID depleted, cannot setup terminal...");
                return;
            }

            TerminalWrapper t = TerminalWrapper.Instantiate(terminal, allotedID);
            Wrappers[terminal.Pointer] = t;
        }

        public TerminalWrapper GetTerminalWrapper(LG_ComputerTerminal terminal) => Wrappers.ContainsKey(terminal.Pointer) ? Wrappers[terminal.Pointer] : null;

        private void Clear()
        {
            UniqueCommandChainPuzzles.Clear();
            Wrappers.Clear();
        }

        private void GatherUniqueCommandChainPuzzles()
        {
            foreach (var terminalsInZone in index2Instance.Values) 
            {
                foreach(var t in terminalsInZone)
                {
                    foreach(var cmd in UNIQUE_CMDS)
                    {
                        if (!t.m_command.m_commandsPerEnum.ContainsKey(cmd)) continue;
                        var cmdName = t.m_command.m_commandsPerEnum[cmd];
                        var events = t.GetUniqueCommandEvents(cmdName);

                        for(int i = 0; i < events.Count; i++)
                        {
                            if(t.TryGetChainPuzzleForCommand(cmd, i, out var cp))
                            {
                                UniqueCommandChainPuzzles[cp.Pointer] = t;
                            }
                        }
                    }
                }
            }
        }

        public bool TryGetParentTerminal(ChainedPuzzleInstance cpInstance, out LG_ComputerTerminal terminal) => UniqueCommandChainPuzzles.TryGetValue(cpInstance.Pointer, out terminal);

        public bool TryGetParentTerminal(IntPtr pointer, out LG_ComputerTerminal terminal) => UniqueCommandChainPuzzles.TryGetValue(pointer, out terminal);

        public void SetTerminalCommand(WardenObjectiveEventData e)
        {
            LG_LayerType layer = e.Layer;
            eLocalZoneIndex localIndex = e.LocalIndex;
            eDimensionIndex dimensionIndex = e.DimensionIndex;
            LG_ComputerTerminal terminal = GetInstance(dimensionIndex, layer, localIndex, (uint)e.Count);
            if (terminal == null)
            {
                EOSLogger.Error($"SetTerminalCommand_Custom: Cannot find reactor for {layer} or instance index ({(dimensionIndex, layer, localIndex, e.Count)})");
                return;
            }

            if (e.Enabled == true)
            {
                terminal.TrySyncSetCommandShow(e.TerminalCommand);
            }
            else
            {
                terminal.TrySyncSetCommandHidden(e.TerminalCommand);
            }

            EOSLogger.Debug($"SetTerminalCommand: Terminal_{terminal.m_serialNumber}, command '{e.TerminalCommand}' enabled ? {e.Enabled}");
        }

        public void ToggleTerminalState(WardenObjectiveEventData e)
        {
            bool enabled = e.Enabled;

            var terminal = GetInstance(e.DimensionIndex, e.Layer, e.LocalIndex, (uint)e.Count);
            if (terminal == null)
            {
                EOSLogger.Error($"ToggleTerminalState: terminal with index {(e.DimensionIndex, e.Layer, e.LocalIndex, e.Count)} not found");
                return;
            }

            var wrapper = GetTerminalWrapper(terminal);
            if (wrapper == null)
            {
                EOSLogger.Error($"ToggleTerminalState: internal error: terminal wrapper not found - {(e.DimensionIndex, e.Layer, e.LocalIndex, e.Count)}");
                return;
            }

            wrapper.ChangeState(enabled);
        }

        private TerminalInstanceManager() 
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;

            LevelAPI.OnEnterLevel += GatherUniqueCommandChainPuzzles;

            EOSWardenEventManager.Current.AddEventDefinition(TerminalWardenEvents.EOSSetTerminalCommand.ToString(), (uint)TerminalWardenEvents.EOSSetTerminalCommand, SetTerminalCommand);
            EOSWardenEventManager.Current.AddEventDefinition(TerminalWardenEvents.EOSToggleTerminalState.ToString(), (uint)TerminalWardenEvents.EOSToggleTerminalState, ToggleTerminalState);
        }
    
        static TerminalInstanceManager() { }
    }
}
