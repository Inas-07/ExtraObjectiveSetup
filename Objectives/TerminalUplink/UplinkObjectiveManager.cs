using GTFO.API;
using ExtraObjectiveSetup.Utils;
using System.Collections.Generic;
using LevelGeneration;
using ChainedPuzzles;
using GameData;
using Localization;
using System;
using GTFO.API.Extensions;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Objectives.TerminalUplink
{
    internal sealed class UplinkObjectiveManager: InstanceDefinitionManager<UplinkDefinition>
    {
        public static UplinkObjectiveManager Current { get; private set; } = new();

        private TextDataBlock UplinkAddrLogContentBlock = null;

        protected override string DEFINITION_NAME { get; } = "TerminalUplink";

        protected override void AddDefinitions(InstanceDefinitionsForLevel<UplinkDefinition> definitions)
        {
            // because we have chained puzzles, sorting is necessary to preserve chained puzzle instance order.
            Sort(definitions);
            definitions.Definitions.ForEach(u => u.RoundOverrides.Sort((r1, r2) => r1.RoundIndex != r2.RoundIndex ? (r1.RoundIndex < r2.RoundIndex ? -1 : 1) : 0));

            base.AddDefinitions(definitions);
        }

        private void Build(UplinkDefinition def)
        {
            LG_ComputerTerminal uplinkTerminal = TerminalInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);

            if(uplinkTerminal == null) return;

            if (uplinkTerminal.m_isWardenObjective && uplinkTerminal.UplinkPuzzle != null)
            {
                EOSLogger.Error($"BuildUplink: Uplink already built by vanilla, aborting custom build!");
                return;
            }

            if (def.SetupAsCorruptedUplink)
            {
                LG_ComputerTerminal receiver = TerminalInstanceManager.Current.GetInstance(
                    def.CorruptedUplinkReceiver.DimensionIndex,
                    def.CorruptedUplinkReceiver.LayerType,
                    def.CorruptedUplinkReceiver.LocalIndex,
                    def.CorruptedUplinkReceiver.InstanceIndex);

                if (receiver == null)
                {
                    EOSLogger.Error("BuildUplink: SetupAsCorruptedUplink specified but didn't find the receiver terminal, will fall back to normal uplink instead");
                    return;
                }

                if (receiver.Pointer == uplinkTerminal.Pointer)
                {
                    EOSLogger.Error("BuildUplink: Don't specify uplink sender and receiver on the same terminal");
                    return;
                }

                uplinkTerminal.CorruptedUplinkReceiver = receiver;
                receiver.CorruptedUplinkReceiver = uplinkTerminal; // need to set on both side
            }

            uplinkTerminal.UplinkPuzzle = new TerminalUplinkPuzzle();
            SetupUplinkPuzzle(uplinkTerminal.UplinkPuzzle, uplinkTerminal, def);
            uplinkTerminal.UplinkPuzzle.OnPuzzleSolved += new Action(() =>
            {
                def.EventsOnComplete?.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
            });


            uplinkTerminal.m_command.AddCommand(
                uplinkTerminal.CorruptedUplinkReceiver == null ? TERM_Command.TerminalUplinkConnect : TERM_Command.TerminalCorruptedUplinkConnect, 
                def.UseUplinkAddress ? "UPLINK_CONNECT" : "UPLINK_ESTABLISH", 
                new LocalizedText() {
                UntranslatedText = Text.Get(3914968919),
                Id = 3914968919
                });

            uplinkTerminal.m_command.AddCommand(TERM_Command.TerminalUplinkVerify, "UPLINK_VERIFY", new LocalizedText()
            {
                UntranslatedText = Text.Get(1728022075),
                Id = 1728022075
            });

            if (def.UseUplinkAddress)
            {
                LG_ComputerTerminal addressLogTerminal = null;

                EOSLogger.Debug($"BuildUplinkOverride: UseUplinkAddress");
                addressLogTerminal = TerminalInstanceManager.Current.GetInstance(def.UplinkAddressLogPosition.DimensionIndex, def.UplinkAddressLogPosition.LayerType, def.UplinkAddressLogPosition.LocalIndex, def.UplinkAddressLogPosition.InstanceIndex);
                if (addressLogTerminal == null)
                {
                    EOSLogger.Error($"BuildUplinkOverride: didn't find the terminal to put the uplink address log, will put on uplink terminal");
                    addressLogTerminal = uplinkTerminal;
                }

                addressLogTerminal.AddLocalLog(new TerminalLogFileData()
                {
                    FileName = $"UPLINK_ADDR_{uplinkTerminal.m_serialNumber}.LOG",
                    FileContent = new LocalizedText() 
                    { 
                        UntranslatedText = string.Format(UplinkAddrLogContentBlock != null ? Text.Get(UplinkAddrLogContentBlock.persistentID) : "Available uplink address for TERMINAL_{0}: {1}", uplinkTerminal.m_serialNumber, uplinkTerminal.UplinkPuzzle.TerminalUplinkIP),
                        Id = 0
                    }
                }, true);

                addressLogTerminal.m_command.ClearOutputQueueAndScreenBuffer();
                addressLogTerminal.m_command.AddInitialTerminalOutput();
            }

            if (def.ChainedPuzzleToStartUplink != 0)
            {
                var block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(def.ChainedPuzzleToStartUplink);
                if (block == null)
                {
                    EOSLogger.Error($"BuildTerminalUplink: ChainedPuzzleToActive with id {def.ChainedPuzzleToStartUplink} is specified but no ChainedPuzzleDataBlock definition is found... Won't build");
                    uplinkTerminal.m_chainPuzzleForWardenObjective = null;
                }
                else
                {
                    uplinkTerminal.m_chainPuzzleForWardenObjective = ChainedPuzzleManager.CreatePuzzleInstance(
                        block,
                        uplinkTerminal.SpawnNode.m_area,
                        uplinkTerminal.m_wardenObjectiveSecurityScanAlign.position,
                        uplinkTerminal.m_wardenObjectiveSecurityScanAlign);
                }
            }

            foreach(var roundOverride in def.RoundOverrides)
            {
                if(roundOverride.ChainedPuzzleToEndRound != 0u)
                {
                    var block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(roundOverride.ChainedPuzzleToEndRound);
                    if(block != null)
                    {
                        LG_ComputerTerminal t = null;
                        switch (roundOverride.BuildChainedPuzzleOn)
                        {
                            case UplinkTerminal.SENDER: t = uplinkTerminal; break;
                            case UplinkTerminal.RECEIVER: 
                                if(def.SetupAsCorruptedUplink && uplinkTerminal.CorruptedUplinkReceiver != null)
                                {
                                    t = uplinkTerminal.CorruptedUplinkReceiver;
                                }
                                else
                                {
                                    EOSLogger.Error($"ChainedPuzzleToEndRound: {roundOverride.ChainedPuzzleToEndRound} specified to build on receiver but this is not a properly setup-ed corr-uplink! Will build ChainedPuzzle on sender side");
                                    t = uplinkTerminal;
                                }
                                break;

                            default: EOSLogger.Error($"Unimplemented enum UplinkTerminal type {roundOverride.BuildChainedPuzzleOn}"); continue;
                        }

                        roundOverride.ChainedPuzzleToEndRoundInstance = ChainedPuzzleManager.CreatePuzzleInstance(
                            block,
                            t.SpawnNode.m_area,
                            t.m_wardenObjectiveSecurityScanAlign.position,
                            t.m_wardenObjectiveSecurityScanAlign);
                    }
                    else
                    {
                        EOSLogger.Error($"ChainedPuzzleToEndRound: {roundOverride.ChainedPuzzleToEndRound} specified but didn't find its ChainedPuzzleDatablock definition! Will not build!");
                    }
                }
            }

            EOSLogger.Debug($"BuildUplink: built on {(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex)}");
        }
        
        private void SetupUplinkPuzzle(TerminalUplinkPuzzle uplinkPuzzle, LG_ComputerTerminal terminal, UplinkDefinition def)
        {
            uplinkPuzzle.m_rounds = new List<TerminalUplinkPuzzleRound>().ToIl2Cpp();
            uplinkPuzzle.TerminalUplinkIP = SerialGenerator.GetIpAddress();
            uplinkPuzzle.m_roundIndex = 0;
            uplinkPuzzle.m_lastRoundIndexToUpdateGui = -1;
            uplinkPuzzle.m_position = terminal.transform.position;
            uplinkPuzzle.IsCorrupted = def.SetupAsCorruptedUplink && terminal.CorruptedUplinkReceiver != null;
            uplinkPuzzle.m_terminal = terminal;
            uint verificationRound = Math.Max(def.NumberOfVerificationRounds, 1u);
            for (int i = 0; i < verificationRound; ++i)
            {
                int candidateWords = 6;
                TerminalUplinkPuzzleRound uplinkPuzzleRound = new TerminalUplinkPuzzleRound()
                {
                    CorrectIndex = Builder.SessionSeedRandom.Range(0, candidateWords, "NO_TAG"),
                    Prefixes = new string[candidateWords],
                    Codes = new string[candidateWords]
                };

                for (int j = 0; j < candidateWords; ++j)
                {
                    uplinkPuzzleRound.Codes[j] = SerialGenerator.GetCodeWord();
                    uplinkPuzzleRound.Prefixes[j] = SerialGenerator.GetCodeWordPrefix();
                }
                uplinkPuzzle.m_rounds.Add(uplinkPuzzleRound);
            }
        }

        private void ClearChainedPuzzleInstance(UplinkDefinition def)
        {
            def.RoundOverrides.ForEach(r => r.ChainedPuzzleToEndRoundInstance = null);
        }

        private void OnBuildDone()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            if (UplinkAddrLogContentBlock == null)
            {
                UplinkAddrLogContentBlock = GameDataBlockBase<TextDataBlock>.GetBlock("InGame.UplinkTerminal.UplinkAddrLog");
            }
            definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(Build);
        }

        private void OnLevelCleanup()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(ClearChainedPuzzleInstance);
        }

        static UplinkObjectiveManager()
        {

        }

        private UplinkObjectiveManager() : base() 
        {
            //BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.FinalLogicLinking, OnBuildDone);
            LevelAPI.OnBuildDone += OnBuildDone;
            LevelAPI.OnLevelCleanup += OnLevelCleanup;
            LevelAPI.OnBuildStart += OnLevelCleanup;
        }
    }
}